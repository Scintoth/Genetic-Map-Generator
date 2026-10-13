using System;

namespace GeneticMapGenerator.Domain.Common
{
    /// <summary>Engine-agnostic 3D vector (Y is up). Adapters map it onto their own vector types.</summary>
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public double HorizontalDistanceTo(Vector3D other)
        {
            var dx = X - other.X;
            var dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        public Vector3D WithY(double y) => new Vector3D(X, y, Z);

        public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public bool Equals(Vector3D other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        public override bool Equals(object? obj) => obj is Vector3D other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override string ToString() => $"({X:0.##}, {Y:0.##}, {Z:0.##})";
    }

    /// <summary>Integer coordinate of a vertex on the terrain grid.</summary>
    public readonly struct GridPoint : IEquatable<GridPoint>
    {
        public GridPoint(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }

        public int Z { get; }

        public bool Equals(GridPoint other) => X == other.X && Z == other.Z;

        public override bool Equals(object? obj) => obj is GridPoint other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Z);

        public static bool operator ==(GridPoint a, GridPoint b) => a.Equals(b);

        public static bool operator !=(GridPoint a, GridPoint b) => !a.Equals(b);

        public override string ToString() => $"[{X}, {Z}]";
    }
}
