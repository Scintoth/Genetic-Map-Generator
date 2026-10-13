using System;
using System.Collections.Generic;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Terrains.Analysis
{
    public interface IPeakDetector
    {
        IReadOnlyList<GridPoint> Detect(HeightMap heightMap, BiomeMap biomes);
    }

    /// <summary>
    /// Counts distinct mountains rather than surface bumps: a peak is a mountain-biome summit whose
    /// topographic prominence (height above the highest saddle linking it to taller ground) is at least
    /// <c>minimumProminenceFraction</c> of the map's relief.
    /// </summary>
    /// <remarks>
    /// Standard watershed sweep: visit vertices from highest to lowest, growing regions with union–find.
    /// When two regions meet, the vertex where they meet is the saddle; the region with the lower summit
    /// ends there and its prominence is <c>summit − saddle</c>. O(n log n) in the number of vertices.
    /// </remarks>
    public sealed class ProminencePeakDetector : IPeakDetector
    {
        private static readonly (int X, int Z)[] Neighbours =
        {
            (-1, -1), (0, -1), (1, -1),
            (-1, 0), (1, 0),
            (-1, 1), (0, 1), (1, 1),
        };

        private readonly double _minimumProminenceFraction;

        public ProminencePeakDetector(double minimumProminenceFraction)
        {
            if (minimumProminenceFraction is <= 0 or >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumProminenceFraction));
            }

            _minimumProminenceFraction = minimumProminenceFraction;
        }

        public IReadOnlyList<GridPoint> Detect(HeightMap heightMap, BiomeMap biomes)
        {
            var relief = heightMap.MaxHeight - heightMap.MinHeight;
            if (relief <= 0)
            {
                return Array.Empty<GridPoint>();
            }

            var minimumProminence = relief * _minimumProminenceFraction;
            var count = heightMap.VertexCount;
            var order = new int[count];
            var heights = new float[count];
            for (var i = 0; i < count; i++)
            {
                order[i] = i;
                heights[i] = -heightMap[i];
            }

            // Descending height; ties by index keep the sweep deterministic.
            Array.Sort(heights, order);

            var parent = new int[count];
            var summit = new int[count];
            var visited = new bool[count];
            var peaks = new List<GridPoint>();

            foreach (var index in order)
            {
                visited[index] = true;
                parent[index] = index;
                summit[index] = index;

                var saddleHeight = heightMap[index];
                var point = heightMap.PointAt(index);
                foreach (var (dx, dz) in Neighbours)
                {
                    var nx = point.X + dx;
                    var nz = point.Z + dz;
                    if (!heightMap.Contains(nx, nz))
                    {
                        continue;
                    }

                    var neighbour = heightMap.Index(nx, nz);
                    if (!visited[neighbour])
                    {
                        continue;
                    }

                    var mine = Find(parent, index);
                    var theirs = Find(parent, neighbour);
                    if (mine == theirs)
                    {
                        continue;
                    }

                    // The region with the lower summit dies at this saddle.
                    var (higher, lower) = heightMap[summit[mine]] >= heightMap[summit[theirs]] ? (mine, theirs) : (theirs, mine);
                    if (lower != index || summit[lower] != index)
                    {
                        RecordPeak(heightMap, biomes, summit[lower], heightMap[summit[lower]] - saddleHeight, minimumProminence, peaks);
                    }

                    parent[lower] = higher;
                }
            }

            // The global summit never meets taller ground; its prominence is the full relief.
            var highest = summit[Find(parent, order[0])];
            RecordPeak(heightMap, biomes, highest, relief, minimumProminence, peaks);
            return peaks;
        }

        private static void RecordPeak(HeightMap heightMap, BiomeMap biomes, int summitIndex, double prominence, double minimumProminence, List<GridPoint> peaks)
        {
            if (prominence >= minimumProminence && biomes[summitIndex] == Biome.Mountain)
            {
                peaks.Add(heightMap.PointAt(summitIndex));
            }
        }

        private static int Find(int[] parent, int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]];
                index = parent[index];
            }

            return index;
        }
    }
}
