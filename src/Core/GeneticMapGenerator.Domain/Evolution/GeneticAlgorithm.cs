using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Evolution.Evaluation;
using GeneticMapGenerator.Domain.Evolution.Selection;
using GeneticMapGenerator.Domain.Evolution.Termination;

namespace GeneticMapGenerator.Domain.Evolution
{
    /// <summary>One evaluated generation, as streamed out of <see cref="GeneticAlgorithm{TGenome}.Evolve"/>.</summary>
    public sealed class GenerationReport<TGenome>
    {
        public GenerationReport(Population<TGenome> population, string? terminationReason)
        {
            Population = population;
            TerminationReason = terminationReason;
        }

        public Population<TGenome> Population { get; }

        public int Generation => Population.Generation;

        public Scored<TGenome> Best => Population.Best;

        /// <summary>Set on the final report only.</summary>
        public string? TerminationReason { get; }

        public bool IsFinal => TerminationReason != null;
    }

    /// <summary>
    /// Generational GA with elitism. Problem-specific behaviour is supplied as strategies, so the
    /// same kernel evolves terrain and settlements. Build instances with <see cref="GeneticAlgorithmBuilder{TGenome}"/>.
    /// </summary>
    public sealed class GeneticAlgorithm<TGenome>
    {
        private readonly EvolutionSettings _settings;
        private readonly IGenomeFactory<TGenome> _genomeFactory;
        private readonly ICrossoverOperator<TGenome> _crossover;
        private readonly IMutationOperator<TGenome> _mutation;
        private readonly IFitnessFunction<TGenome> _fitnessFunction;
        private readonly ISelectionStrategy _selection;
        private readonly ITerminationCondition _termination;
        private readonly IFitnessEvaluator _evaluator;

        internal GeneticAlgorithm(
            EvolutionSettings settings,
            IGenomeFactory<TGenome> genomeFactory,
            ICrossoverOperator<TGenome> crossover,
            IMutationOperator<TGenome> mutation,
            IFitnessFunction<TGenome> fitnessFunction,
            ISelectionStrategy selection,
            ITerminationCondition termination,
            IFitnessEvaluator evaluator)
        {
            _settings = settings;
            _genomeFactory = genomeFactory;
            _crossover = crossover;
            _mutation = mutation;
            _fitnessFunction = fitnessFunction;
            _selection = selection;
            _termination = termination;
            _evaluator = evaluator;
        }

        /// <summary>
        /// Lazily streams one report per generation, starting with the random initial population
        /// (generation 0) and ending with a report whose <see cref="GenerationReport{TGenome}.IsFinal"/> is set.
        /// Callers control pacing simply by how fast they enumerate.
        /// </summary>
        public IEnumerable<GenerationReport<TGenome>> Evolve(CancellationToken cancellationToken = default)
        {
            var random = new SplitMix64Random(_settings.Seed);
            var population = new Population<TGenome>(0, Evaluate(CreateInitialGenomes(random), cancellationToken));
            var bestScore = population.Best.Fitness.Score;
            var generationsWithoutImprovement = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var progress = new EvolutionProgress(population.Generation, population.Best.Fitness.Score, generationsWithoutImprovement);
                if (_termination.IsSatisfiedBy(progress, out var reason))
                {
                    yield return new GenerationReport<TGenome>(population, reason);
                    yield break;
                }

                yield return new GenerationReport<TGenome>(population, null);

                population = Breed(population, random, cancellationToken);
                if (population.Best.Fitness.Score > bestScore)
                {
                    bestScore = population.Best.Fitness.Score;
                    generationsWithoutImprovement = 0;
                }
                else
                {
                    generationsWithoutImprovement++;
                }
            }
        }

        private IReadOnlyList<TGenome> CreateInitialGenomes(IRandomSource random) =>
            Enumerable.Range(0, _settings.PopulationSize).Select(_ => _genomeFactory.Create(random)).ToArray();

        private Population<TGenome> Breed(Population<TGenome> parents, IRandomSource random, CancellationToken cancellationToken)
        {
            var elites = parents.Ranked.Take(_settings.EliteCount);
            var offspringCount = _settings.PopulationSize - _settings.EliteCount;
            var offspring = new List<TGenome>(offspringCount);

            while (offspring.Count < offspringCount)
            {
                var (mother, father) = _selection.SelectParents(parents, random);
                var (first, second) = random.Chance(_settings.CrossoverRate)
                    ? _crossover.Cross(mother.Genome, father.Genome, random)
                    : (mother.Genome, father.Genome);

                offspring.Add(_mutation.Mutate(first, _settings.MutationRate, random));
                if (offspring.Count < offspringCount)
                {
                    offspring.Add(_mutation.Mutate(second, _settings.MutationRate, random));
                }
            }

            // Elites keep their existing scores; only new genomes are evaluated.
            return new Population<TGenome>(parents.Generation + 1, elites.Concat(Evaluate(offspring, cancellationToken)));
        }

        private IReadOnlyList<Scored<TGenome>> Evaluate(IReadOnlyList<TGenome> genomes, CancellationToken cancellationToken) =>
            _evaluator.Evaluate(genomes, _fitnessFunction, cancellationToken);
    }
}
