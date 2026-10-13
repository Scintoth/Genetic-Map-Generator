using System.Collections.Generic;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Application.Ports
{
    /// <summary>
    /// Outbound (driven) port through which hosts watch a world being generated: a console logger,
    /// an image writer, a Unity renderer. Called on the generating thread — adapters that need a
    /// specific thread must marshal themselves.
    /// </summary>
    public interface IWorldGenerationObserver
    {
        void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot);

        void OnSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot);

        void OnCompleted(WorldGenerationResult result);
    }

    /// <summary>The state of a running evolution after one generation, with its best individual expressed.</summary>
    public sealed class EvolutionSnapshot<TPhenotype>
    {
        public EvolutionSnapshot(GenerationStatistics statistics, int maxGenerations, Fitness bestFitness, TPhenotype best, string? terminationReason)
        {
            Statistics = statistics;
            MaxGenerations = maxGenerations;
            BestFitness = bestFitness;
            Best = best;
            TerminationReason = terminationReason;
        }

        public GenerationStatistics Statistics { get; }

        public int Generation => Statistics.Generation;

        public int MaxGenerations { get; }

        public Fitness BestFitness { get; }

        public TPhenotype Best { get; }

        public string? TerminationReason { get; }

        public bool IsFinal => TerminationReason != null;
    }

    public sealed record GenerationStatistics(int Generation, double BestScore, double AverageScore);

    public sealed class WorldGenerationResult
    {
        public WorldGenerationResult(
            Terrain terrain,
            Fitness terrainFitness,
            IReadOnlyList<GenerationStatistics> terrainHistory,
            SettlementPlan settlement,
            Fitness settlementFitness,
            IReadOnlyList<GenerationStatistics> settlementHistory)
        {
            Terrain = terrain;
            TerrainFitness = terrainFitness;
            TerrainHistory = terrainHistory;
            Settlement = settlement;
            SettlementFitness = settlementFitness;
            SettlementHistory = settlementHistory;
        }

        public Terrain Terrain { get; }

        public Fitness TerrainFitness { get; }

        public IReadOnlyList<GenerationStatistics> TerrainHistory { get; }

        public SettlementPlan Settlement { get; }

        public Fitness SettlementFitness { get; }

        public IReadOnlyList<GenerationStatistics> SettlementHistory { get; }
    }
}
