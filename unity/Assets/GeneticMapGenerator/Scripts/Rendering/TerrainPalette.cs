using UnityEngine;
using Terrain = GeneticMapGenerator.Domain.Terrains.Terrain;

namespace GeneticMapGenerator.Unity.Rendering
{
    /// <summary>Biome colouring by elevation. Lighting comes from the scene, so no shading is baked in.</summary>
    public static class TerrainPalette
    {
        private static readonly Color32 DeepWater = new Color32(0x1B, 0x49, 0x65, 0xFF);
        private static readonly Color32 ShallowWater = new Color32(0x5F, 0xA8, 0xD3, 0xFF);
        private static readonly Color32 Sand = new Color32(0xE9, 0xD8, 0xA6, 0xFF);
        private static readonly Color32 Lowland = new Color32(0x8A, 0xB1, 0x7D, 0xFF);
        private static readonly Color32 Highland = new Color32(0x4F, 0x77, 0x2D, 0xFF);
        private static readonly Color32 Rock = new Color32(0x7D, 0x74, 0x61, 0xFF);
        private static readonly Color32 Snow = new Color32(0xF4, 0xF4, 0xF4, 0xFF);

        public static readonly Color Water = new Color(0.22f, 0.52f, 0.75f, 0.72f);
        public static readonly Color Roof = new Color32(0xB5, 0x52, 0x3B, 0xFF);
        public static readonly Color Walls = new Color32(0xE8, 0xDD, 0xC8, 0xFF);
        public static readonly Color Resident = new Color32(0x2B, 0x2D, 0x42, 0xFF);
        public static readonly Color Peak = new Color32(0xD6, 0x28, 0x28, 0xFF);

        public static Color32 ColourAt(Terrain terrain, float height)
        {
            var water = (float)terrain.Biomes.WaterHeight;
            var mountain = (float)terrain.Biomes.MountainHeight;
            var highest = terrain.HeightMap.MaxHeight;

            if (height < water)
            {
                var depth = (water - height) / Mathf.Max(water, 1e-6f);
                return Color32.Lerp(ShallowWater, DeepWater, depth * 1.5f);
            }

            if (height <= mountain)
            {
                var t = (height - water) / Mathf.Max(mountain - water, 1e-6f);
                return t < 0.03f ? Sand : Color32.Lerp(Lowland, Highland, t);
            }

            var rockiness = (height - mountain) / Mathf.Max(highest - mountain, 1e-6f);
            return rockiness > 0.55f ? Color32.Lerp(Rock, Snow, (rockiness - 0.55f) / 0.3f) : Rock;
        }
    }
}
