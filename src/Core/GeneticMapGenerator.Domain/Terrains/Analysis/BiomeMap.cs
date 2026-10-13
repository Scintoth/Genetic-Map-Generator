using System;

namespace GeneticMapGenerator.Domain.Terrains.Analysis
{
    public enum Biome : byte
    {
        Water,
        Grassland,
        Mountain,
    }

    /// <summary>Share of vertices in each biome; always sums to 1.</summary>
    public sealed record BiomeProportions(double Water, double Grassland, double Mountain)
    {
        public double Of(Biome biome) => biome switch
        {
            Biome.Water => Water,
            Biome.Grassland => Grassland,
            Biome.Mountain => Mountain,
            _ => throw new ArgumentOutOfRangeException(nameof(biome)),
        };
    }

    /// <summary>Per-vertex biome classification plus the thresholds that produced it.</summary>
    public sealed class BiomeMap
    {
        private readonly Biome[] _biomes;

        public BiomeMap(Biome[] biomes, int resolution, double waterHeight, double mountainHeight)
        {
            _biomes = biomes;
            Resolution = resolution;
            WaterHeight = waterHeight;
            MountainHeight = mountainHeight;

            var counts = new int[3];
            foreach (var biome in biomes)
            {
                counts[(int)biome]++;
            }

            double total = biomes.Length;
            Proportions = new BiomeProportions(
                counts[(int)Biome.Water] / total,
                counts[(int)Biome.Grassland] / total,
                counts[(int)Biome.Mountain] / total);
        }

        public int Resolution { get; }

        public double WaterHeight { get; }

        public double MountainHeight { get; }

        public BiomeProportions Proportions { get; }

        public Biome this[int x, int z] => _biomes[z * Resolution + x];

        public Biome this[int index] => _biomes[index];
    }

    public interface IBiomeClassifier
    {
        BiomeMap Classify(HeightMap heightMap, TerrainGenome genome);
    }

    /// <summary>
    /// Water below <c>WaterLevel × highest</c>, mountain above <c>MountainLevel × highest</c>,
    /// grassland between — the original thresholds.
    /// </summary>
    public sealed class ThresholdBiomeClassifier : IBiomeClassifier
    {
        public BiomeMap Classify(HeightMap heightMap, TerrainGenome genome)
        {
            var waterHeight = genome.WaterLevel * heightMap.MaxHeight;
            var mountainHeight = genome.MountainLevel * heightMap.MaxHeight;
            var biomes = new Biome[heightMap.VertexCount];

            for (var i = 0; i < biomes.Length; i++)
            {
                var height = heightMap[i];
                biomes[i] = height < waterHeight ? Biome.Water
                    : height > mountainHeight ? Biome.Mountain
                    : Biome.Grassland;
            }

            return new BiomeMap(biomes, heightMap.Resolution, waterHeight, mountainHeight);
        }
    }
}
