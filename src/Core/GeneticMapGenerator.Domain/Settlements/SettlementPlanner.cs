using System;
using System.Collections.Generic;
using System.Linq;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Terrains;
using GeneticMapGenerator.Domain.Terrains.Analysis;

namespace GeneticMapGenerator.Domain.Settlements
{
    /// <summary>Expresses a settlement genome as a concrete layout on a given terrain.</summary>
    public interface ISettlementPlanner
    {
        SettlementPlan Plan(SettlementGenome genome, Terrain terrain);
    }

    public sealed record SettlementPlannerSettings
    {
        public Interval BuildingFootprintSide { get; init; } = new Interval(0.5, 3);

        public Interval BuildingHeight { get; init; } = new Interval(1, 5);

        public int MaxResidentsPerBuilding { get; init; } = 4;

        public int PlacementAttempts { get; init; } = 16;
    }

    /// <summary>
    /// Organic growth layout from the original design: the first buildings cluster round the chosen site,
    /// later ones grow outwards from existing buildings. A building is only placed on buildable land,
    /// without overlapping its neighbours, while the prosperity budget lasts. Residents are drawn from the
    /// inhabitant budget. All randomness comes from the genome's layout seed, so layouts are reproducible.
    /// </summary>
    public sealed class OrganicSettlementPlanner : ISettlementPlanner
    {
        private const int SeedBuildings = 2;

        private readonly SettlementPlannerSettings _settings;

        public OrganicSettlementPlanner(SettlementPlannerSettings? settings = null)
        {
            _settings = settings ?? new SettlementPlannerSettings();
        }

        public SettlementPlan Plan(SettlementGenome genome, Terrain terrain)
        {
            var heightMap = terrain.HeightMap;
            var sites = CandidateSites(terrain);
            var radius = Math.Sqrt(genome.Area / Math.PI);

            if (sites.Count == 0)
            {
                return new SettlementPlan(genome, new Vector3D(0, heightMap.SampleHeight(0, 0), 0), radius, Array.Empty<BuildingPlan>());
            }

            var siteIndex = Math.Min((int)(genome.SiteSelector * sites.Count), sites.Count - 1);
            var centre = heightMap.WorldPosition(sites[siteIndex]);
            var random = new SplitMix64Random(genome.LayoutSeed);
            var buildings = new List<BuildingPlan>();
            var prosperityLeft = genome.ProsperityBudget;
            var residentsLeft = genome.InhabitantBudget;

            for (var i = 0; i < genome.BuildingCount && prosperityLeft > 0; i++)
            {
                var width = _settings.BuildingFootprintSide.Sample(random);
                var depth = _settings.BuildingFootprintSide.Sample(random);
                var height = _settings.BuildingHeight.Sample(random);
                var rotation = random.NextDouble(0, 360);
                var residents = Math.Min(random.NextInt(0, _settings.MaxResidentsPerBuilding + 1), residentsLeft);

                var growsFromSettlement = buildings.Count >= SeedBuildings;
                var anchor = growsFromSettlement ? random.Pick(buildings).Position : centre;
                var spread = growsFromSettlement ? radius * 2 : radius;

                var position = FindPlot(terrain, buildings, anchor, spread, BuildingPlan.ClearanceFor(width, depth), random);
                if (position is null)
                {
                    continue;
                }

                var building = new BuildingPlan(position.Value, width, depth, height, rotation, residents);
                buildings.Add(building);
                prosperityLeft -= building.Prosperity;
                residentsLeft -= residents;
            }

            return new SettlementPlan(genome, centre, radius, buildings);
        }

        /// <summary>Prefer buildable land; degrade gracefully on terrains that have none.</summary>
        private static IReadOnlyList<GridPoint> CandidateSites(Terrain terrain)
        {
            if (terrain.BuildableLand.Sites.Count > 0)
            {
                return terrain.BuildableLand.Sites;
            }

            var heightMap = terrain.HeightMap;
            return Enumerable.Range(0, heightMap.VertexCount)
                .Where(i => terrain.Biomes[i] == Biome.Grassland)
                .Select(heightMap.PointAt)
                .ToArray();
        }

        private Vector3D? FindPlot(
            Terrain terrain,
            IReadOnlyList<BuildingPlan> existing,
            Vector3D anchor,
            double spread,
            double clearance,
            IRandomSource random)
        {
            var heightMap = terrain.HeightMap;
            var requireBuildable = terrain.BuildableLand.Sites.Count > 0;

            for (var attempt = 0; attempt < _settings.PlacementAttempts; attempt++)
            {
                // Uniform point in a disc.
                var distance = spread * Math.Sqrt(random.NextDouble());
                var angle = random.NextDouble(0, 2 * Math.PI);
                var x = anchor.X + distance * Math.Cos(angle);
                var z = anchor.Z + distance * Math.Sin(angle);

                var vertex = heightMap.NearestPoint(x, z);
                if (requireBuildable ? !terrain.BuildableLand.IsBuildable(vertex) : terrain.Biomes[vertex.X, vertex.Z] == Biome.Water)
                {
                    continue;
                }

                var candidate = new Vector3D(x, heightMap.SampleHeight(x, z), z);
                if (existing.All(b => b.Position.HorizontalDistanceTo(candidate) >= b.ClearanceRadius + clearance))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
