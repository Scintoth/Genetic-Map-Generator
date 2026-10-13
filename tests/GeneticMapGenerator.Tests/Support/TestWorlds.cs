using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;
using GeneticMapGenerator.Domain.Terrains.Analysis;

namespace GeneticMapGenerator.Tests.Support;

/// <summary>Builders for hand-crafted terrains so analysis and planning can be tested without noise.</summary>
internal static class TestWorlds
{
    public static TerrainGenome Genome(double waterLevel = 0.2, double mountainLevel = 0.6) => new(
        flattenTerrain: false,
        additionalHills: false,
        noiseOffsets: Enumerable.Repeat(40, TerrainGeneRanges.NoiseOffsetCount).ToArray(),
        maxHeight: 20,
        waterLevel: waterLevel,
        xFrequency: 0.02,
        zFrequency: 0.02,
        amplitude: 3,
        exponent: 2,
        mountainLevel: mountainLevel);

    public static HeightMap HeightMap(int resolution, double spacing, Func<double, double, double> heightAtWorld)
    {
        var heights = new float[resolution * resolution];
        var half = (resolution - 1) * spacing / 2;
        for (var z = 0; z < resolution; z++)
        {
            for (var x = 0; x < resolution; x++)
            {
                heights[z * resolution + x] = (float)heightAtWorld(x * spacing - half, z * spacing - half);
            }
        }

        return new HeightMap(resolution, spacing, heights);
    }

    public static double Hill(double x, double z, double centreX, double centreZ, double height, double width)
    {
        var dx = x - centreX;
        var dz = z - centreZ;
        return height * Math.Exp(-(dx * dx + dz * dz) / (2 * width * width));
    }

    public static Terrain Analyse(HeightMap heightMap, TerrainGenome? genome = null, double maxSlopeDegrees = 12)
    {
        genome ??= Genome();
        var biomes = new ThresholdBiomeClassifier().Classify(heightMap, genome);
        var peaks = new ProminencePeakDetector(0.1).Detect(heightMap, biomes);
        var buildable = new SlopeBuildableLandDetector(maxSlopeDegrees).Detect(heightMap, biomes);
        return new Terrain(genome, heightMap, biomes, peaks, buildable);
    }

    /// <summary>A flat-topped island (buildable grassland) ringed by water, with one steep central mountain.</summary>
    public static Terrain Island() => Analyse(HeightMap(65, 2, (x, z) =>
    {
        var radius = Math.Sqrt(x * x + z * z);
        var plateau = 10 / (1 + Math.Exp((radius - 40) / 3));
        return 2 + plateau + Hill(x, z, 0, 0, 28, 6);
    }));

    public static SettlementGenome Settlement(int buildings = 8, int inhabitants = 20, double prosperity = 14, ulong seed = 7) =>
        new(SiteSelector: 0.5, Area: 120, BuildingCount: buildings, ProsperityBudget: prosperity, InhabitantBudget: inhabitants, LayoutSeed: seed);
}
