using FluentAssertions;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Terrains.Analysis;
using GeneticMapGenerator.Tests.Support;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Domain.Terrains;

public class ProminencePeakDetectorTests
{
    private readonly ProminencePeakDetector _detector = new(0.1);

    [Test]
    public void SingleHill_IsOnePeakAtItsSummit()
    {
        var heightMap = TestWorlds.HeightMap(41, 1, (x, z) => TestWorlds.Hill(x, z, 4, -6, 30, 5));

        var peaks = Detect(heightMap);

        peaks.Should().ContainSingle().Which.Should().Be(heightMap.NearestPoint(4, -6));
    }

    [Test]
    public void TwoSeparateHills_AreTwoPeaks()
    {
        var heightMap = TestWorlds.HeightMap(61, 1, (x, z) =>
            TestWorlds.Hill(x, z, -15, 0, 30, 4) + TestWorlds.Hill(x, z, 15, 0, 25, 4));

        Detect(heightMap).Should().HaveCount(2);
    }

    [Test]
    public void SmallBumpOnAFlank_IsNotAPeak()
    {
        // The bump is a local maximum, but rises far less than 10% of the relief above its saddle.
        var heightMap = TestWorlds.HeightMap(61, 1, (x, z) =>
            TestWorlds.Hill(x, z, 0, 0, 30, 10) + TestWorlds.Hill(x, z, 12, 0, 1.5, 1.5));

        Detect(heightMap).Should().ContainSingle();
    }

    [Test]
    public void HillBelowTheTreeline_IsNotAPeak()
    {
        var heightMap = TestWorlds.HeightMap(61, 1, (x, z) =>
            TestWorlds.Hill(x, z, -15, 0, 30, 4) + TestWorlds.Hill(x, z, 15, 0, 10, 4));

        // Mountain level 0.6 × 30 = 18, so the 10-unit hill is grassland.
        Detect(heightMap).Should().ContainSingle().Which.Should().Be(heightMap.NearestPoint(-15, 0));
    }

    [Test]
    public void FlatMap_HasNoPeaks() =>
        Detect(TestWorlds.HeightMap(11, 1, (_, _) => 5)).Should().BeEmpty();

    private IReadOnlyList<GridPoint> Detect(GeneticMapGenerator.Domain.Terrains.HeightMap heightMap) =>
        _detector.Detect(heightMap, new ThresholdBiomeClassifier().Classify(heightMap, TestWorlds.Genome(mountainLevel: 0.6)));
}

public class BiomeAndBuildableLandTests
{
    [Test]
    public void Classifier_UsesThresholdsRelativeToTheHighestVertex()
    {
        var heightMap = TestWorlds.HeightMap(3, 1, (x, _) => x + 1); // columns at heights 0, 1, 2
        var genome = TestWorlds.Genome(waterLevel: 0.25, mountainLevel: 0.75);

        var biomes = new ThresholdBiomeClassifier().Classify(heightMap, genome);

        biomes.WaterHeight.Should().Be(0.5);
        biomes.MountainHeight.Should().Be(1.5);
        biomes[0, 0].Should().Be(Biome.Water);
        biomes[1, 0].Should().Be(Biome.Grassland);
        biomes[2, 0].Should().Be(Biome.Mountain);
        biomes.Proportions.Should().Be(new BiomeProportions(1 / 3.0, 1 / 3.0, 1 / 3.0));
    }

    [Test]
    public void Proportions_AlwaysSumToOne()
    {
        var p = TestWorlds.Island().Biomes.Proportions;

        (p.Water + p.Grassland + p.Mountain).Should().BeApproximately(1, 1e-9);
    }

    [Test]
    public void BuildableLand_IsGentleGrasslandOnly()
    {
        var terrain = TestWorlds.Island();

        terrain.BuildableLand.Sites.Should().NotBeEmpty();
        terrain.BuildableLand.Sites.Should().OnlyContain(site =>
            terrain.Biomes[site.X, site.Z] == Biome.Grassland
            && terrain.HeightMap.SlopeDegrees(site.X, site.Z) <= 12);
    }

    [Test]
    public void HeightMap_SampleHeight_MatchesVerticesAndInterpolatesBetweenThem()
    {
        var heightMap = TestWorlds.HeightMap(3, 2, (x, _) => x);

        heightMap.SampleHeight(heightMap.WorldX(1), heightMap.WorldZ(1)).Should().BeApproximately(heightMap[1, 1], 1e-6);
        heightMap.SampleHeight(1, 0).Should().BeApproximately(1, 1e-6);
        heightMap.SlopeDegrees(1, 1).Should().BeApproximately(45, 1e-6);
    }
}
