using System;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Terrains.Noise
{
    /// <summary>Coherent 2D noise in roughly [-1, 1].</summary>
    public interface INoise2D
    {
        double Sample(double x, double y);
    }

    /// <summary>
    /// Ken Perlin's improved gradient noise. The permutation table is derived from a seed rather than
    /// hard-coded, so results are identical on every host (replacing <c>UnityEngine.Mathf.PerlinNoise</c>).
    /// </summary>
    public sealed class PerlinNoise : INoise2D
    {
        private const int TableSize = 256;

        private readonly int[] _permutation = new int[TableSize * 2];

        public PerlinNoise(ulong seed = 0x5EEDUL)
        {
            var table = new int[TableSize];
            for (var i = 0; i < TableSize; i++)
            {
                table[i] = i;
            }

            var random = new SplitMix64Random(seed);
            for (var i = TableSize - 1; i > 0; i--)
            {
                var j = random.NextInt(0, i + 1);
                (table[i], table[j]) = (table[j], table[i]);
            }

            for (var i = 0; i < _permutation.Length; i++)
            {
                _permutation[i] = table[i & (TableSize - 1)];
            }
        }

        public double Sample(double x, double y)
        {
            var xFloor = Math.Floor(x);
            var yFloor = Math.Floor(y);
            var xi = (int)xFloor & (TableSize - 1);
            var yi = (int)yFloor & (TableSize - 1);
            var xf = x - xFloor;
            var yf = y - yFloor;

            var u = Fade(xf);
            var v = Fade(yf);
            var p = _permutation;
            var aa = p[p[xi] + yi];
            var ab = p[p[xi] + yi + 1];
            var ba = p[p[xi + 1] + yi];
            var bb = p[p[xi + 1] + yi + 1];

            var bottom = Lerp(u, Gradient(aa, xf, yf), Gradient(ba, xf - 1, yf));
            var top = Lerp(u, Gradient(ab, xf, yf - 1), Gradient(bb, xf - 1, yf - 1));
            return Lerp(v, bottom, top);
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        private static double Lerp(double t, double a, double b) => a + t * (b - a);

        private static double Gradient(int hash, double x, double y) => (hash & 7) switch
        {
            0 => x + y,
            1 => -x + y,
            2 => x - y,
            3 => -x - y,
            4 => x,
            5 => -x,
            6 => y,
            _ => -y,
        };
    }

    /// <summary>
    /// Fractal Brownian motion: octaves of a basis noise at doubling frequency and halving amplitude,
    /// normalised to [0, 1].
    /// </summary>
    public sealed class FractalNoise
    {
        private const double OctaveShift = 31.7;

        private readonly INoise2D _basis;
        private readonly int _octaves;
        private readonly double _lacunarity;
        private readonly double _persistence;
        private readonly double _amplitudeSum;

        public FractalNoise(INoise2D basis, int octaves, double lacunarity = 2.0, double persistence = 0.5)
        {
            if (octaves < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(octaves));
            }

            _basis = basis;
            _octaves = octaves;
            _lacunarity = lacunarity;
            _persistence = persistence;

            var amplitude = 1.0;
            for (var i = 0; i < octaves; i++)
            {
                _amplitudeSum += amplitude;
                amplitude *= persistence;
            }
        }

        public double Sample(double x, double y)
        {
            var sum = 0.0;
            var amplitude = 1.0;
            var frequency = 1.0;
            for (var octave = 0; octave < _octaves; octave++)
            {
                // Shift each octave so their lattice points do not line up.
                var shift = octave * OctaveShift;
                sum += amplitude * _basis.Sample(x * frequency + shift, y * frequency + shift);
                amplitude *= _persistence;
                frequency *= _lacunarity;
            }

            var normalised = 0.5 * (sum / _amplitudeSum + 1.0);
            return normalised < 0 ? 0 : normalised > 1 ? 1 : normalised;
        }
    }
}
