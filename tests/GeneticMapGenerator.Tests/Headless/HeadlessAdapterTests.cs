using System.Buffers.Binary;
using System.IO.Compression;
using FluentAssertions;
using GeneticMapGenerator.Domain.Evolution.Selection;
using GeneticMapGenerator.Headless.Cli;
using GeneticMapGenerator.Headless.Rendering;
using GeneticMapGenerator.Tests.Support;
using NUnit.Framework;

namespace GeneticMapGenerator.Tests.Headless;

public class CliOptionsTests
{
    [Test]
    public void Parse_MapsOptionsOntoTheCommand()
    {
        var command = CliOptions.Parse(
        [
            "--seed", "7", "--water", "1", "--grassland", "1", "--mountain", "2", "--peaks", "4",
            "--population", "12", "--generations", "9", "--selection", "best-and-worst", "--resolution", "65",
        ]).ToCommand();

        command.TerrainTargets.Mountain.Should().Be(0.5);
        command.TerrainTargets.PeakCount.Should().Be(4);
        command.TerrainEvolution.Seed.Should().Be(7);
        command.TerrainEvolution.PopulationSize.Should().Be(12);
        command.SettlementEvolution.PopulationSize.Should().Be(12);
        command.TerrainEvolution.MaxGenerations.Should().Be(9);
        command.TerrainEvolution.Selection.Should().Be(SelectionMethod.BestAndWorst);
        command.Grid.Resolution.Should().Be(65);
    }

    [TestCase("--unknown")]
    [TestCase("--seed")]
    [TestCase("--seed", "abc")]
    [TestCase("--selection", "lottery")]
    public void Parse_RejectsInvalidInput(params string[] args)
    {
        var act = () => CliOptions.Parse(args);

        act.Should().Throw<CliException>();
    }

    [TestCase("--resolution", "300")]
    [TestCase("--water", "-1")]
    [TestCase("--buildings", "0")]
    public void ToCommand_RejectsInvalidValues(params string[] args)
    {
        var act = () => CliOptions.Parse(args).ToCommand();

        act.Should().Throw<CliException>();
    }
}

public class RenderingTests
{
    [Test]
    public void PngEncoder_WritesAValidRgbImage()
    {
        var image = new RgbImage(4, 3);
        image[1, 2] = new Rgb(10, 20, 30);
        using var stream = new MemoryStream();

        PngEncoder.Write(image, stream);

        var bytes = stream.ToArray();
        bytes.Take(8).Should().Equal(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);
        BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16)).Should().Be(4);
        BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20)).Should().Be(3);

        var pixels = Inflate(bytes);
        pixels.Should().HaveCount(3 * (1 + 4 * 3));
        pixels.Skip(2 * 13 + 1 + 3).Take(3).Should().Equal(10, 20, 30);
    }

    [Test]
    public void Renderer_ProducesImagesOfTheRequestedSize()
    {
        var terrain = TestWorlds.Island();
        var plan = new GeneticMapGenerator.Domain.Settlements.OrganicSettlementPlanner().Plan(TestWorlds.Settlement(), terrain);
        var renderer = new WorldImageRenderer();

        renderer.RenderOverview(terrain, plan, pixelsPerVertex: 2).Width.Should().Be(128);
        renderer.RenderSettlement(terrain, plan, size: 200).Height.Should().Be(200);
    }

    private static byte[] Inflate(byte[] png)
    {
        var dataLength = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(33));
        using var zlib = new ZLibStream(new MemoryStream(png, 41, dataLength), CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }
}
