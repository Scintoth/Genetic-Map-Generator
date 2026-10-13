using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticMapGenerator.Domain.Evolution.Termination
{
    /// <summary>Snapshot of a run used to decide whether it should stop.</summary>
    public sealed record EvolutionProgress(int Generation, double BestScore, int GenerationsWithoutImprovement);

    /// <summary>Specification deciding when evolution stops.</summary>
    public interface ITerminationCondition
    {
        bool IsSatisfiedBy(EvolutionProgress progress, out string reason);
    }

    public sealed class TargetFitnessReached : ITerminationCondition
    {
        private readonly double _target;

        public TargetFitnessReached(double target) => _target = target;

        public bool IsSatisfiedBy(EvolutionProgress progress, out string reason)
        {
            reason = $"target fitness {_target:0.###} reached";
            return progress.BestScore >= _target;
        }
    }

    public sealed class GenerationLimitReached : ITerminationCondition
    {
        private readonly int _limit;

        public GenerationLimitReached(int limit) => _limit = limit;

        public bool IsSatisfiedBy(EvolutionProgress progress, out string reason)
        {
            reason = $"generation limit {_limit} reached";
            return progress.Generation >= _limit;
        }
    }

    public sealed class StagnationDetected : ITerminationCondition
    {
        private readonly int _limit;

        public StagnationDetected(int limit) => _limit = limit;

        public bool IsSatisfiedBy(EvolutionProgress progress, out string reason)
        {
            reason = $"no improvement for {_limit} generations";
            return progress.GenerationsWithoutImprovement >= _limit;
        }
    }

    /// <summary>Composite: satisfied when any child condition is.</summary>
    public sealed class AnyTerminationCondition : ITerminationCondition
    {
        private readonly IReadOnlyList<ITerminationCondition> _conditions;

        public AnyTerminationCondition(IEnumerable<ITerminationCondition> conditions)
        {
            _conditions = conditions.ToArray();
        }

        public bool IsSatisfiedBy(EvolutionProgress progress, out string reason)
        {
            foreach (var condition in _conditions)
            {
                if (condition.IsSatisfiedBy(progress, out reason))
                {
                    return true;
                }
            }

            reason = string.Empty;
            return false;
        }

        public static ITerminationCondition FromSettings(EvolutionSettings settings)
        {
            var conditions = new List<ITerminationCondition>
            {
                new TargetFitnessReached(settings.TargetFitness),
                new GenerationLimitReached(settings.MaxGenerations),
            };

            if (settings.StagnationLimit > 0)
            {
                conditions.Add(new StagnationDetected(settings.StagnationLimit));
            }

            return new AnyTerminationCondition(conditions);
        }
    }
}
