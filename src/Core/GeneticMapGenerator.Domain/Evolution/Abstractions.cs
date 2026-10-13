using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Evolution
{
    // The GA kernel is generic over the genome; each problem (terrain, settlement) plugs in
    // its own operators. Genomes are immutable values, so operators return new instances.

    /// <summary>Creates random members of the initial population.</summary>
    public interface IGenomeFactory<TGenome>
    {
        TGenome Create(IRandomSource random);
    }

    /// <summary>Recombines two parents into two children.</summary>
    public interface ICrossoverOperator<TGenome>
    {
        (TGenome First, TGenome Second) Cross(TGenome first, TGenome second, IRandomSource random);
    }

    /// <summary>Perturbs a genome; <c>rate</c> is the per-gene mutation probability.</summary>
    public interface IMutationOperator<TGenome>
    {
        TGenome Mutate(TGenome genome, double rate, IRandomSource random);
    }

    /// <summary>
    /// Scores a genome. Implementations must be deterministic and thread-safe:
    /// the evaluator may call them concurrently.
    /// </summary>
    public interface IFitnessFunction<in TGenome>
    {
        Fitness Evaluate(TGenome genome);
    }

    /// <summary>A genome paired with its evaluated fitness.</summary>
    public sealed record Scored<TGenome>(TGenome Genome, Fitness Fitness);
}
