namespace GeneticMapGenerator.Headless.Rendering;

public readonly record struct Rgb(byte R, byte G, byte B)
{
    public static Rgb FromHex(uint hex) => new((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex);

    public Rgb Lerp(Rgb to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Rgb(Mix(R, to.R, t), Mix(G, to.G, t), Mix(B, to.B, t));
    }

    public Rgb Scale(double factor) =>
        new(ClampByte(R * factor), ClampByte(G * factor), ClampByte(B * factor));

    private static byte Mix(byte a, byte b, double t) => ClampByte(a + (b - a) * t);

    private static byte ClampByte(double value) => (byte)Math.Clamp(Math.Round(value), 0, 255);
}

/// <summary>Minimal mutable 24-bit raster.</summary>
public sealed class RgbImage(int width, int height)
{
    private readonly Rgb[] _pixels = new Rgb[width * height];

    public int Width { get; } = width;

    public int Height { get; } = height;

    public Rgb this[int x, int y]
    {
        get => _pixels[y * Width + x];
        set
        {
            if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            {
                _pixels[y * Width + x] = value;
            }
        }
    }

    public void FillDisc(double centreX, double centreY, double radius, Rgb colour)
    {
        var r2 = radius * radius;
        for (var y = (int)Math.Floor(centreY - radius); y <= (int)Math.Ceiling(centreY + radius); y++)
        {
            for (var x = (int)Math.Floor(centreX - radius); x <= (int)Math.Ceiling(centreX + radius); x++)
            {
                var dx = x + 0.5 - centreX;
                var dy = y + 0.5 - centreY;
                if (dx * dx + dy * dy <= r2)
                {
                    this[x, y] = colour;
                }
            }
        }
    }

    public void StrokeCircle(double centreX, double centreY, double radius, double thickness, Rgb colour)
    {
        var outer = radius + thickness / 2;
        var inner = Math.Max(0, radius - thickness / 2);
        for (var y = (int)Math.Floor(centreY - outer); y <= (int)Math.Ceiling(centreY + outer); y++)
        {
            for (var x = (int)Math.Floor(centreX - outer); x <= (int)Math.Ceiling(centreX + outer); x++)
            {
                var distance = Math.Sqrt(Math.Pow(x + 0.5 - centreX, 2) + Math.Pow(y + 0.5 - centreY, 2));
                if (distance >= inner && distance <= outer)
                {
                    this[x, y] = colour;
                }
            }
        }
    }

    /// <summary>Fills a rectangle rotated about its centre; <paramref name="outline"/> draws a 1px border.</summary>
    public void FillRotatedRectangle(double centreX, double centreY, double width, double depth, double radians, Rgb fill, Rgb outline)
    {
        var (sin, cos) = Math.SinCos(radians);
        var halfW = width / 2;
        var halfD = depth / 2;
        var reach = Math.Sqrt(halfW * halfW + halfD * halfD) + 1;

        for (var y = (int)Math.Floor(centreY - reach); y <= (int)Math.Ceiling(centreY + reach); y++)
        {
            for (var x = (int)Math.Floor(centreX - reach); x <= (int)Math.Ceiling(centreX + reach); x++)
            {
                var dx = x + 0.5 - centreX;
                var dy = y + 0.5 - centreY;
                var u = Math.Abs(dx * cos + dy * sin);
                var v = Math.Abs(-dx * sin + dy * cos);
                if (u <= halfW && v <= halfD)
                {
                    this[x, y] = u > halfW - 1 || v > halfD - 1 ? outline : fill;
                }
            }
        }
    }

    public ReadOnlySpan<Rgb> Row(int y) => _pixels.AsSpan(y * Width, Width);
}
