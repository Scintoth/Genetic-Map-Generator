using System.Globalization;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Evolution.Selection;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Headless.Cli;

/// <summary>Parsed command line. Maps onto the core's <see cref="GenerateWorldCommand"/>.</summary>
public sealed record CliOptions
{
    public ulong Seed { get; init; } = 1;
    public double Water { get; init; } = TerrainTargets.Default.Water;
    public double Grassland { get; init; } = TerrainTargets.Default.Grassland;
    public double Mountain { get; init; } = TerrainTargets.Default.Mountain;
    public int Peaks { get; init; } = TerrainTargets.Default.PeakCount;
    public double FloorArea { get; init; } = SettlementTargets.Default.FloorArea;
    public int Buildings { get; init; } = SettlementTargets.Default.BuildingCount;
    public int Inhabitants { get; init; } = SettlementTargets.Default.Inhabitants;
    public double Prosperity { get; init; } = SettlementTargets.Default.Prosperity;
    public int? Population { get; init; }
    public int? TerrainGenerations { get; init; }
    public int? SettlementGenerations { get; init; }
    public double? MutationRate { get; init; }
    public SelectionMethod? Selection { get; init; }
    public int Resolution { get; init; } = new TerrainGridSettings().Resolution;
    public string OutputDirectory { get; init; } = "output";
    public int SnapshotEvery { get; init; } = 10;
    public bool Sequential { get; init; }
    public bool ShowHelp { get; init; }

    public const string Usage = """
        Usage: genmap [options]

        Evolves a terrain towards a biome mix and peak count, then evolves a settlement on it.
        Writes PNG renders and per-generation fitness to the output directory.

        World
          --seed <n>                    Random seed; same seed, same world (default 1)
          --water <w>                   Relative share of water (default 0.3)
          --grassland <w>               Relative share of grassland (default 0.5)
          --mountain <w>                Relative share of mountain (default 0.2)
          --peaks <n>                   Number of mountain peaks (default 5)
          --resolution <n>              Vertices per side, 3-255 (default 161)

        Settlement
          --floor-area <a>              Total building footprint (default 20)
          --buildings <n>               Number of buildings (default 8)
          --inhabitants <n>             Number of residents (default 20)
          --prosperity <p>              Total prosperity (default 12)

        Evolution
          --population <n>              Population size for both stages (default 40)
          --generations <n>             Max terrain generations (default 80)
          --settlement-generations <n>  Max settlement generations (default 60)
          --mutation-rate <p>           Per-gene mutation probability (default 0.1)
          --selection <method>          tournament | roulette-wheel | two-fittest |
                                        fittest-and-random | best-and-worst (default tournament)
          --sequential                  Evaluate on one thread

        Output
          --output <dir>                Output directory (default ./output)
          --snapshot-every <n>          Save a terrain render every n generations; 0 disables (default 10)
          -h, --help                    Show this help
        """;

    public static CliOptions Parse(IReadOnlyList<string> args)
    {
        var options = new CliOptions();
        for (var i = 0; i < args.Count; i++)
        {
            var name = args[i];
            string Value() => i + 1 < args.Count ? args[++i] : throw new CliException($"Option '{name}' expects a value.");

            options = name switch
            {
                "-h" or "--help" => options with { ShowHelp = true },
                "--seed" => options with { Seed = ParseValue<ulong>(name, Value()) },
                "--water" => options with { Water = ParseValue<double>(name, Value()) },
                "--grassland" => options with { Grassland = ParseValue<double>(name, Value()) },
                "--mountain" => options with { Mountain = ParseValue<double>(name, Value()) },
                "--peaks" => options with { Peaks = ParseValue<int>(name, Value()) },
                "--resolution" => options with { Resolution = ParseValue<int>(name, Value()) },
                "--floor-area" => options with { FloorArea = ParseValue<double>(name, Value()) },
                "--buildings" => options with { Buildings = ParseValue<int>(name, Value()) },
                "--inhabitants" => options with { Inhabitants = ParseValue<int>(name, Value()) },
                "--prosperity" => options with { Prosperity = ParseValue<double>(name, Value()) },
                "--population" => options with { Population = ParseValue<int>(name, Value()) },
                "--generations" => options with { TerrainGenerations = ParseValue<int>(name, Value()) },
                "--settlement-generations" => options with { SettlementGenerations = ParseValue<int>(name, Value()) },
                "--mutation-rate" => options with { MutationRate = ParseValue<double>(name, Value()) },
                "--selection" => options with { Selection = ParseSelection(Value()) },
                "--sequential" => options with { Sequential = true },
                "--output" => options with { OutputDirectory = Value() },
                "--snapshot-every" => options with { SnapshotEvery = ParseValue<int>(name, Value()) },
                _ => throw new CliException($"Unknown option '{name}'. Use --help for usage."),
            };
        }

        return options;
    }

    public GenerateWorldCommand ToCommand()
    {
        if (Resolution is < 3 or > 255)
        {
            throw new CliException("--resolution must be between 3 and 255.");
        }

        if (SnapshotEvery < 0)
        {
            throw new CliException("--snapshot-every must not be negative.");
        }

        try
        {
            var defaults = new GenerateWorldCommand();
            return new GenerateWorldCommand
            {
                TerrainTargets = TerrainTargets.Create(Water, Grassland, Mountain, Peaks),
                SettlementTargets = new SettlementTargets(FloorArea, Buildings, Inhabitants, Prosperity),
                Grid = defaults.Grid with { Resolution = Resolution },
                TerrainEvolution = defaults.TerrainEvolution with
                {
                    PopulationSize = Population ?? defaults.TerrainEvolution.PopulationSize,
                    MaxGenerations = TerrainGenerations ?? defaults.TerrainEvolution.MaxGenerations,
                    MutationRate = MutationRate ?? defaults.TerrainEvolution.MutationRate,
                    Selection = Selection ?? defaults.TerrainEvolution.Selection,
                },
                SettlementEvolution = defaults.SettlementEvolution with
                {
                    PopulationSize = Population ?? defaults.SettlementEvolution.PopulationSize,
                    MaxGenerations = SettlementGenerations ?? defaults.SettlementEvolution.MaxGenerations,
                    MutationRate = MutationRate ?? defaults.SettlementEvolution.MutationRate,
                    Selection = Selection ?? defaults.SettlementEvolution.Selection,
                },
            }.WithSeed(Seed);
        }
        catch (ArgumentException exception)
        {
            throw new CliException(exception.Message);
        }
    }

    private static T ParseValue<T>(string name, string value)
        where T : IParsable<T> =>
        T.TryParse(value, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new CliException($"Option '{name}' has an invalid value '{value}'.");

    private static SelectionMethod ParseSelection(string value) =>
        Enum.TryParse<SelectionMethod>(value.Replace("-", string.Empty), ignoreCase: true, out var method)
            ? method
            : throw new CliException($"Unknown selection method '{value}'.");
}

public sealed class CliException(string message) : Exception(message);
