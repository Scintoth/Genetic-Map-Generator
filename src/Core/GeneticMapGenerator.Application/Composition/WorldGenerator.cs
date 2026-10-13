using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Evolution.Evaluation;
using GeneticMapGenerator.Domain.Scoring;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Application.Composition
{
    public sealed record WorldGeneratorOptions
    {
        /// <summary>Evaluate each generation across all cores. Disable for single-threaded hosts.</summary>
        public bool ParallelEvaluation { get; init; } = true;

        public SettlementPlannerSettings Planner { get; init; } = new SettlementPlannerSettings();
    }

    /// <summary>
    /// Composition root for the core. Hosts call this instead of wiring domain services themselves,
    /// so adapters depend only on the ports.
    /// </summary>
    public static class WorldGenerator
    {
        public static IGenerateWorldUseCase Create(WorldGeneratorOptions? options = null)
        {
            options ??= new WorldGeneratorOptions();

            return new GenerateWorldUseCase(
                new DefaultTerrainBuilderFactory(),
                new OrganicSettlementPlanner(options.Planner),
                options.ParallelEvaluation ? new ParallelFitnessEvaluator() : new SequentialFitnessEvaluator(),
                new HyperbolicToleranceCurve());
        }
    }
}
