using System;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Scoring;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Domain.Settlements
{
    /// <summary>What the user asked for in a settlement (the original <c>SettlementParameters</c>).</summary>
    public sealed record SettlementTargets
    {
        public SettlementTargets(double floorArea, int buildingCount, int inhabitants, double prosperity)
        {
            if (floorArea <= 0 || buildingCount <= 0 || inhabitants < 0 || prosperity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(floorArea), "Settlement targets must be positive.");
            }

            FloorArea = floorArea;
            BuildingCount = buildingCount;
            Inhabitants = inhabitants;
            Prosperity = prosperity;
        }

        public double FloorArea { get; }

        public int BuildingCount { get; }

        public int Inhabitants { get; }

        public double Prosperity { get; }

        public static SettlementTargets Default { get; } = new SettlementTargets(floorArea: 20, buildingCount: 8, inhabitants: 20, prosperity: 12);
    }

    /// <summary>Scores a settlement layout against <see cref="SettlementTargets"/> on a fixed terrain.</summary>
    public sealed class SettlementFitnessFunction : IFitnessFunction<SettlementGenome>
    {
        private readonly Terrain _terrain;
        private readonly ISettlementPlanner _planner;
        private readonly SettlementTargets _targets;
        private readonly IToleranceCurve _curve;

        public SettlementFitnessFunction(Terrain terrain, ISettlementPlanner planner, SettlementTargets targets, IToleranceCurve curve)
        {
            _terrain = terrain;
            _planner = planner;
            _targets = targets;
            _curve = curve;
        }

        public Fitness Evaluate(SettlementGenome genome) => Score(_planner.Plan(genome, _terrain));

        public Fitness Score(SettlementPlan plan) => Fitness.MeanOf(
            new FitnessComponent("Floor area", _curve.Score(plan.TotalFloorArea, _targets.FloorArea)),
            new FitnessComponent("Buildings", _curve.Score(plan.Buildings.Count, _targets.BuildingCount)),
            new FitnessComponent("Inhabitants", _curve.Score(plan.TotalResidents, _targets.Inhabitants)),
            new FitnessComponent("Prosperity", _curve.Score(plan.TotalProsperity, _targets.Prosperity)));
    }
}
