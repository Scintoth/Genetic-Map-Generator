using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Settlements
{
    /// <summary>
    /// Heritable description of a settlement, following the original <c>Settlement</c> chromosome
    /// (area, building count, prosperity and inhabitant budgets) plus site and layout genes so a
    /// genome always expresses the same, reproducible layout.
    /// </summary>
    public sealed record SettlementGenome(
        double SiteSelector,
        double Area,
        int BuildingCount,
        double ProsperityBudget,
        int InhabitantBudget,
        ulong LayoutSeed);

    public static class SettlementGeneRanges
    {
        /// <summary>Fractional position in the terrain's list of buildable sites.</summary>
        public static Interval SiteSelector { get; } = new Interval(0, 1);

        /// <summary>Land claimed by the settlement; sets how far buildings spread.</summary>
        public static Interval Area { get; } = new Interval(9, 180);

        public static IntInterval BuildingCount { get; } = new IntInterval(1, 20);

        public static Interval ProsperityBudget { get; } = new Interval(3, 14);

        public static IntInterval InhabitantBudget { get; } = new IntInterval(1, 40);
    }
}
