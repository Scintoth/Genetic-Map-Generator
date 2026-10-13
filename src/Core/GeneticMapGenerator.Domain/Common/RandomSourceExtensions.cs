using System;
using System.Collections.Generic;

namespace GeneticMapGenerator.Domain.Common
{
    public static class RandomSourceExtensions
    {
        public static double NextDouble(this IRandomSource random, double minInclusive, double maxExclusive) =>
            minInclusive + random.NextDouble() * (maxExclusive - minInclusive);

        public static bool NextBool(this IRandomSource random) => random.NextDouble() < 0.5;

        public static bool Chance(this IRandomSource random, double probability) => random.NextDouble() < probability;

        /// <summary>Standard normal sample (Box–Muller).</summary>
        public static double NextGaussian(this IRandomSource random)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        public static T Pick<T>(this IRandomSource random, IReadOnlyList<T> items)
        {
            if (items.Count == 0)
            {
                throw new ArgumentException("Cannot pick from an empty list.", nameof(items));
            }

            return items[random.NextInt(0, items.Count)];
        }

        /// <summary>Returns the pair in random order — the primitive behind uniform crossover.</summary>
        public static (T First, T Second) Shuffle<T>(this IRandomSource random, T a, T b) =>
            random.NextBool() ? (a, b) : (b, a);
    }
}
