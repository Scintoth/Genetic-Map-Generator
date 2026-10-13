using System;

namespace GeneticMapGenerator.Domain.Common
{
    /// <summary>Closed real interval [Min, Max]; used to describe the legal range of a gene.</summary>
    public readonly struct Interval : IEquatable<Interval>
    {
        public Interval(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max) || max < min)
            {
                throw new ArgumentException($"Invalid interval [{min}, {max}].");
            }

            Min = min;
            Max = max;
        }

        public double Min { get; }

        public double Max { get; }

        public double Width => Max - Min;

        public bool Contains(double value) => value >= Min && value <= Max;

        public double Clamp(double value) => value < Min ? Min : value > Max ? Max : value;

        public double Sample(IRandomSource random) => Min + random.NextDouble() * Width;

        public bool Equals(Interval other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        public override bool Equals(object? obj) => obj is Interval other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Min, Max);

        public override string ToString() => $"[{Min}, {Max}]";
    }

    /// <summary>Closed integer interval [Min, Max].</summary>
    public readonly struct IntInterval : IEquatable<IntInterval>
    {
        public IntInterval(int min, int max)
        {
            if (max < min)
            {
                throw new ArgumentException($"Invalid interval [{min}, {max}].");
            }

            Min = min;
            Max = max;
        }

        public int Min { get; }

        public int Max { get; }

        public bool Contains(int value) => value >= Min && value <= Max;

        public int Clamp(int value) => value < Min ? Min : value > Max ? Max : value;

        public int Sample(IRandomSource random) => random.NextInt(Min, Max + 1);

        public bool Equals(IntInterval other) => Min == other.Min && Max == other.Max;

        public override bool Equals(object? obj) => obj is IntInterval other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Min, Max);

        public override string ToString() => $"[{Min}, {Max}]";
    }
}
