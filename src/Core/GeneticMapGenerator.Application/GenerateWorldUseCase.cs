using System;
using System.Collections.Generic;
using System.Threading;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Evolution.Evaluation;
using GeneticMapGenerator.Domain.Scoring;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Settlements.Genetics;
using GeneticMapGenerator.Domain.Terrains;
using GeneticMapGenerator.Domain.Terrains.Genetics;

namespace GeneticMapGenerator.Application
{
    /// <summary>
    /// The two-stage pipeline from the dissertation: evolve terrain, then evolve a settlement on the
    /// winning terrain. Each generation's best individual is expressed and pushed to the observer.
    /// </summary>
    public sealed class GenerateWorldUseCase : IGenerateWorldUseCase
    {
        private readonly ITerrainBuilderFactory _terrainBuilderFactory;
        private readonly ISettlementPlanner _settlementPlanner;
        private readonly IFitnessEvaluator _fitnessEvaluator;
        private readonly IToleranceCurve _toleranceCurve;

        public GenerateWorldUseCase(
            ITerrainBuilderFactory terrainBuilderFactory,
            ISettlementPlanner settlementPlanner,
            IFitnessEvaluator fitnessEvaluator,
            IToleranceCurve toleranceCurve)
        {
            _terrainBuilderFactory = terrainBuilderFactory;
            _settlementPlanner = settlementPlanner;
            _fitnessEvaluator = fitnessEvaluator;
            _toleranceCurve = toleranceCurve;
        }

        public WorldGenerationResult Execute(GenerateWorldCommand command, IWorldGenerationObserver observer, CancellationToken cancellationToken = default)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            command.Validate();

            var terrainBuilder = _terrainBuilderFactory.Create(command.Grid, command.Analysis);
            var terrainStage = Evolve(
                command.TerrainEvolution,
                new TerrainGeneticOperators(),
                new TerrainFitnessFunction(terrainBuilder, command.TerrainTargets, _toleranceCurve),
                terrainBuilder.Build,
                observer.OnTerrainProgress,
                cancellationToken);

            var terrain = terrainStage.Best;
            var settlementStage = Evolve(
                command.SettlementEvolution,
                new SettlementGeneticOperators(),
                new SettlementFitnessFunction(terrain, _settlementPlanner, command.SettlementTargets, _toleranceCurve),
                genome => _settlementPlanner.Plan(genome, terrain),
                observer.OnSettlementProgress,
                cancellationToken);

            var result = new WorldGenerationResult(
                terrain,
                terrainStage.Fitness,
                terrainStage.History,
                settlementStage.Best,
                settlementStage.Fitness,
                settlementStage.History);

            observer.OnCompleted(result);
            return result;
        }

        private StageResult<TPhenotype> Evolve<TGenome, TPhenotype, TOperators>(
            EvolutionSettings settings,
            TOperators operators,
            IFitnessFunction<TGenome> fitnessFunction,
            Func<TGenome, TPhenotype> express,
            Action<EvolutionSnapshot<TPhenotype>> report,
            CancellationToken cancellationToken)
            where TGenome : class
            where TPhenotype : class
            where TOperators : IGenomeFactory<TGenome>, ICrossoverOperator<TGenome>, IMutationOperator<TGenome>
        {
            var algorithm = new GeneticAlgorithmBuilder<TGenome>()
                .WithSettings(settings)
                .WithOperators(operators)
                .WithFitnessFunction(fitnessFunction)
                .WithEvaluator(_fitnessEvaluator)
                .Build();

            var history = new List<GenerationStatistics>();
            TGenome? expressedGenome = null;
            TPhenotype? phenotype = null;

            foreach (var generation in algorithm.Evolve(cancellationToken))
            {
                var best = generation.Best;
                var statistics = new GenerationStatistics(generation.Generation, best.Fitness.Score, generation.Population.AverageScore);
                history.Add(statistics);

                // Elitism keeps the same champion for many generations; only re-express when it changes.
                if (phenotype is null || !Equals(best.Genome, expressedGenome))
                {
                    expressedGenome = best.Genome;
                    phenotype = express(best.Genome);
                }

                report(new EvolutionSnapshot<TPhenotype>(statistics, settings.MaxGenerations, best.Fitness, phenotype, generation.TerminationReason));

                if (generation.IsFinal)
                {
                    return new StageResult<TPhenotype>(phenotype, best.Fitness, history);
                }
            }

            throw new InvalidOperationException("Evolution ended without reporting a final generation.");
        }

        private sealed class StageResult<TPhenotype>
        {
            public StageResult(TPhenotype best, Fitness fitness, IReadOnlyList<GenerationStatistics> history)
            {
                Best = best;
                Fitness = fitness;
                History = history;
            }

            public TPhenotype Best { get; }

            public Fitness Fitness { get; }

            public IReadOnlyList<GenerationStatistics> History { get; }
        }
    }
}
