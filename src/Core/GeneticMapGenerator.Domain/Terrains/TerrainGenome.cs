using System;
using System.Collections.Generic;
using System.Linq;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Terrains
{
    /// <summary>
    /// The heritable description of a terrain: noise parameters plus the shaping and biome thresholds.
    /// Gene set and ranges follow the original dissertation's <c>Map</c> chromosome.
    /// </summary>
    public sealed record TerrainGenome
    {
        public TerrainGenome(
            bool flattenTerrain,
            bool additionalHills,
            IReadOnlyList<int> noiseOffsets,
            double maxHeight,
            double waterLevel,
            double xFrequency,
            double zFrequency,
            double amplitude,
            double exponent,
            double mountainLevel)
        {
            if (noiseOffsets.Count != TerrainGeneRanges.NoiseOffsetCount)
            {
                throw new ArgumentException($"Expected {TerrainGeneRanges.NoiseOffsetCount} noise offsets.", nameof(noiseOffsets));
            }

            FlattenTerrain = flattenTerrain;
            AdditionalHills = additionalHills;
            NoiseOffsets = noiseOffsets.ToArray();
            MaxHeight = maxHeight;
            WaterLevel = waterLevel;
            XFrequency = xFrequency;
            ZFrequency = zFrequency;
            Amplitude = amplitude;
            Exponent = exponent;
            MountainLevel = mountainLevel;
        }

        /// <summary>Clamp and scale tall terrain into mesas, producing lakes and plateaus.</summary>
        public bool FlattenTerrain { get; init; }

        /// <summary>Raise extra hills on top of mesas (only meaningful with <see cref="FlattenTerrain"/>).</summary>
        public bool AdditionalHills { get; init; }

        /// <summary>
        /// Integer genes that position the terrain within the noise field: the first half sums to the
        /// X offset, the second half to the Z offset.
        /// </summary>
        public IReadOnlyList<int> NoiseOffsets { get; init; }

        public double MaxHeight { get; init; }

        /// <summary>Sea level as a fraction of the highest vertex.</summary>
        public double WaterLevel { get; init; }

        public double XFrequency { get; init; }

        public double ZFrequency { get; init; }

        /// <summary>Noise amplitude (named <c>waveLength</c> in the original code).</summary>
        public double Amplitude { get; init; }

        public double Exponent { get; init; }

        /// <summary>Treeline as a fraction of the highest vertex; above it is mountain.</summary>
        public double MountainLevel { get; init; }

        public double OffsetX => NoiseOffsets.Take(NoiseOffsets.Count / 2).Sum();

        public double OffsetZ => NoiseOffsets.Skip(NoiseOffsets.Count / 2).Sum();

        public bool Equals(TerrainGenome? other) =>
            other is not null
            && FlattenTerrain == other.FlattenTerrain
            && AdditionalHills == other.AdditionalHills
            && NoiseOffsets.SequenceEqual(other.NoiseOffsets)
            && MaxHeight.Equals(other.MaxHeight)
            && WaterLevel.Equals(other.WaterLevel)
            && XFrequency.Equals(other.XFrequency)
            && ZFrequency.Equals(other.ZFrequency)
            && Amplitude.Equals(other.Amplitude)
            && Exponent.Equals(other.Exponent)
            && MountainLevel.Equals(other.MountainLevel);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(FlattenTerrain);
            hash.Add(AdditionalHills);
            foreach (var offset in NoiseOffsets)
            {
                hash.Add(offset);
            }

            hash.Add(MaxHeight);
            hash.Add(WaterLevel);
            hash.Add(XFrequency);
            hash.Add(ZFrequency);
            hash.Add(Amplitude);
            hash.Add(Exponent);
            hash.Add(MountainLevel);
            return hash.ToHashCode();
        }
    }

    /// <summary>Legal range of every terrain gene.</summary>
    public static class TerrainGeneRanges
    {
        public const int NoiseOffsetCount = 10;

        public static IntInterval NoiseOffset { get; } = new IntInterval(10, 80);

        public static Interval MaxHeight { get; } = new Interval(15, 40);

        public static Interval WaterLevel { get; } = new Interval(0, 0.4);

        public static Interval Frequency { get; } = new Interval(0.005, 0.04);

        public static Interval Amplitude { get; } = new Interval(1.3, 5);

        public static Interval Exponent { get; } = new Interval(1, 4);

        public static Interval MountainLevel { get; } = new Interval(0.5, 0.95);
    }
}
