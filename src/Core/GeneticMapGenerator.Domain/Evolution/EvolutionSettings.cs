using System;
using GeneticMapGenerator.Domain.Evolution.Selection;

namespace GeneticMapGenerator.Domain.Evolution
{
    /// <summary>Tuning knobs for a single GA run.</summary>
    public sealed record EvolutionSettings
    {
        public int PopulationSize { get; init; } = 40;

        /// <summary>Probability that a selected pair is recombined rather than cloned.</summary>
        public double CrossoverRate { get; init; } = 0.9;

        /// <summary>Per-gene mutation probability.</summary>
        public double MutationRate { get; init; } = 0.1;

        /// <summary>Number of top individuals copied unchanged into the next generation.</summary>
        public int EliteCount { get; init; } = 2;

        public SelectionMethod Selection { get; init; } = SelectionMethod.Tournament;

        public int MaxGenerations { get; init; } = 100;

        /// <summary>Stop as soon as the best individual reaches this score.</summary>
        public double TargetFitness { get; init; } = 0.95;

        /// <summary>Stop after this many generations without improvement; 0 disables the check.</summary>
        public int StagnationLimit { get; init; }

        public ulong Seed { get; init; } = 1;

        public void Validate()
        {
            Require(PopulationSize >= 2, nameof(PopulationSize), "must be at least 2");
            Require(CrossoverRate is >= 0 and <= 1, nameof(CrossoverRate), "must be within [0, 1]");
            Require(MutationRate is >= 0 and <= 1, nameof(MutationRate), "must be within [0, 1]");
            Require(EliteCount >= 0 && EliteCount < PopulationSize, nameof(EliteCount), "must be within [0, PopulationSize)");
            Require(MaxGenerations >= 0, nameof(MaxGenerations), "must not be negative");
            Require(TargetFitness is > 0 and <= 1, nameof(TargetFitness), "must be within (0, 1]");
            Require(StagnationLimit >= 0, nameof(StagnationLimit), "must not be negative");
        }

        private static void Require(bool condition, string name, string message)
        {
            if (!condition)
            {
                throw new ArgumentOutOfRangeException(name, $"{name} {message}.");
            }
        }
    }
}
