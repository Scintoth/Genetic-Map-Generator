using FluentAssertions;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Evolution.Evaluation;
using GeneticMapGenerator.Tests.Support;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Domain.Evolution;

public class GeneticAlgorithmTests
{
    private static readonly EvolutionSettings Settings = new()
    {
        PopulationSize = 30,
        MaxGenerations = 200,
        TargetFitness = 0.97,
        Seed = 42,
    };

    [Test]
    public void Evolve_OnASolvableProblem_ReachesTheTargetFitness()
    {
        var reports = ToyProblem.Builder(Settings).Build().Evolve().ToList();

        var final = reports.Last();
        final.IsFinal.Should().BeTrue();
        final.Best.Fitness.Score.Should().BeGreaterThanOrEqualTo(Settings.TargetFitness);
        final.TerminationReason.Should().Contain("target fitness");
        final.Generation.Should().BeLessThan(Settings.MaxGenerations);
    }

    [Test]
    public void Evolve_ImprovesOnTheRandomInitialPopulation()
    {
        var reports = ToyProblem.Builder(Settings).Build().Evolve().ToList();

        reports.Last().Best.Fitness.Score.Should().BeGreaterThan(reports.First().Best.Fitness.Score);
        reports.Last().Population.AverageScore.Should().BeGreaterThan(reports.First().Population.AverageScore);
    }

    [Test]
    public void Evolve_WithElitism_NeverLosesTheBestIndividual()
    {
        var bestScores = ToyProblem.Builder(Settings with { EliteCount = 1 }).Build()
            .Evolve()
            .Select(report => report.Best.Fitness.Score)
            .ToList();

        bestScores.Should().BeInAscendingOrder();
    }

    [Test]
    public void Evolve_WithTheSameSeed_IsDeterministic()
    {
        var parallel = ToyProblem.Builder(Settings).WithEvaluator(new ParallelFitnessEvaluator()).Build().Evolve().Last();
        var sequential = ToyProblem.Builder(Settings).WithEvaluator(new SequentialFitnessEvaluator()).Build().Evolve().Last();

        parallel.Generation.Should().Be(sequential.Generation);
        parallel.Best.Genome.Should().Be(sequential.Best.Genome);
    }

    [Test]
    public void Evolve_StopsAtTheGenerationLimit()
    {
        var unreachable = Settings with { TargetFitness = 1, MaxGenerations = 5 };

        var reports = ToyProblem.Builder(unreachable).Build().Evolve().ToList();

        reports.Select(r => r.Generation).Should().Equal(0, 1, 2, 3, 4, 5);
        reports.Should().ContainSingle(r => r.IsFinal).Which.TerminationReason.Should().Contain("generation limit");
    }

    [Test]
    public void Evolve_StopsWhenStagnant()
    {
        var stagnating = Settings with { TargetFitness = 1, MaxGenerations = 1_000, StagnationLimit = 3, MutationRate = 0, CrossoverRate = 0 };

        var final = ToyProblem.Builder(stagnating).Build().Evolve().Last();

        final.TerminationReason.Should().Contain("no improvement");
    }

    [Test]
    public void Evolve_KeepsThePopulationSizeConstant()
    {
        var oddSized = Settings with { PopulationSize = 7, EliteCount = 2, MaxGenerations = 4, TargetFitness = 1 };

        ToyProblem.Builder(oddSized).Build().Evolve().Should().OnlyContain(r => r.Population.Size == 7);
    }

    [Test]
    public void Evolve_WhenCancelled_Throws()
    {
        using var cancellation = new CancellationTokenSource();
        var reports = ToyProblem.Builder(Settings with { TargetFitness = 1 }).Build().Evolve(cancellation.Token);

        var act = () =>
        {
            foreach (var _ in reports)
            {
                cancellation.Cancel();
            }
        };

        act.Should().Throw<OperationCanceledException>();
    }

    [Test]
    public void Build_WithoutAFitnessFunction_Throws()
    {
        var act = () => new GeneticAlgorithmBuilder<Vector>().WithOperators(new VectorOperators(3)).Build();

        act.Should().Throw<InvalidOperationException>().WithMessage("*fitness function*");
    }

    [Test]
    public void Build_WithInvalidSettings_Throws()
    {
        var act = () => ToyProblem.Builder(Settings with { EliteCount = Settings.PopulationSize }).Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*EliteCount*");
    }
}
