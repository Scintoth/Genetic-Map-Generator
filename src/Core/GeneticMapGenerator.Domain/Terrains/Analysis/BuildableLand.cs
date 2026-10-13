using System;
using System.Collections.Generic;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Terrains.Analysis
{
    /// <summary>Vertices where a settlement may be built: dry, below the treeline and gently sloped.</summary>
    public sealed class BuildableLand
    {
        private readonly bool[] _mask;

        public BuildableLand(bool[] mask, int resolution)
        {
            _mask = mask;
            Resolution = resolution;

            var sites = new List<GridPoint>();
            for (var i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    sites.Add(new GridPoint(i % resolution, i / resolution));
                }
            }

            Sites = sites;
        }

        public int Resolution { get; }

        public IReadOnlyList<GridPoint> Sites { get; }

        public double Proportion => _mask.Length == 0 ? 0 : (double)Sites.Count / _mask.Length;

        public bool IsBuildable(GridPoint point) => _mask[point.Z * Resolution + point.X];
    }

    public interface IBuildableLandDetector
    {
        BuildableLand Detect(HeightMap heightMap, BiomeMap biomes);
    }

    /// <summary>Grassland whose slope is at most <c>maxSlopeDegrees</c>.</summary>
    public sealed class SlopeBuildableLandDetector : IBuildableLandDetector
    {
        private readonly double _maxSlopeDegrees;

        public SlopeBuildableLandDetector(double maxSlopeDegrees)
        {
            if (maxSlopeDegrees is <= 0 or >= 90)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSlopeDegrees));
            }

            _maxSlopeDegrees = maxSlopeDegrees;
        }

        public BuildableLand Detect(HeightMap heightMap, BiomeMap biomes)
        {
            var mask = new bool[heightMap.VertexCount];
            for (var z = 0; z < heightMap.Resolution; z++)
            {
                for (var x = 0; x < heightMap.Resolution; x++)
                {
                    mask[heightMap.Index(x, z)] = biomes[x, z] == Biome.Grassland
                        && heightMap.SlopeDegrees(x, z) <= _maxSlopeDegrees;
                }
            }

            return new BuildableLand(mask, heightMap.Resolution);
        }
    }
}
