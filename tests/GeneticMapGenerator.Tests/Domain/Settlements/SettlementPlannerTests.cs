using FluentAssertions;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Scoring;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Settlements.Genetics;
using GeneticMapGenerator.Domain.Terrains.Analysis;
using GeneticMapGenerator.Tests.Support;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Domain.Settlements;

public class SettlementPlannerTests
{
    private readonly OrganicSettlementPlanner _planner = new();

    [Test]
    public void Plan_PlacesBuildingsOnlyOnBuildableLand()
    {
        var terrain = TestWorlds.Island();

        var plan = _planner.Plan(TestWorlds.Settlement(buildings: 15), terrain);

        plan.Buildings.Should().NotBeEmpty();
        plan.Buildings.Should().OnlyContain(b =>
            terrain.BuildableLand.IsBuildable(terrain.HeightMap.NearestPoint(b.Position.X, b.Position.Z)));
    }

    [Test]
    public void Plan_SitsBuildingsOnTheGround()
    {
        var terrain = TestWorlds.Island();

        var plan = _planner.Plan(TestWorlds.Settlement(), terrain);

        plan.Buildings.Should().OnlyContain(b =>
            Math.Abs(b.Position.Y - terrain.HeightMap.SampleHeight(b.Position.X, b.Position.Z)) < 1e-9);
    }

    [Test]
    public void Plan_NeverOverlapsBuildings()
    {
        var plan = _planner.Plan(TestWorlds.Settlement(buildings: 20, prosperity: 14), TestWorlds.Island());

        foreach (var (a, i) in plan.Buildings.Select((b, i) => (b, i)))
        {
            foreach (var b in plan.Buildings.Skip(i + 1))
            {
                a.Position.HorizontalDistanceTo(b.Position).Should().BeGreaterThanOrEqualTo(a.ClearanceRadius + b.ClearanceRadius);
            }
        }
    }

    [Test]
    public void Plan_RespectsBuildingAndInhabitantBudgets()
    {
        var plan = _planner.Plan(TestWorlds.Settlement(buildings: 5, inhabitants: 3), TestWorlds.Island());

        plan.Buildings.Count.Should().BeLessThanOrEqualTo(5);
        plan.TotalResidents.Should().BeLessThanOrEqualTo(3);
    }

    [Test]
    public void Plan_StopsBuildingWhenProsperityRunsOut()
    {
        var plan = _planner.Plan(TestWorlds.Settlement(buildings: 20, prosperity: 3), TestWorlds.Island());

        // Each building costs at least 0.5 prosperity; the budget may be overshot by the last one only.
        plan.TotalProsperity.Should().BeLessThan(3 + 3);
        plan.Buildings.Count.Should().BeLessThan(20);
    }

    [Test]
    public void Plan_IsReproducibleFromTheGenome()
    {
        var terrain = TestWorlds.Island();
        var genome = TestWorlds.Settlement();

        _planner.Plan(genome, terrain).Buildings.Should().Equal(_planner.Plan(genome, terrain).Buildings);
    }

    [Test]
    public void Plan_OnTerrainWithNoGrassland_IsEmpty()
    {
        // Sea level at 4 and treeline at 3.5: every vertex is either water or mountain.
        var barren = TestWorlds.Analyse(
            TestWorlds.HeightMap(9, 1, (x, _) => x < 0 ? 0 : 10),
            TestWorlds.Genome(waterLevel: 0.4, mountainLevel: 0.35));

        barren.Biomes.Proportions.Grassland.Should().Be(0);
        _planner.Plan(TestWorlds.Settlement(), barren).Buildings.Should().BeEmpty();
    }
}

public class SettlementFitnessAndOperatorsTests
{
    [Test]
    public void Fitness_IsOneWhenThePlanMatchesTheTargets()
    {
        var terrain = TestWorlds.Island();
        var planner = new OrganicSettlementPlanner();
        var genome = TestWorlds.Settlement();
        var plan = planner.Plan(genome, terrain);
        var targets = new SettlementTargets(plan.TotalFloorArea, plan.Buildings.Count, Math.Max(1, plan.TotalResidents), plan.TotalProsperity);

        var fitness = new SettlementFitnessFunction(terrain, planner, targets, new HyperbolicToleranceCurve()).Evaluate(genome);

        fitness.Components.Where(c => c.Name != "Inhabitants").Should().OnlyContain(c => c.Score > 0.999);
    }

    [Test]
    public void Operators_KeepGenesInRange()
    {
        var operators = new SettlementGeneticOperators();
        var random = new SplitMix64Random(6);
        var genome = operators.Create(random);

        for (var i = 0; i < 300; i++)
        {
            var (child, _) = operators.Cross(genome, operators.Create(random), random);
            genome = operators.Mutate(child, 0.5, random);

            SettlementGeneRanges.SiteSelector.Contains(genome.SiteSelector).Should().BeTrue();
            SettlementGeneRanges.Area.Contains(genome.Area).Should().BeTrue();
            SettlementGeneRanges.BuildingCount.Contains(genome.BuildingCount).Should().BeTrue();
            SettlementGeneRanges.ProsperityBudget.Contains(genome.ProsperityBudget).Should().BeTrue();
            SettlementGeneRanges.InhabitantBudget.Contains(genome.InhabitantBudget).Should().BeTrue();
        }
    }

    [Test]
    public void Island_HasMountainGrassAndWater()
    {
        var p = TestWorlds.Island().Biomes.Proportions;

        new[] { p.Water, p.Grassland, p.Mountain }.Should().OnlyContain(share => share > 0);
        TestWorlds.Island().Biomes[0, 0].Should().Be(Biome.Water);
    }
}
