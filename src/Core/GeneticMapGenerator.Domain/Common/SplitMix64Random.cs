using System;

namespace GeneticMapGenerator.Domain.Common
{
    /// <summary>
    /// SplitMix64 generator. Chosen over <see cref="Random"/> because its output is identical on
    /// every runtime (.NET, Mono/IL2CPP), which keeps seeded runs reproducible between CLI and Unity.
    /// </summary>
    public sealed class SplitMix64Random : IRandomSource
    {
        private const double UnitScale = 1.0 / (1UL << 53);

        private ulong _state;

        public SplitMix64Random(ulong seed)
        {
            _state = seed;
        }

        public ulong NextUInt64()
        {
            unchecked
            {
                var z = _state += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        public double NextDouble() => (NextUInt64() >> 11) * UnitScale;

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Range must not be empty.");
            }

            var range = (ulong)((long)maxExclusive - minInclusive);
            return (int)(minInclusive + (long)(NextUInt64() % range));
        }

        public IRandomSource Fork() => new SplitMix64Random(NextUInt64());
    }
}
