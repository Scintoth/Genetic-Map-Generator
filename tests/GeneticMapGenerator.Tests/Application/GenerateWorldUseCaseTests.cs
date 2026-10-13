using FluentAssertions;
using GeneticMapGenerator.Application.Composition;
using GeneticMapGenerator.Application.Observers;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Application;

public class GenerateWorldUseCaseTests
{
    private static readonly GenerateWorldCommand SmallWorld = new GenerateWorldCommand
    {
        Grid = new TerrainGridSettings { Resolution = 41 },
        TerrainEvolution = new GenerateWorldCommand().TerrainEvolution with { PopulationSize = 16, MaxGenerations = 8 },
        SettlementEvolution = new GenerateWorldCommand().SettlementEvolution with { PopulationSize = 16, MaxGenerations = 8 },
    }.WithSeed(2024);

    private readonly IGenerateWorldUseCase _useCase = WorldGenerator.Create();

    [Test]
    public void Execute_RunsTerrainThenSettlementAndNotifiesInOrder()
    {
        var observer = new RecordingObserver();

        var result = _useCase.Execute(SmallWorld, observer);

        observer.Events.First().Should().StartWith("terrain 0");
        observer.Events.Last().Should().Be("completed");
        observer.Events.Should().ContainSingle(e => e == "completed");

        var terrainEvents = observer.Events.TakeWhile(e => e.StartsWith("terrain")).ToList();
        var settlementEvents = observer.Events.Skip(terrainEvents.Count).TakeWhile(e => e.StartsWith("settlement")).ToList();
        terrainEvents.Should().HaveCount(result.TerrainHistory.Count);
        settlementEvents.Should().HaveCount(result.SettlementHistory.Count);
        terrainEvents.Last().Should().EndWith("final");
        settlementEvents.Last().Should().EndWith("final");
    }

    [Test]
    public void Execute_PlacesTheSettlementOnTheEvolvedTerrain()
    {
        var result = _useCase.Execute(SmallWorld, NullWorldGenerationObserver.Instance);

        result.Settlement.Buildings.Should().NotBeEmpty();
        result.Settlement.Buildings.Should().OnlyContain(b =>
            Math.Abs(b.Position.Y - result.Terrain.HeightMap.SampleHeight(b.Position.X, b.Position.Z)) < 1e-9);
    }

    [Test]
    public void Execute_BestFitnessNeverRegresses()
    {
        var result = _useCase.Execute(SmallWorld, NullWorldGenerationObserver.Instance);

        result.TerrainHistory.Select(h => h.BestScore).Should().BeInAscendingOrder();
        result.SettlementHistory.Select(h => h.BestScore).Should().BeInAscendingOrder();
    }

    [Test]
    public void Execute_WithTheSameSeed_ProducesTheSameWorld()
    {
        var first = _useCase.Execute(SmallWorld, NullWorldGenerationObserver.Instance);
        var second = WorldGenerator.Create(new WorldGeneratorOptions { ParallelEvaluation = false })
            .Execute(SmallWorld, NullWorldGenerationObserver.Instance);

        second.Terrain.Genome.Should().Be(first.Terrain.Genome);
        second.Settlement.Genome.Should().Be(first.Settlement.Genome);
    }

    [Test]
    public void Execute_WithADifferentSeed_ProducesADifferentWorld()
    {
        var first = _useCase.Execute(SmallWorld, NullWorldGenerationObserver.Instance);
        var second = _useCase.Execute(SmallWorld.WithSeed(99), NullWorldGenerationObserver.Instance);

        second.Terrain.Genome.Should().NotBe(first.Terrain.Genome);
    }

    [Test]
    public void Execute_WhenCancelled_StopsWithoutCompleting()
    {
        using var cancellation = new CancellationTokenSource();
        var observer = new RecordingObserver(onTerrain: cancellation.Cancel);

        var act = () => _useCase.Execute(SmallWorld, observer, cancellation.Token);

        act.Should().Throw<OperationCanceledException>();
        observer.Events.Should().NotContain("completed");
    }

    [Test]
    public void Execute_WithInvalidSettings_Throws()
    {
        var invalid = SmallWorld with { TerrainEvolution = SmallWorld.TerrainEvolution with { PopulationSize = 1 } };

        var act = () => _useCase.Execute(invalid, NullWorldGenerationObserver.Instance);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private sealed class RecordingObserver(Action? onTerrain = null) : IWorldGenerationObserver
    {
        public List<string> Events { get; } = [];

        public void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot)
        {
            Events.Add($"terrain {snapshot.Generation}{(snapshot.IsFinal ? " final" : string.Empty)}");
            onTerrain?.Invoke();
        }

        public void OnSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot) =>
            Events.Add($"settlement {snapshot.Generation}{(snapshot.IsFinal ? " final" : string.Empty)}");

        public void OnCompleted(WorldGenerationResult result) => Events.Add("completed");
    }
}

public class CompositeObserverTests
{
    [Test]
    public void Composite_ForwardsToEveryObserver()
    {
        var first = new CountingObserver();
        var second = new CountingObserver();

        WorldGenerator.Create().Execute(
            new GenerateWorldCommand
            {
                Grid = new TerrainGridSettings { Resolution = 21 },
                TerrainEvolution = new GenerateWorldCommand().TerrainEvolution with { PopulationSize = 4, MaxGenerations = 1 },
                SettlementEvolution = new GenerateWorldCommand().SettlementEvolution with { PopulationSize = 4, MaxGenerations = 1 },
            },
            new CompositeWorldGenerationObserver(first, second));

        first.Completed.Should().Be(1);
        second.Completed.Should().Be(1);
    }

    private sealed class CountingObserver : WorldGenerationObserver
    {
        public int Completed { get; private set; }

        public override void OnCompleted(WorldGenerationResult result) => Completed++;
    }
}
