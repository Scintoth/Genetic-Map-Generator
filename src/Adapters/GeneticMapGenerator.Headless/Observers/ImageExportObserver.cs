using System.Globalization;
using System.Text;
using GeneticMapGenerator.Application.Observers;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Terrains;
using GeneticMapGenerator.Headless.Rendering;

namespace GeneticMapGenerator.Headless.Observers;

/// <summary>
/// Persists the run: periodic terrain snapshots while evolving, then the final overview, a close-up
/// of the settlement and the fitness history as CSV.
/// </summary>
public sealed class ImageExportObserver(string outputDirectory, int snapshotEvery, WorldImageRenderer renderer, TextWriter log)
    : WorldGenerationObserver
{
    public override void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot)
    {
        if (snapshotEvery > 0 && (snapshot.Generation % snapshotEvery == 0 || snapshot.IsFinal))
        {
            Save(renderer.RenderOverview(snapshot.Best, pixelsPerVertex: 2), Path.Combine("snapshots", $"terrain-gen-{snapshot.Generation:D3}.png"));
        }
    }

    public override void OnCompleted(WorldGenerationResult result)
    {
        Save(renderer.RenderOverview(result.Terrain, result.Settlement), "world.png");
        Save(renderer.RenderSettlement(result.Terrain, result.Settlement), "settlement.png");
        WriteHistory(result);
        log.WriteLine($"Output written to {Path.GetFullPath(outputDirectory)}");
    }

    private void Save(RgbImage image, string relativePath) =>
        PngEncoder.Save(image, Path.Combine(outputDirectory, relativePath));

    private void WriteHistory(WorldGenerationResult result)
    {
        var csv = new StringBuilder("stage,generation,best,average\n");
        Append("terrain", result.TerrainHistory);
        Append("settlement", result.SettlementHistory);
        File.WriteAllText(Path.Combine(outputDirectory, "fitness.csv"), csv.ToString());

        void Append(string stage, IEnumerable<GenerationStatistics> history)
        {
            foreach (var entry in history)
            {
                csv.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{stage},{entry.Generation},{entry.BestScore:0.#####},{entry.AverageScore:0.#####}"));
            }
        }
    }
}
