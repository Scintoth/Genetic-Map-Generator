using System;
using GeneticMapGenerator.Domain.Evolution.Evaluation;
using GeneticMapGenerator.Domain.Evolution.Selection;
using GeneticMapGenerator.Domain.Evolution.Termination;

namespace GeneticMapGenerator.Domain.Evolution
{
    /// <summary>
    /// Fluent builder for <see cref="GeneticAlgorithm{TGenome}"/>. Problem operators are required;
    /// selection, termination and evaluation default from <see cref="EvolutionSettings"/>.
    /// </summary>
    public sealed class GeneticAlgorithmBuilder<TGenome>
    {
        private EvolutionSettings _settings = new EvolutionSettings();
        private IGenomeFactory<TGenome>? _genomeFactory;
        private ICrossoverOperator<TGenome>? _crossover;
        private IMutationOperator<TGenome>? _mutation;
        private IFitnessFunction<TGenome>? _fitnessFunction;
        private ISelectionStrategy? _selection;
        private ITerminationCondition? _termination;
        private IFitnessEvaluator _evaluator = new ParallelFitnessEvaluator();

        public GeneticAlgorithmBuilder<TGenome> WithSettings(EvolutionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            return this;
        }

        public GeneticAlgorithmBuilder<TGenome> WithGenomeFactory(IGenomeFactory<TGenome> genomeFactory)
        {
            _genomeFactory = genomeFactory;
            return this;
        }

        public GeneticAlgorithmBuilder<TGenome> WithCrossover(ICrossoverOperator<TGenome> crossover)
        {
            _crossover = crossover;
            return this;
        }

        public GeneticAlgorithmBuilder<TGenome> WithMutation(IMutationOperator<TGenome> mutation)
        {
            _mutation = mutation;
            return this;
        }

        /// <summary>Convenience for genome types whose operators live on one object.</summary>
        public GeneticAlgorithmBuilder<TGenome> WithOperators<TOperators>(TOperators operators)
            where TOperators : IGenomeFactory<TGenome>, ICrossoverOperator<TGenome>, IMutationOperator<TGenome> =>
            WithGenomeFactory(operators).WithCrossover(operators).WithMutation(operators);

        public GeneticAlgorithmBuilder<TGenome> WithFitnessFunction(IFitnessFunction<TGenome> fitnessFunction)
        {
            _fitnessFunction = fitnessFunction;
            return this;
        }

        public GeneticAlgorithmBuilder<TGenome> WithSelection(ISelectionStrategy selection)
        {
            _selection = selection;
            return this;
        }

        public GeneticAlgorithmBuilder<TGenome> WithTermination(ITerminationCondition termination)
        {
            _termination = termination;
            return this;
        }

        public GeneticAlgorithmBuilder<TGenome> WithEvaluator(IFitnessEvaluator evaluator)
        {
            _evaluator = evaluator;
            return this;
        }

        public GeneticAlgorithm<TGenome> Build()
        {
            _settings.Validate();

            return new GeneticAlgorithm<TGenome>(
                _settings,
                _genomeFactory ?? throw Missing("genome factory"),
                _crossover ?? throw Missing("crossover operator"),
                _mutation ?? throw Missing("mutation operator"),
                _fitnessFunction ?? throw Missing("fitness function"),
                _selection ?? SelectionStrategyFactory.Create(_settings.Selection),
                _termination ?? AnyTerminationCondition.FromSettings(_settings),
                _evaluator);
        }

        private static InvalidOperationException Missing(string dependency) =>
            new InvalidOperationException($"A {dependency} must be supplied before building the genetic algorithm.");
    }
}
