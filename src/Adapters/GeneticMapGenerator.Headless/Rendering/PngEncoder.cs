using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace GeneticMapGenerator.Headless.Rendering;

/// <summary>Dependency-free PNG writer (8-bit RGB, no filtering) so the headless host needs no imaging library.</summary>
public static class PngEncoder
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static void Save(RgbImage image, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var file = File.Create(path);
        Write(image, file);
    }

    public static void Write(RgbImage image, Stream output)
    {
        output.Write(Signature);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, image.Width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], image.Height);
        header[8] = 8;  // bit depth
        header[9] = 2;  // colour type: truecolour
        header[10] = 0; // compression
        header[11] = 0; // filter
        header[12] = 0; // interlace
        WriteChunk(output, "IHDR", header);

        WriteChunk(output, "IDAT", Compress(image));
        WriteChunk(output, "IEND", []);
    }

    private static byte[] Compress(RgbImage image)
    {
        using var buffer = new MemoryStream();
        using (var zlib = new ZLibStream(buffer, CompressionLevel.Optimal, leaveOpen: true))
        {
            var scanline = new byte[1 + image.Width * 3];
            for (var y = 0; y < image.Height; y++)
            {
                var row = image.Row(y);
                for (var x = 0; x < row.Length; x++)
                {
                    scanline[1 + x * 3] = row[x].R;
                    scanline[2 + x * 3] = row[x].G;
                    scanline[3 + x * 3] = row[x].B;
                }

                zlib.Write(scanline);
            }
        }

        return buffer.ToArray();
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        output.Write(length);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);

        var crc = UpdateCrc(UpdateCrc(0xFFFFFFFFu, typeBytes), data) ^ 0xFFFFFFFFu;
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc);
        output.Write(crcBytes);
    }

    private static uint UpdateCrc(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (var b in data)
        {
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < table.Length; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
