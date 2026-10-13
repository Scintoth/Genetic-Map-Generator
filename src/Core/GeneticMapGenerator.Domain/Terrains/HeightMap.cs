using System;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Terrains
{
    /// <summary>Resolution and extent of the square terrain grid.</summary>
    public sealed record TerrainGridSettings
    {
        /// <summary>Vertices per side. 255 keeps a full-resolution Unity mesh under the 16-bit index limit.</summary>
        public int Resolution { get; init; } = 161;

        /// <summary>World-space width of the terrain (the original used 255 vertices at 1.4 spacing).</summary>
        public double WorldSize { get; init; } = 357;

        public int Octaves { get; init; } = 5;

        public double Spacing => WorldSize / (Resolution - 1);

        public void Validate()
        {
            if (Resolution < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Resolution), "Resolution must be at least 3.");
            }

            if (WorldSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(WorldSize), "World size must be positive.");
            }

            if (Octaves < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Octaves), "At least one octave is required.");
            }
        }
    }

    /// <summary>
    /// Immutable square grid of heights centred on the world origin. Row-major: index = z * resolution + x.
    /// </summary>
    public sealed class HeightMap
    {
        private readonly float[] _heights;

        public HeightMap(int resolution, double spacing, float[] heights)
        {
            if (heights.Length != resolution * resolution)
            {
                throw new ArgumentException("Height count must equal resolution squared.", nameof(heights));
            }

            Resolution = resolution;
            Spacing = spacing;
            _heights = heights;

            MinHeight = float.MaxValue;
            MaxHeight = float.MinValue;
            foreach (var height in heights)
            {
                MinHeight = Math.Min(MinHeight, height);
                MaxHeight = Math.Max(MaxHeight, height);
            }
        }

        public int Resolution { get; }

        public double Spacing { get; }

        public int VertexCount => _heights.Length;

        public double WorldSize => (Resolution - 1) * Spacing;

        public float MinHeight { get; }

        public float MaxHeight { get; }

        public float this[int x, int z] => _heights[Index(x, z)];

        public float this[GridPoint point] => this[point.X, point.Z];

        public float this[int index] => _heights[index];

        public int Index(int x, int z) => z * Resolution + x;

        public GridPoint PointAt(int index) => new GridPoint(index % Resolution, index / Resolution);

        public bool Contains(int x, int z) => x >= 0 && z >= 0 && x < Resolution && z < Resolution;

        public double WorldX(int x) => x * Spacing - WorldSize / 2;

        public double WorldZ(int z) => z * Spacing - WorldSize / 2;

        public Vector3D WorldPosition(GridPoint point) =>
            new Vector3D(WorldX(point.X), this[point], WorldZ(point.Z));

        /// <summary>Grid vertex nearest to a world-space position (clamped to the grid).</summary>
        public GridPoint NearestPoint(double worldX, double worldZ) => new GridPoint(
            ClampIndex((int)Math.Round((worldX + WorldSize / 2) / Spacing)),
            ClampIndex((int)Math.Round((worldZ + WorldSize / 2) / Spacing)));

        /// <summary>Bilinearly interpolated height at a world-space position.</summary>
        public double SampleHeight(double worldX, double worldZ)
        {
            var gx = Clamp((worldX + WorldSize / 2) / Spacing, 0, Resolution - 1);
            var gz = Clamp((worldZ + WorldSize / 2) / Spacing, 0, Resolution - 1);
            var x0 = Math.Min((int)gx, Resolution - 2);
            var z0 = Math.Min((int)gz, Resolution - 2);
            var tx = gx - x0;
            var tz = gz - z0;

            var bottom = this[x0, z0] + (this[x0 + 1, z0] - this[x0, z0]) * tx;
            var top = this[x0, z0 + 1] + (this[x0 + 1, z0 + 1] - this[x0, z0 + 1]) * tx;
            return bottom + (top - bottom) * tz;
        }

        /// <summary>Slope at a vertex, in degrees, from central differences.</summary>
        public double SlopeDegrees(int x, int z)
        {
            var left = this[Math.Max(x - 1, 0), z];
            var right = this[Math.Min(x + 1, Resolution - 1), z];
            var down = this[x, Math.Max(z - 1, 0)];
            var up = this[x, Math.Min(z + 1, Resolution - 1)];

            var dx = (right - left) / (2 * Spacing);
            var dz = (up - down) / (2 * Spacing);
            return Math.Atan(Math.Sqrt(dx * dx + dz * dz)) * (180.0 / Math.PI);
        }

        private int ClampIndex(int value) => value < 0 ? 0 : value >= Resolution ? Resolution - 1 : value;

        private static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
    }
}
