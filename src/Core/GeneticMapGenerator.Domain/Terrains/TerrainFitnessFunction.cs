using System;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Scoring;

namespace GeneticMapGenerator.Domain.Terrains
{
    /// <summary>What the user asked for: biome mix (normalised to sum to 1) and number of peaks.</summary>
    public sealed record TerrainTargets
    {
        private TerrainTargets(double water, double grassland, double mountain, int peakCount)
        {
            Water = water;
            Grassland = grassland;
            Mountain = mountain;
            PeakCount = peakCount;
        }

        public double Water { get; }

        public double Grassland { get; }

        public double Mountain { get; }

        public int PeakCount { get; }

        public static TerrainTargets Default { get; } = Create(water: 0.3, grassland: 0.5, mountain: 0.2, peakCount: 5);

        /// <summary>Biome weights are relative and are normalised, as the original <c>MapParameters</c> did.</summary>
        public static TerrainTargets Create(double water, double grassland, double mountain, int peakCount)
        {
            if (water < 0 || grassland < 0 || mountain < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(water), "Biome weights must not be negative.");
            }

            var total = water + grassland + mountain;
            if (total <= 0)
            {
                throw new ArgumentException("At least one biome weight must be positive.");
            }

            if (peakCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(peakCount));
            }

            return new TerrainTargets(water / total, grassland / total, mountain / total, peakCount);
        }
    }

    /// <summary>Scores how closely an expressed terrain matches <see cref="TerrainTargets"/>.</summary>
    public sealed class TerrainFitnessFunction : IFitnessFunction<TerrainGenome>
    {
        private readonly ITerrainBuilder _terrainBuilder;
        private readonly TerrainTargets _targets;
        private readonly IToleranceCurve _curve;

        public TerrainFitnessFunction(ITerrainBuilder terrainBuilder, TerrainTargets targets, IToleranceCurve curve)
        {
            _terrainBuilder = terrainBuilder;
            _targets = targets;
            _curve = curve;
        }

        public Fitness Evaluate(TerrainGenome genome) => Score(_terrainBuilder.Build(genome));

        public Fitness Score(Terrain terrain)
        {
            var proportions = terrain.Biomes.Proportions;
            return Fitness.MeanOf(
                new FitnessComponent("Water", _curve.Score(proportions.Water, _targets.Water)),
                new FitnessComponent("Grassland", _curve.Score(proportions.Grassland, _targets.Grassland)),
                new FitnessComponent("Mountain", _curve.Score(proportions.Mountain, _targets.Mountain)),
                new FitnessComponent("Peaks", _curve.Score(terrain.Peaks.Count, _targets.PeakCount)));
        }
    }
}
