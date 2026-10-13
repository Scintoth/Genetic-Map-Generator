using GeneticMapGenerator.Domain.Terrains.Noise;
using GeneticMapGenerator.Domain.Terrains.Shaping;

namespace GeneticMapGenerator.Domain.Terrains
{
    public interface IHeightMapGenerator
    {
        HeightMap Generate(TerrainGenome genome);
    }

    /// <summary>
    /// Samples fractal noise at the genome's offset and frequency, then runs each sample through the
    /// genome's shaping pipeline. Pure and allocation-light, so it is safe to call from many threads.
    /// </summary>
    public sealed class HeightMapGenerator : IHeightMapGenerator
    {
        // Scales normalised noise so typical elevations match the original summed-octave output (~Amplitude).
        private const double ElevationScale = 2.0;

        private readonly TerrainGridSettings _grid;
        private readonly FractalNoise _noise;

        public HeightMapGenerator(TerrainGridSettings grid, INoise2D basis)
        {
            grid.Validate();
            _grid = grid;
            _noise = new FractalNoise(basis, grid.Octaves);
        }

        public HeightMap Generate(TerrainGenome genome)
        {
            var resolution = _grid.Resolution;
            var spacing = _grid.Spacing;
            var halfSize = _grid.WorldSize / 2;
            var shaper = HeightShapingPipeline.For(genome);
            var offsetX = genome.OffsetX;
            var offsetZ = genome.OffsetZ;
            var heights = new float[resolution * resolution];

            for (var z = 0; z < resolution; z++)
            {
                var sampleZ = (z * spacing - halfSize + offsetZ) * genome.ZFrequency;
                for (var x = 0; x < resolution; x++)
                {
                    var sampleX = (x * spacing - halfSize + offsetX) * genome.XFrequency;
                    var raw = ElevationScale * genome.Amplitude * _noise.Sample(sampleX, sampleZ);
                    heights[z * resolution + x] = (float)shaper.Shape(raw, raw);
                }
            }

            return new HeightMap(resolution, spacing, heights);
        }
    }
}
