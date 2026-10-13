using GeneticMapGenerator.Application.Observers;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Headless.Observers;

/// <summary>Writes one line per generation plus a summary when each stage finishes.</summary>
public sealed class ConsoleProgressObserver(TextWriter output) : WorldGenerationObserver
{
    public override void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot)
    {
        var proportions = snapshot.Best.Biomes.Proportions;
        WriteProgress(
            "terrain",
            snapshot.Generation,
            snapshot.Statistics,
            $"water {proportions.Water:P0}  grass {proportions.Grassland:P0}  mountain {proportions.Mountain:P0}  peaks {snapshot.Best.Peaks.Count}");

        if (snapshot.IsFinal)
        {
            WriteSummary("Terrain", snapshot.TerminationReason, snapshot.BestFitness);
        }
    }

    public override void OnSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot)
    {
        var plan = snapshot.Best;
        WriteProgress(
            "settlement",
            snapshot.Generation,
            snapshot.Statistics,
            $"buildings {plan.Buildings.Count}  floor {plan.TotalFloorArea:0.0}  residents {plan.TotalResidents}  prosperity {plan.TotalProsperity:0.0}");

        if (snapshot.IsFinal)
        {
            WriteSummary("Settlement", snapshot.TerminationReason, snapshot.BestFitness);
        }
    }

    private void WriteProgress(string stage, int generation, GenerationStatistics statistics, string detail) =>
        output.WriteLine($"[{stage,-10}] gen {generation,3}  best {statistics.BestScore:0.000}  avg {statistics.AverageScore:0.000}  | {detail}");

    private void WriteSummary(string stage, string? reason, Fitness fitness)
    {
        output.WriteLine($"{stage} finished: {reason}.");
        output.WriteLine($"  fitness {fitness}");
        output.WriteLine();
    }
}
