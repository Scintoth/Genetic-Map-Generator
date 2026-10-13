using System;
using System.Collections.Generic;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Terrains.Analysis;
using GeneticMapGenerator.Domain.Terrains.Noise;

namespace GeneticMapGenerator.Domain.Terrains
{
    /// <summary>The expressed phenotype of a <see cref="TerrainGenome"/>: geometry plus its analysis.</summary>
    public sealed class Terrain
    {
        public Terrain(TerrainGenome genome, HeightMap heightMap, BiomeMap biomes, IReadOnlyList<GridPoint> peaks, BuildableLand buildableLand)
        {
            Genome = genome;
            HeightMap = heightMap;
            Biomes = biomes;
            Peaks = peaks;
            BuildableLand = buildableLand;
        }

        public TerrainGenome Genome { get; }

        public HeightMap HeightMap { get; }

        public BiomeMap Biomes { get; }

        public IReadOnlyList<GridPoint> Peaks { get; }

        public BuildableLand BuildableLand { get; }

        public double WaterHeight => Biomes.WaterHeight;
    }

    /// <summary>Settings for interpreting a height map.</summary>
    public sealed record TerrainAnalysisSettings
    {
        /// <summary>Minimum prominence of a peak, as a fraction of the map's height range.</summary>
        public double PeakProminenceFraction { get; init; } = 0.1;

        /// <summary>Steepest slope a building may sit on.</summary>
        public double MaxBuildableSlopeDegrees { get; init; } = 12;
    }

    /// <summary>Expresses a genome as a fully analysed <see cref="Terrain"/>.</summary>
    public interface ITerrainBuilder
    {
        Terrain Build(TerrainGenome genome);
    }

    /// <summary>Facade over generation and analysis.</summary>
    public sealed class TerrainBuilder : ITerrainBuilder
    {
        private readonly IHeightMapGenerator _heightMapGenerator;
        private readonly IBiomeClassifier _biomeClassifier;
        private readonly IPeakDetector _peakDetector;
        private readonly IBuildableLandDetector _buildableLandDetector;

        public TerrainBuilder(
            IHeightMapGenerator heightMapGenerator,
            IBiomeClassifier biomeClassifier,
            IPeakDetector peakDetector,
            IBuildableLandDetector buildableLandDetector)
        {
            _heightMapGenerator = heightMapGenerator;
            _biomeClassifier = biomeClassifier;
            _peakDetector = peakDetector;
            _buildableLandDetector = buildableLandDetector;
        }

        /// <summary>Wires the default terrain pipeline.</summary>
        public static ITerrainBuilder CreateDefault(TerrainGridSettings grid, TerrainAnalysisSettings analysis) =>
            new TerrainBuilder(
                new HeightMapGenerator(grid, new PerlinNoise()),
                new ThresholdBiomeClassifier(),
                new ProminencePeakDetector(analysis.PeakProminenceFraction),
                new SlopeBuildableLandDetector(analysis.MaxBuildableSlopeDegrees));

        public Terrain Build(TerrainGenome genome)
        {
            if (genome is null)
            {
                throw new ArgumentNullException(nameof(genome));
            }

            var heightMap = _heightMapGenerator.Generate(genome);
            var biomes = _biomeClassifier.Classify(heightMap, genome);
            var peaks = _peakDetector.Detect(heightMap, biomes);
            var buildable = _buildableLandDetector.Detect(heightMap, biomes);
            return new Terrain(genome, heightMap, biomes, peaks, buildable);
        }
    }
}
