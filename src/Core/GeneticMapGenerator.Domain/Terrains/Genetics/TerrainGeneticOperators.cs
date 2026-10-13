using System.Linq;
using GeneticMapGenerator.Domain.Common;
using GeneticMapGenerator.Domain.Evolution;

namespace GeneticMapGenerator.Domain.Terrains.Genetics
{
    /// <summary>Creation, recombination and mutation of <see cref="TerrainGenome"/>s.</summary>
    public sealed class TerrainGeneticOperators :
        IGenomeFactory<TerrainGenome>,
        ICrossoverOperator<TerrainGenome>,
        IMutationOperator<TerrainGenome>
    {
        public TerrainGenome Create(IRandomSource random) => new TerrainGenome(
            flattenTerrain: random.NextBool(),
            additionalHills: random.NextBool(),
            noiseOffsets: Enumerable.Range(0, TerrainGeneRanges.NoiseOffsetCount)
                .Select(_ => TerrainGeneRanges.NoiseOffset.Sample(random))
                .ToArray(),
            maxHeight: TerrainGeneRanges.MaxHeight.Sample(random),
            waterLevel: TerrainGeneRanges.WaterLevel.Sample(random),
            xFrequency: TerrainGeneRanges.Frequency.Sample(random),
            zFrequency: TerrainGeneRanges.Frequency.Sample(random),
            amplitude: TerrainGeneRanges.Amplitude.Sample(random),
            exponent: TerrainGeneRanges.Exponent.Sample(random),
            mountainLevel: TerrainGeneRanges.MountainLevel.Sample(random));

        /// <summary>
        /// Uniform crossover for scalar genes; the noise offsets swap halves (single-point at the midpoint),
        /// matching the original scheme so each child inherits one parent's X offset and the other's Z.
        /// </summary>
        public (TerrainGenome First, TerrainGenome Second) Cross(TerrainGenome first, TerrainGenome second, IRandomSource random)
        {
            var (flatten1, flatten2) = random.Shuffle(first.FlattenTerrain, second.FlattenTerrain);
            var (hills1, hills2) = random.Shuffle(first.AdditionalHills, second.AdditionalHills);
            var (height1, height2) = random.Shuffle(first.MaxHeight, second.MaxHeight);
            var (water1, water2) = random.Shuffle(first.WaterLevel, second.WaterLevel);
            var (xFrequency1, xFrequency2) = random.Shuffle(first.XFrequency, second.XFrequency);
            var (zFrequency1, zFrequency2) = random.Shuffle(first.ZFrequency, second.ZFrequency);
            var (amplitude1, amplitude2) = random.Shuffle(first.Amplitude, second.Amplitude);
            var (exponent1, exponent2) = random.Shuffle(first.Exponent, second.Exponent);
            var (mountain1, mountain2) = random.Shuffle(first.MountainLevel, second.MountainLevel);
            var (head, tail) = random.Shuffle(first, second);

            var half = TerrainGeneRanges.NoiseOffsetCount / 2;
            var offsets1 = head.NoiseOffsets.Take(half).Concat(tail.NoiseOffsets.Skip(half)).ToArray();
            var offsets2 = tail.NoiseOffsets.Take(half).Concat(head.NoiseOffsets.Skip(half)).ToArray();

            return (
                new TerrainGenome(flatten1, hills1, offsets1, height1, water1, xFrequency1, zFrequency1, amplitude1, exponent1, mountain1),
                new TerrainGenome(flatten2, hills2, offsets2, height2, water2, xFrequency2, zFrequency2, amplitude2, exponent2, mountain2));
        }

        public TerrainGenome Mutate(TerrainGenome genome, double rate, IRandomSource random) => new TerrainGenome(
            flattenTerrain: GeneMutation.Flag(genome.FlattenTerrain, rate, random),
            additionalHills: GeneMutation.Flag(genome.AdditionalHills, rate, random),
            noiseOffsets: genome.NoiseOffsets
                .Select(offset => GeneMutation.Integer(offset, TerrainGeneRanges.NoiseOffset, rate, random))
                .ToArray(),
            maxHeight: GeneMutation.Real(genome.MaxHeight, TerrainGeneRanges.MaxHeight, rate, random),
            waterLevel: GeneMutation.Real(genome.WaterLevel, TerrainGeneRanges.WaterLevel, rate, random),
            xFrequency: GeneMutation.Real(genome.XFrequency, TerrainGeneRanges.Frequency, rate, random),
            zFrequency: GeneMutation.Real(genome.ZFrequency, TerrainGeneRanges.Frequency, rate, random),
            amplitude: GeneMutation.Real(genome.Amplitude, TerrainGeneRanges.Amplitude, rate, random),
            exponent: GeneMutation.Real(genome.Exponent, TerrainGeneRanges.Exponent, rate, random),
            mountainLevel: GeneMutation.Real(genome.MountainLevel, TerrainGeneRanges.MountainLevel, rate, random));
    }
}
