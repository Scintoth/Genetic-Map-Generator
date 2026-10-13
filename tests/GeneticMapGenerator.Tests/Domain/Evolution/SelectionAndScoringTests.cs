using FluentAssertions;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Evolution.Selection;
using GeneticMapGenerator.Domain.Evolution.Termination;
using GeneticMapGenerator.Domain.Scoring;
using GeneticMapGenerator.Tests.Support;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Domain.Evolution;

public class SelectionStrategyTests
{
    private static readonly Population<Vector> Population = ToyProblem.PopulationWithScores(0.2, 0.9, 0.5, 0.1, 0.7);

    [Test]
    public void TwoFittest_PicksTheTwoHighestScores()
    {
        var (first, second) = new TwoFittestSelection().SelectParents(Population, new SplitMix64Random(1));

        (first.Fitness.Score, second.Fitness.Score).Should().Be((0.9, 0.7));
    }

    [Test]
    public void BestAndWorst_PicksTheExtremes()
    {
        var (first, second) = new BestAndWorstSelection().SelectParents(Population, new SplitMix64Random(1));

        (first.Fitness.Score, second.Fitness.Score).Should().Be((0.9, 0.1));
    }

    [Test]
    public void FittestAndRandom_PairsTheBestWithSomeoneElse()
    {
        var random = new SplitMix64Random(1);

        for (var i = 0; i < 50; i++)
        {
            var (first, second) = new FittestAndRandomSelection().SelectParents(Population, random);
            first.Should().BeSameAs(Population.Best);
            second.Should().NotBeSameAs(Population.Best);
        }
    }

    [Test]
    public void Tournament_FavoursFitterIndividuals()
    {
        var picks = Draw(new TournamentSelection(3), 4_000);

        picks[0.9].Should().BeGreaterThan(picks[0.5]);
        picks[0.5].Should().BeGreaterThan(picks.GetValueOrDefault(0.1));
    }

    [Test]
    public void RouletteWheel_SelectsInProportionToFitness()
    {
        var picks = Draw(new RouletteWheelSelection(), 20_000);
        var total = picks.Values.Sum();

        ((double)picks[0.9] / total).Should().BeApproximately(0.9 / 2.4, 0.02);
        ((double)picks[0.1] / total).Should().BeApproximately(0.1 / 2.4, 0.02);
    }

    [Test]
    public void RouletteWheel_WithAllZeroScores_StillSelects()
    {
        var zeros = ToyProblem.PopulationWithScores(0, 0, 0);

        var act = () => new RouletteWheelSelection().SelectParents(zeros, new SplitMix64Random(1));

        act.Should().NotThrow();
    }

    [TestCaseSource(nameof(AllMethods))]
    public void Factory_CreatesEveryMethod(SelectionMethod method) =>
        SelectionStrategyFactory.Create(method).Should().NotBeNull();

    private static IEnumerable<SelectionMethod> AllMethods() => Enum.GetValues<SelectionMethod>();

    private static Dictionary<double, int> Draw(ISelectionStrategy strategy, int pairs)
    {
        var random = new SplitMix64Random(11);
        return Enumerable.Range(0, pairs)
            .SelectMany(_ =>
            {
                var (first, second) = strategy.SelectParents(Population, random);
                return new[] { first.Fitness.Score, second.Fitness.Score };
            })
            .GroupBy(score => score)
            .ToDictionary(group => group.Key, group => group.Count());
    }
}

public class FitnessTests
{
    [Test]
    public void MeanOf_AveragesEveryComponent()
    {
        // Regression: the original GAFunctions.Average skipped the last value (and so ignored peaks).
        var fitness = Fitness.MeanOf(
            new FitnessComponent("A", 1),
            new FitnessComponent("B", 1),
            new FitnessComponent("C", 1),
            new FitnessComponent("Peaks", 0));

        fitness.Score.Should().Be(0.75);
        fitness.Components.Should().HaveCount(4);
    }

    [Test]
    public void MeanOf_NoComponents_IsZero() => Fitness.MeanOf().Score.Should().Be(0);
}

public class HyperbolicToleranceCurveTests
{
    private readonly HyperbolicToleranceCurve _curve = new();

    [Test]
    public void OnTarget_ScoresOne() => _curve.Score(0.3, 0.3).Should().Be(1);

    [Test]
    public void ScoreFallsAsErrorGrows()
    {
        // Regression: the 2018 rewrite used 1 + exp(-3x²), which peaked when the value was zero.
        var scores = new[] { 0.3, 0.33, 0.4, 0.6, 1.0 }.Select(actual => _curve.Score(actual, 0.3)).ToArray();

        scores.Should().BeInDescendingOrder();
        scores.Should().OnlyContain(score => score >= 0 && score <= 1);
    }

    [Test]
    public void ThirtyPercentRelativeError_ScoresAboutThreeQuarters() =>
        _curve.Score(13, 10).Should().BeApproximately(Math.Tanh(1), 1e-9);

    [Test]
    public void ZeroTarget_IsWellDefined()
    {
        _curve.Score(0, 0).Should().Be(1);
        _curve.Score(0.5, 0).Should().BeInRange(0, 0.1);
    }
}

public class TerminationTests
{
    [Test]
    public void AnyCondition_ReportsTheFirstSatisfiedReason()
    {
        var condition = AnyTerminationCondition.FromSettings(new EvolutionSettings { TargetFitness = 0.9, MaxGenerations = 10 });

        condition.IsSatisfiedBy(new EvolutionProgress(3, 0.95, 0), out var reason).Should().BeTrue();
        reason.Should().Contain("target fitness");

        condition.IsSatisfiedBy(new EvolutionProgress(3, 0.5, 0), out _).Should().BeFalse();
    }
}
