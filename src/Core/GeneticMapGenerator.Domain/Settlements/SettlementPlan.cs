using System;
using System.Collections.Generic;
using System.Linq;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Settlements
{
    /// <summary>A single building: ground-level centre, footprint, height and rotation about the up axis.</summary>
    public sealed record BuildingPlan(
        Vector3D Position,
        double Width,
        double Depth,
        double Height,
        double RotationDegrees,
        int Residents)
    {
        public double FootprintArea => Width * Depth;

        /// <summary>Original rule: prosperity grows with the square root of floor area.</summary>
        public double Prosperity => Math.Sqrt(FootprintArea);

        /// <summary>Radius of the circle enclosing the footprint, used for spacing.</summary>
        public double ClearanceRadius => ClearanceFor(Width, Depth);

        public static double ClearanceFor(double width, double depth) => 0.5 * Math.Sqrt(width * width + depth * depth);
    }

    /// <summary>The expressed phenotype of a <see cref="SettlementGenome"/>.</summary>
    public sealed class SettlementPlan
    {
        public SettlementPlan(SettlementGenome genome, Vector3D centre, double radius, IReadOnlyList<BuildingPlan> buildings)
        {
            Genome = genome;
            Centre = centre;
            Radius = radius;
            Buildings = buildings;
        }

        public SettlementGenome Genome { get; }

        public Vector3D Centre { get; }

        public double Radius { get; }

        public IReadOnlyList<BuildingPlan> Buildings { get; }

        public double TotalFloorArea => Buildings.Sum(b => b.FootprintArea);

        public double TotalProsperity => Buildings.Sum(b => b.Prosperity);

        public int TotalResidents => Buildings.Sum(b => b.Residents);
    }
}
