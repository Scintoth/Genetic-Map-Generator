namespace GeneticMapGenerator.Domain.Common
{
    /// <summary>
    /// Deterministic source of randomness. The domain never touches engine or shared global RNGs,
    /// so a run is fully reproducible from its seed regardless of host (CLI, Unity, tests).
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Uniform 64-bit value.</summary>
        ulong NextUInt64();

        /// <summary>Uniform value in [0, 1).</summary>
        double NextDouble();

        /// <summary>Uniform integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>Creates an independent, deterministic stream derived from this one.</summary>
        IRandomSource Fork();
    }
}
