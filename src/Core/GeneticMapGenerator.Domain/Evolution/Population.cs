using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticMapGenerator.Domain.Evolution
{
    /// <summary>An evaluated generation, ranked fittest first.</summary>
    public sealed class Population<TGenome>
    {
        public Population(int generation, IEnumerable<Scored<TGenome>> members)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(generation));
            }

            // OrderByDescending is stable, so equal scores keep insertion order and runs stay deterministic.
            Ranked = members.OrderByDescending(m => m.Fitness.Score).ToArray();
            if (Ranked.Count == 0)
            {
                throw new ArgumentException("A population needs at least one member.", nameof(members));
            }

            Generation = generation;
            AverageScore = Ranked.Average(m => m.Fitness.Score);
        }

        public int Generation { get; }

        public IReadOnlyList<Scored<TGenome>> Ranked { get; }

        public int Size => Ranked.Count;

        public Scored<TGenome> Best => Ranked[0];

        public Scored<TGenome> Worst => Ranked[Ranked.Count - 1];

        public double AverageScore { get; }
    }
}
