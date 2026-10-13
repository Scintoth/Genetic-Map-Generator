using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Evolution;

namespace GeneticMapGenerator.Domain.Settlements.Genetics
{
    /// <summary>Creation, uniform crossover and mutation of <see cref="SettlementGenome"/>s.</summary>
    public sealed class SettlementGeneticOperators :
        IGenomeFactory<SettlementGenome>,
        ICrossoverOperator<SettlementGenome>,
        IMutationOperator<SettlementGenome>
    {
        public SettlementGenome Create(IRandomSource random) => new SettlementGenome(
            SiteSelector: SettlementGeneRanges.SiteSelector.Sample(random),
            Area: SettlementGeneRanges.Area.Sample(random),
            BuildingCount: SettlementGeneRanges.BuildingCount.Sample(random),
            ProsperityBudget: SettlementGeneRanges.ProsperityBudget.Sample(random),
            InhabitantBudget: SettlementGeneRanges.InhabitantBudget.Sample(random),
            LayoutSeed: random.NextUInt64());

        public (SettlementGenome First, SettlementGenome Second) Cross(SettlementGenome first, SettlementGenome second, IRandomSource random)
        {
            var (site1, site2) = random.Shuffle(first.SiteSelector, second.SiteSelector);
            var (area1, area2) = random.Shuffle(first.Area, second.Area);
            var (buildings1, buildings2) = random.Shuffle(first.BuildingCount, second.BuildingCount);
            var (prosperity1, prosperity2) = random.Shuffle(first.ProsperityBudget, second.ProsperityBudget);
            var (inhabitants1, inhabitants2) = random.Shuffle(first.InhabitantBudget, second.InhabitantBudget);
            var (seed1, seed2) = random.Shuffle(first.LayoutSeed, second.LayoutSeed);

            return (
                new SettlementGenome(site1, area1, buildings1, prosperity1, inhabitants1, seed1),
                new SettlementGenome(site2, area2, buildings2, prosperity2, inhabitants2, seed2));
        }

        public SettlementGenome Mutate(SettlementGenome genome, double rate, IRandomSource random) => new SettlementGenome(
            SiteSelector: GeneMutation.Real(genome.SiteSelector, SettlementGeneRanges.SiteSelector, rate, random),
            Area: GeneMutation.Real(genome.Area, SettlementGeneRanges.Area, rate, random),
            BuildingCount: GeneMutation.Integer(genome.BuildingCount, SettlementGeneRanges.BuildingCount, rate, random),
            ProsperityBudget: GeneMutation.Real(genome.ProsperityBudget, SettlementGeneRanges.ProsperityBudget, rate, random),
            InhabitantBudget: GeneMutation.Integer(genome.InhabitantBudget, SettlementGeneRanges.InhabitantBudget, rate, random),
            LayoutSeed: GeneMutation.Seed(genome.LayoutSeed, rate, random));
    }
}
