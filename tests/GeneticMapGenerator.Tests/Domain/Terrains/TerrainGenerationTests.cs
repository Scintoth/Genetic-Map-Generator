using FluentAssertions;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Scoring;
using GeneticMapGenerator.Domain.Terrains;
using GeneticMapGenerator.Domain.Terrains.Genetics;
using GeneticMapGenerator.Domain.Terrains.Noise;
using GeneticMapGenerator.Tests.Support;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Domain.Terrains;

public class TerrainGeneticOperatorsTests
{
    private readonly TerrainGeneticOperators _operators = new();

    [Test]
    public void Create_KeepsEveryGeneInRange()
    {
        var random = new SplitMix64Random(1);

        for (var i = 0; i < 200; i++)
        {
            AssertInRange(_operators.Create(random));
        }
    }

    [Test]
    public void Mutate_AtFullRate_StaysInRange()
    {
        var random = new SplitMix64Random(2);
        var genome = _operators.Create(random);

        for (var i = 0; i < 200; i++)
        {
            genome = _operators.Mutate(genome, 1, random);
            AssertInRange(genome);
        }
    }

    [Test]
    public void Mutate_AtZeroRate_ReturnsAnEqualGenome()
    {
        var random = new SplitMix64Random(3);
        var genome = _operators.Create(random);

        _operators.Mutate(genome, 0, random).Should().Be(genome);
    }

    [Test]
    public void Cross_ChildrenOnlyInheritParentGenes()
    {
        var random = new SplitMix64Random(4);
        var mother = _operators.Create(random);
        var father = _operators.Create(random);

        var (first, second) = _operators.Cross(mother, father, random);

        new[] { first.MaxHeight, second.MaxHeight }.Should().BeEquivalentTo(new[] { mother.MaxHeight, father.MaxHeight });
        new[] { first.Exponent, second.Exponent }.Should().BeEquivalentTo(new[] { mother.Exponent, father.Exponent });
        new[] { first.OffsetX, second.OffsetX }.Should().BeEquivalentTo(new[] { mother.OffsetX, father.OffsetX });
        new[] { first.OffsetZ, second.OffsetZ }.Should().BeEquivalentTo(new[] { mother.OffsetZ, father.OffsetZ });
    }

    private static void AssertInRange(TerrainGenome genome)
    {
        genome.NoiseOffsets.Should().HaveCount(TerrainGeneRanges.NoiseOffsetCount)
            .And.OnlyContain(offset => TerrainGeneRanges.NoiseOffset.Contains(offset));
        TerrainGeneRanges.MaxHeight.Contains(genome.MaxHeight).Should().BeTrue();
        TerrainGeneRanges.WaterLevel.Contains(genome.WaterLevel).Should().BeTrue();
        TerrainGeneRanges.Frequency.Contains(genome.XFrequency).Should().BeTrue();
        TerrainGeneRanges.Frequency.Contains(genome.ZFrequency).Should().BeTrue();
        TerrainGeneRanges.Amplitude.Contains(genome.Amplitude).Should().BeTrue();
        TerrainGeneRanges.Exponent.Contains(genome.Exponent).Should().BeTrue();
        TerrainGeneRanges.MountainLevel.Contains(genome.MountainLevel).Should().BeTrue();
    }
}

public class HeightMapGeneratorTests
{
    private static readonly TerrainGridSettings Grid = new() { Resolution = 33, WorldSize = 100 };

    [Test]
    public void Generate_IsDeterministic()
    {
        var genome = TestWorlds.Genome();

        var first = new HeightMapGenerator(Grid, new PerlinNoise()).Generate(genome);
        var second = new HeightMapGenerator(Grid, new PerlinNoise()).Generate(genome);

        Enumerable.Range(0, first.VertexCount).Should().OnlyContain(i => first[i] == second[i]);
    }

    [Test]
    public void Generate_RespectsTheHeightCeiling()
    {
        var genome = TestWorlds.Genome() with { Exponent = 4, Amplitude = 5, MaxHeight = 15 };

        var heightMap = new HeightMapGenerator(Grid, new PerlinNoise()).Generate(genome);

        heightMap.MaxHeight.Should().BeLessThanOrEqualTo(15 * 5);
        heightMap.MinHeight.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void DifferentOffsets_ProduceDifferentTerrain()
    {
        var genome = TestWorlds.Genome();
        var shifted = genome with { NoiseOffsets = genome.NoiseOffsets.Select(o => o + 7).ToArray() };
        var generator = new HeightMapGenerator(Grid, new PerlinNoise());

        generator.Generate(genome)[10, 10].Should().NotBe(generator.Generate(shifted)[10, 10]);
    }

    [Test]
    public void PerlinNoise_IsBoundedAndZeroOnLatticePoints()
    {
        var noise = new PerlinNoise();
        var random = new SplitMix64Random(8);

        noise.Sample(3, 7).Should().Be(0);
        Enumerable.Range(0, 5_000)
            .Select(_ => noise.Sample(random.NextDouble(-100, 100), random.NextDouble(-100, 100)))
            .Should().OnlyContain(value => value >= -1 && value <= 1);
    }
}

public class TerrainFitnessFunctionTests
{
    [Test]
    public void TerrainMatchingItsTargets_ScoresOne()
    {
        var terrain = TestWorlds.Island();
        var p = terrain.Biomes.Proportions;
        var targets = TerrainTargets.Create(p.Water, p.Grassland, p.Mountain, terrain.Peaks.Count);

        var fitness = new TerrainFitnessFunction(new StubTerrainBuilder(terrain), targets, new HyperbolicToleranceCurve())
            .Evaluate(terrain.Genome);

        fitness.Score.Should().BeApproximately(1, 1e-9);
        fitness.Components.Select(c => c.Name).Should().Equal("Water", "Grassland", "Mountain", "Peaks");
    }

    [Test]
    public void WrongPeakCount_LowersOnlyThePeakComponent()
    {
        var terrain = TestWorlds.Island();
        var p = terrain.Biomes.Proportions;
        var targets = TerrainTargets.Create(p.Water, p.Grassland, p.Mountain, terrain.Peaks.Count + 5);

        var fitness = new TerrainFitnessFunction(new StubTerrainBuilder(terrain), targets, new HyperbolicToleranceCurve())
            .Evaluate(terrain.Genome);

        fitness.Components.Single(c => c.Name == "Peaks").Score.Should().BeLessThan(0.5);
        fitness.Components.Where(c => c.Name != "Peaks").Should().OnlyContain(c => c.Score == 1);
    }

    [Test]
    public void Targets_AreNormalised()
    {
        var targets = TerrainTargets.Create(water: 2, grassland: 1, mountain: 1, peakCount: 3);

        (targets.Water, targets.Grassland, targets.Mountain).Should().Be((0.5, 0.25, 0.25));
    }

    private sealed class StubTerrainBuilder(Terrain terrain) : ITerrainBuilder
    {
        public Terrain Build(TerrainGenome genome) => terrain;
    }
}
