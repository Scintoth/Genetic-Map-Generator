using FluentAssertions;
using GeneticMapGenerator.Domain.Common;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Domain.Common;

public class SplitMix64RandomTests
{
    [Test]
    public void SameSeed_ProducesIdenticalSequences()
    {
        var first = new SplitMix64Random(123);
        var second = new SplitMix64Random(123);

        Enumerable.Range(0, 100).Select(_ => first.NextUInt64())
            .Should().Equal(Enumerable.Range(0, 100).Select(_ => second.NextUInt64()));
    }

    [Test]
    public void NextDouble_StaysWithinUnitInterval()
    {
        var random = new SplitMix64Random(9);

        Enumerable.Range(0, 10_000).Select(_ => random.NextDouble())
            .Should().OnlyContain(value => value >= 0 && value < 1);
    }

    [Test]
    public void NextInt_CoversWholeRangeWithoutLeavingIt()
    {
        var random = new SplitMix64Random(5);

        var values = Enumerable.Range(0, 5_000).Select(_ => random.NextInt(-2, 3)).ToArray();

        values.Should().OnlyContain(value => value >= -2 && value < 3);
        values.Distinct().Should().HaveCount(5);
    }

    [Test]
    public void NextInt_WithEmptyRange_Throws()
    {
        var act = () => new SplitMix64Random(1).NextInt(3, 3);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Fork_ProducesAnIndependentDeterministicStream()
    {
        var forkA = new SplitMix64Random(77).Fork();
        var forkB = new SplitMix64Random(77).Fork();

        forkA.NextUInt64().Should().Be(forkB.NextUInt64());
    }
}

public class IntervalTests
{
    [TestCase(-1, 0)]
    [TestCase(0.5, 0.5)]
    [TestCase(3, 2)]
    public void Clamp_KeepsValuesInsideTheInterval(double value, double expected) =>
        new Interval(0, 2).Clamp(value).Should().Be(expected);

    [Test]
    public void Sample_StaysInsideTheInterval()
    {
        var interval = new Interval(10, 20);
        var random = new SplitMix64Random(3);

        Enumerable.Range(0, 1_000).Select(_ => interval.Sample(random)).Should().OnlyContain(v => interval.Contains(v));
    }

    [Test]
    public void Constructor_RejectsInvertedBounds()
    {
        var act = () => new Interval(2, 1);

        act.Should().Throw<ArgumentException>();
    }
}
