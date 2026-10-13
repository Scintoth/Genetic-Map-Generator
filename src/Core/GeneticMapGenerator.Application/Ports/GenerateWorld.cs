using System.Threading;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Application.Ports
{
    /// <summary>
    /// Inbound (driving) port: evolve a terrain to the requested biome mix and peak count, then evolve a
    /// settlement on it. Blocking and CPU-bound — hosts decide which thread it runs on.
    /// </summary>
    public interface IGenerateWorldUseCase
    {
        WorldGenerationResult Execute(GenerateWorldCommand command, IWorldGenerationObserver observer, CancellationToken cancellationToken = default);
    }

    public sealed record GenerateWorldCommand
    {
        public TerrainTargets TerrainTargets { get; init; } = TerrainTargets.Default;

        public SettlementTargets SettlementTargets { get; init; } = SettlementTargets.Default;

        public TerrainGridSettings Grid { get; init; } = new TerrainGridSettings();

        public TerrainAnalysisSettings Analysis { get; init; } = new TerrainAnalysisSettings();

        public EvolutionSettings TerrainEvolution { get; init; } = new EvolutionSettings
        {
            PopulationSize = 40,
            MaxGenerations = 80,
            TargetFitness = 0.95,
        };

        public EvolutionSettings SettlementEvolution { get; init; } = new EvolutionSettings
        {
            PopulationSize = 40,
            MaxGenerations = 60,
            TargetFitness = 0.97,
        };

        /// <summary>Seeds both stages from one value so a whole world is reproducible from a single number.</summary>
        public GenerateWorldCommand WithSeed(ulong seed) => this with
        {
            TerrainEvolution = TerrainEvolution with { Seed = seed },
            SettlementEvolution = SettlementEvolution with { Seed = unchecked(seed * 0x9E3779B97F4A7C15UL + 1) },
        };

        public void Validate()
        {
            Grid.Validate();
            TerrainEvolution.Validate();
            SettlementEvolution.Validate();
        }
    }
}
