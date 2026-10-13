using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticMapGenerator.Domain.Evolution
{
    /// <summary>A named, normalised [0, 1] contribution to an overall fitness score.</summary>
    public sealed record FitnessComponent(string Name, double Score);

    /// <summary>
    /// Normalised fitness in [0, 1] plus the components it was derived from, so hosts can show
    /// <em>why</em> a candidate scored as it did rather than a single opaque number.
    /// </summary>
    public sealed record Fitness
    {
        private Fitness(double score, IReadOnlyList<FitnessComponent> components)
        {
            Score = score;
            Components = components;
        }

        public double Score { get; }

        public IReadOnlyList<FitnessComponent> Components { get; }

        public static Fitness Zero { get; } = new Fitness(0, Array.Empty<FitnessComponent>());

        /// <summary>Unweighted mean of the components.</summary>
        public static Fitness MeanOf(params FitnessComponent[] components)
        {
            if (components.Length == 0)
            {
                return Zero;
            }

            return new Fitness(components.Average(c => c.Score), components);
        }

        public override string ToString() =>
            $"{Score:0.000} ({string.Join(", ", Components.Select(c => $"{c.Name} {c.Score:0.00}"))})";
    }
}
