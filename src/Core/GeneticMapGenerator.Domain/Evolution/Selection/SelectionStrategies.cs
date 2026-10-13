using System;
using GeneticMapGenerator.Domain.Common;

namespace GeneticMapGenerator.Domain.Evolution.Selection
{
    /// <summary>Parent-selection strategies; the first four are the schemes from the original dissertation.</summary>
    public enum SelectionMethod
    {
        Tournament,
        RouletteWheel,
        TwoFittest,
        FittestAndRandom,
        BestAndWorst,
    }

    /// <summary>Strategy for choosing a breeding pair from a ranked population.</summary>
    public interface ISelectionStrategy
    {
        (Scored<TGenome> First, Scored<TGenome> Second) SelectParents<TGenome>(Population<TGenome> population, IRandomSource random);
    }

    /// <summary>Picks the best of <c>k</c> random contenders for each parent. Robust default.</summary>
    public sealed class TournamentSelection : ISelectionStrategy
    {
        private readonly int _tournamentSize;

        public TournamentSelection(int tournamentSize = 3)
        {
            if (tournamentSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(tournamentSize));
            }

            _tournamentSize = tournamentSize;
        }

        public (Scored<TGenome> First, Scored<TGenome> Second) SelectParents<TGenome>(Population<TGenome> population, IRandomSource random) =>
            (RunTournament(population, random), RunTournament(population, random));

        private Scored<TGenome> RunTournament<TGenome>(Population<TGenome> population, IRandomSource random)
        {
            // The population is ranked, so the lowest sampled index is the fittest contender.
            var winner = population.Size - 1;
            for (var i = 0; i < _tournamentSize; i++)
            {
                winner = Math.Min(winner, random.NextInt(0, population.Size));
            }

            return population.Ranked[winner];
        }
    }

    /// <summary>Fitness-proportionate selection (the original "breeding wheel").</summary>
    public sealed class RouletteWheelSelection : ISelectionStrategy
    {
        public (Scored<TGenome> First, Scored<TGenome> Second) SelectParents<TGenome>(Population<TGenome> population, IRandomSource random) =>
            (Spin(population, random), Spin(population, random));

        private static Scored<TGenome> Spin<TGenome>(Population<TGenome> population, IRandomSource random)
        {
            var total = 0.0;
            foreach (var member in population.Ranked)
            {
                total += member.Fitness.Score;
            }

            if (total <= 0)
            {
                return random.Pick(population.Ranked);
            }

            var target = random.NextDouble() * total;
            foreach (var member in population.Ranked)
            {
                target -= member.Fitness.Score;
                if (target < 0)
                {
                    return member;
                }
            }

            return population.Worst;
        }
    }

    /// <summary>Always breeds the two fittest individuals.</summary>
    public sealed class TwoFittestSelection : ISelectionStrategy
    {
        public (Scored<TGenome> First, Scored<TGenome> Second) SelectParents<TGenome>(Population<TGenome> population, IRandomSource random) =>
            (population.Ranked[0], population.Ranked[Math.Min(1, population.Size - 1)]);
    }

    /// <summary>Breeds the fittest individual with a random other member.</summary>
    public sealed class FittestAndRandomSelection : ISelectionStrategy
    {
        public (Scored<TGenome> First, Scored<TGenome> Second) SelectParents<TGenome>(Population<TGenome> population, IRandomSource random) =>
            (population.Best, population.Size == 1 ? population.Best : population.Ranked[random.NextInt(1, population.Size)]);
    }

    /// <summary>Breeds the fittest with the least fit individual, trading exploitation for diversity.</summary>
    public sealed class BestAndWorstSelection : ISelectionStrategy
    {
        public (Scored<TGenome> First, Scored<TGenome> Second) SelectParents<TGenome>(Population<TGenome> population, IRandomSource random) =>
            (population.Best, population.Worst);
    }

    public static class SelectionStrategyFactory
    {
        public static ISelectionStrategy Create(SelectionMethod method) => method switch
        {
            SelectionMethod.Tournament => new TournamentSelection(),
            SelectionMethod.RouletteWheel => new RouletteWheelSelection(),
            SelectionMethod.TwoFittest => new TwoFittestSelection(),
            SelectionMethod.FittestAndRandom => new FittestAndRandomSelection(),
            SelectionMethod.BestAndWorst => new BestAndWorstSelection(),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown selection method."),
        };
    }
}
