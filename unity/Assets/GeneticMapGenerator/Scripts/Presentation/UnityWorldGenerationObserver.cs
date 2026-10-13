using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;
using GeneticMapGenerator.Unity.Threading;

namespace GeneticMapGenerator.Unity.Presentation
{
    /// <summary>
    /// Driven adapter for the core's observer port. Runs on the generator thread and marshals to the
    /// main thread; progress updates are coalesced so rendering never falls behind evolution.
    /// </summary>
    public sealed class UnityWorldGenerationObserver : IWorldGenerationObserver
    {
        private const string TerrainKey = "terrain";
        private const string SettlementKey = "settlement";

        private readonly WorldPresenter _presenter;
        private readonly MainThreadDispatcher _dispatcher;

        public UnityWorldGenerationObserver(WorldPresenter presenter, MainThreadDispatcher dispatcher)
        {
            _presenter = presenter;
            _dispatcher = dispatcher;
        }

        public void OnTerrainProgress(EvolutionSnapshot<Terrain> snapshot) =>
            _dispatcher.Post(snapshot.IsFinal ? null : TerrainKey, () => _presenter.ShowTerrainProgress(snapshot));

        public void OnSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot) =>
            _dispatcher.Post(snapshot.IsFinal ? null : SettlementKey, () => _presenter.ShowSettlementProgress(snapshot));

        public void OnCompleted(WorldGenerationResult result) =>
            _dispatcher.Post(() => _presenter.ShowCompleted(result));
    }
}
