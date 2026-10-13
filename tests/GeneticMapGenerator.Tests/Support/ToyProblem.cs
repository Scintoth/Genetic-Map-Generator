using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Evolution;

namespace GeneticMapGenerator.Tests.Support;

/// <summary>
/// Minimal problem for exercising the GA kernel in isolation: evolve a vector of reals in [0, 1]
/// towards a fixed target. Fitness is 1 − mean absolute error.
/// </summary>
internal sealed record Vector(double[] Genes)
{
    public bool Equals(Vector? other) => other is not null && Genes.SequenceEqual(other.Genes);

    public override int GetHashCode() => Genes.Aggregate(17, (hash, gene) => HashCode.Combine(hash, gene));
}

internal sealed class VectorOperators(int length) :
    IGenomeFactory<Vector>, ICrossoverOperator<Vector>, IMutationOperator<Vector>
{
    private static readonly Interval Range = new(0, 1);

    public Vector Create(IRandomSource random) =>
        new(Enumerable.Range(0, length).Select(_ => Range.Sample(random)).ToArray());

    public (Vector First, Vector Second) Cross(Vector first, Vector second, IRandomSource random)
    {
        var pairs = first.Genes.Zip(second.Genes, (a, b) => random.Shuffle(a, b)).ToArray();
        return (new Vector(pairs.Select(p => p.First).ToArray()), new Vector(pairs.Select(p => p.Second).ToArray()));
    }

    public Vector Mutate(Vector genome, double rate, IRandomSource random) =>
        new(genome.Genes.Select(g => GeneMutation.Real(g, Range, rate, random)).ToArray());
}

internal sealed class DistanceToTarget(double target) : IFitnessFunction<Vector>
{
    public Fitness Evaluate(Vector genome) =>
        Fitness.MeanOf(new FitnessComponent("Distance", 1 - genome.Genes.Average(g => Math.Abs(g - target))));
}

internal static class ToyProblem
{
    public static GeneticAlgorithmBuilder<Vector> Builder(EvolutionSettings settings, int length = 8, double target = 0.75) =>
        new GeneticAlgorithmBuilder<Vector>()
            .WithSettings(settings)
            .WithOperators(new VectorOperators(length))
            .WithFitnessFunction(new DistanceToTarget(target));

    public static Population<Vector> PopulationWithScores(params double[] scores) =>
        new(0, scores.Select((score, i) => new Scored<Vector>(new Vector([i]), Fitness.MeanOf(new FitnessComponent("Score", score)))));
}
