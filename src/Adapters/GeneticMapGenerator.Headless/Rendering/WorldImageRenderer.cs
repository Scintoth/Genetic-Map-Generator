using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Headless.Rendering;

/// <summary>
/// Top-down, hill-shaded map renderer (north up). Every pixel samples the terrain in world space, so
/// the same code draws the whole-map overview and the zoomed settlement view.
/// </summary>
public sealed class WorldImageRenderer
{
    private static readonly Rgb DeepWater = Rgb.FromHex(0x1B4965);
    private static readonly Rgb ShallowWater = Rgb.FromHex(0x5FA8D3);
    private static readonly Rgb Sand = Rgb.FromHex(0xE9D8A6);
    private static readonly Rgb Lowland = Rgb.FromHex(0x8AB17D);
    private static readonly Rgb Highland = Rgb.FromHex(0x4F772D);
    private static readonly Rgb Rock = Rgb.FromHex(0x7D7461);
    private static readonly Rgb Snow = Rgb.FromHex(0xF4F4F4);
    private static readonly Rgb PeakMarker = Rgb.FromHex(0xD62828);
    private static readonly Rgb MarkerOutline = Rgb.FromHex(0xFFFFFF);
    private static readonly Rgb SettlementMarker = Rgb.FromHex(0xFFC300);
    private static readonly Rgb Roof = Rgb.FromHex(0xB5523B);
    private static readonly Rgb RoofOutline = Rgb.FromHex(0x3D1F17);

    // Light from the north-west, 45° up (x east, y up, z north).
    private static readonly (double X, double Y, double Z) Light = Normalise(-1, 1.4, 1);

    public RgbImage RenderOverview(Terrain terrain, SettlementPlan? settlement = null, int pixelsPerVertex = 3)
    {
        var heightMap = terrain.HeightMap;
        var size = (heightMap.Resolution - 1) * pixelsPerVertex;
        var viewport = new Viewport(-heightMap.WorldSize / 2, -heightMap.WorldSize / 2, heightMap.WorldSize);
        var image = RenderTerrain(terrain, viewport, size);

        foreach (var peak in terrain.Peaks)
        {
            var (x, y) = viewport.ToPixel(heightMap.WorldX(peak.X), heightMap.WorldZ(peak.Z), size);
            image.FillDisc(x, y, 4.5, MarkerOutline);
            image.FillDisc(x, y, 3, PeakMarker);
        }

        if (settlement is not null && settlement.Buildings.Count > 0)
        {
            DrawBuildings(image, viewport, size, settlement);
            var (x, y) = viewport.ToPixel(settlement.Centre.X, settlement.Centre.Z, size);
            image.StrokeCircle(x, y, Math.Max(10, settlement.Radius * 2.5 * size / viewport.Size), 2.5, SettlementMarker);
        }

        return image;
    }

    public RgbImage RenderSettlement(Terrain terrain, SettlementPlan settlement, int size = 640)
    {
        var extent = settlement.Buildings.Count == 0
            ? settlement.Radius
            : settlement.Buildings.Max(b => b.Position.HorizontalDistanceTo(settlement.Centre) + b.ClearanceRadius);
        var span = Math.Max(30, extent * 2.6);
        var viewport = new Viewport(settlement.Centre.X - span / 2, settlement.Centre.Z - span / 2, span);

        var image = RenderTerrain(terrain, viewport, size);
        DrawBuildings(image, viewport, size, settlement);
        return image;
    }

    private static void DrawBuildings(RgbImage image, Viewport viewport, int size, SettlementPlan settlement)
    {
        var scale = size / viewport.Size;
        foreach (var building in settlement.Buildings)
        {
            var (x, y) = viewport.ToPixel(building.Position.X, building.Position.Z, size);
            image.FillRotatedRectangle(
                x,
                y,
                Math.Max(2, building.Width * scale),
                Math.Max(2, building.Depth * scale),
                building.RotationDegrees * Math.PI / 180,
                Roof,
                RoofOutline);
        }
    }

    private static RgbImage RenderTerrain(Terrain terrain, Viewport viewport, int size)
    {
        var image = new RgbImage(size, size);
        var heightMap = terrain.HeightMap;
        var step = viewport.Size / size;

        Parallel.For(0, size, py =>
        {
            for (var px = 0; px < size; px++)
            {
                var (wx, wz) = viewport.ToWorld(px + 0.5, py + 0.5, size);
                var height = heightMap.SampleHeight(wx, wz);
                var gradientX = (heightMap.SampleHeight(wx + step, wz) - heightMap.SampleHeight(wx - step, wz)) / (2 * step);
                var gradientZ = (heightMap.SampleHeight(wx, wz + step) - heightMap.SampleHeight(wx, wz - step)) / (2 * step);
                var normal = Normalise(-gradientX, 1, -gradientZ);
                var lambert = Math.Max(0, normal.X * Light.X + normal.Y * Light.Y + normal.Z * Light.Z);

                image[px, py] = Colour(terrain, height, lambert);
            }
        });

        return image;
    }

    private static Rgb Colour(Terrain terrain, double height, double lambert)
    {
        var water = terrain.Biomes.WaterHeight;
        var mountain = terrain.Biomes.MountainHeight;
        var highest = terrain.HeightMap.MaxHeight;

        if (height < water)
        {
            var depth = (water - height) / Math.Max(water, 1e-6);
            return ShallowWater.Lerp(DeepWater, depth * 1.5).Scale(0.85 + 0.2 * lambert);
        }

        Rgb land;
        if (height <= mountain)
        {
            var t = (height - water) / Math.Max(mountain - water, 1e-6);
            land = t < 0.03 ? Sand : Lowland.Lerp(Highland, t);
        }
        else
        {
            var t = (height - mountain) / Math.Max(highest - mountain, 1e-6);
            land = t > 0.55 ? Rock.Lerp(Snow, (t - 0.55) / 0.3) : Rock;
        }

        return land.Scale(0.45 + 0.75 * lambert);
    }

    private static (double X, double Y, double Z) Normalise(double x, double y, double z)
    {
        var length = Math.Sqrt(x * x + y * y + z * z);
        return (x / length, y / length, z / length);
    }

    /// <summary>Square world-space window; image row 0 is the northern edge.</summary>
    private sealed record Viewport(double MinX, double MinZ, double Size)
    {
        public (double X, double Y) ToPixel(double worldX, double worldZ, int pixels) =>
            ((worldX - MinX) / Size * pixels, (MinZ + Size - worldZ) / Size * pixels);

        public (double X, double Z) ToWorld(double pixelX, double pixelY, int pixels) =>
            (MinX + pixelX / pixels * Size, MinZ + Size - pixelY / pixels * Size);
    }
}
