using System.Collections.Generic;
using System.Linq;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;

namespace GeneticMapGenerator.Application.Observers
{
    /// <summary>Base class for adapters that only care about some notifications.</summary>
    public abstract class WorldGenerationObserver : IWorldGenerationObserver
    {
        public virtual void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot)
        {
        }

        public virtual void OnSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot)
        {
        }

        public virtual void OnCompleted(WorldGenerationResult result)
        {
        }
    }

    /// <summary>Null object: for callers that only want the returned result.</summary>
    public sealed class NullWorldGenerationObserver : WorldGenerationObserver
    {
        public static NullWorldGenerationObserver Instance { get; } = new NullWorldGenerationObserver();

        private NullWorldGenerationObserver()
        {
        }
    }

    /// <summary>Composite: fans each notification out to several observers in order.</summary>
    public sealed class CompositeWorldGenerationObserver : IWorldGenerationObserver
    {
        private readonly IReadOnlyList<IWorldGenerationObserver> _observers;

        public CompositeWorldGenerationObserver(IEnumerable<IWorldGenerationObserver> observers)
        {
            _observers = observers.ToArray();
        }

        public CompositeWorldGenerationObserver(params IWorldGenerationObserver[] observers)
            : this((IEnumerable<IWorldGenerationObserver>)observers)
        {
        }

        public void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot)
        {
            foreach (var observer in _observers)
            {
                observer.OnTerrainProgress(snapshot);
            }
        }

        public void OnSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot)
        {
            foreach (var observer in _observers)
            {
                observer.OnSettlementProgress(snapshot);
            }
        }

        public void OnCompleted(WorldGenerationResult result)
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted(result);
            }
        }
    }
}
