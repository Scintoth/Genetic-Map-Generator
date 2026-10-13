using System;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Evolution
{
    /// <summary>
    /// Per-gene mutation primitives shared by genome operators. A mutating gene usually takes a small
    /// Gaussian step (fine-tuning) and occasionally resets anywhere in range (escaping local optima).
    /// </summary>
    public static class GeneMutation
    {
        private const double CreepStandardDeviation = 0.1;
        private const double ResetProbability = 0.2;

        public static double Real(double value, Interval range, double rate, IRandomSource random)
        {
            if (!random.Chance(rate))
            {
                return value;
            }

            return random.Chance(ResetProbability)
                ? range.Sample(random)
                : range.Clamp(value + random.NextGaussian() * CreepStandardDeviation * range.Width);
        }

        public static int Integer(int value, IntInterval range, double rate, IRandomSource random)
        {
            if (!random.Chance(rate))
            {
                return value;
            }

            if (random.Chance(ResetProbability))
            {
                return range.Sample(random);
            }

            var step = random.NextGaussian() * CreepStandardDeviation * (range.Max - range.Min);
            var rounded = (int)Math.Round(step, MidpointRounding.AwayFromZero);
            return range.Clamp(value + (rounded == 0 ? (random.NextBool() ? 1 : -1) : rounded));
        }

        public static bool Flag(bool value, double rate, IRandomSource random) =>
            random.Chance(rate) ? !value : value;

        public static ulong Seed(ulong value, double rate, IRandomSource random) =>
            random.Chance(rate) ? random.NextUInt64() : value;
    }
}
