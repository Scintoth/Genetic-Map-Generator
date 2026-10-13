using System;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Unity.Rendering;
using GeneticMapGenerator.Unity.Views;
using UnityEngine;
using Terrain = GeneticMapGenerator.Domain.Terrains.Terrain;

namespace GeneticMapGenerator.Unity.Presentation
{
    /// <summary>
    /// Translates generation events into view updates (main thread only). Views stay passive; this is
    /// the only place that knows how domain results map onto the scene.
    /// </summary>
    public sealed class WorldPresenter
    {
        private readonly TerrainView _terrainView;
        private readonly SettlementView _settlementView;
        private readonly OrbitCameraRig _camera;
        private readonly HudModel _hud;

        private Terrain? _terrain;
        private SettlementPlan? _settlement;
        private bool _focusedOnSettlement;

        public WorldPresenter(TerrainView terrainView, SettlementView settlementView, OrbitCameraRig camera, HudModel hud)
        {
            _terrainView = terrainView;
            _settlementView = settlementView;
            _camera = camera;
            _hud = hud;
        }

        public void ShowStarted(int seed)
        {
            _settlementView.Clear();
            _settlement = null;
            _focusedOnSettlement = false;
            _hud.Seed = seed;
            _hud.IsRunning = true;
            _hud.Phase = "Evolving terrain";
            _hud.Status = string.Empty;
            _hud.Generation = 0;
            _hud.Fitness = Fitness.Zero;
        }

        public void ShowTerrainProgress(EvolutionSnapshot<Terrain> snapshot)
        {
            var firstTerrain = _terrain == null;
            _terrain = snapshot.Best;
            _terrainView.Render(snapshot.Best);
            UpdateHud("Evolving terrain", snapshot.Generation, snapshot.MaxGenerations, snapshot.Statistics.AverageScore, snapshot.BestFitness);

            var proportions = snapshot.Best.Biomes.Proportions;
            _hud.Details = new[]
            {
                $"water {proportions.Water:P0} · grass {proportions.Grassland:P0} · mountain {proportions.Mountain:P0}",
                $"{snapshot.Best.Peaks.Count} peaks",
            };

            if (firstTerrain)
            {
                FrameTerrain(immediately: true);
            }

            if (snapshot.IsFinal)
            {
                _hud.Status = $"Terrain done: {snapshot.TerminationReason}.";
            }
        }

        public void ShowSettlementProgress(EvolutionSnapshot<SettlementPlan> snapshot)
        {
            var firstSettlement = _settlement == null;
            _settlement = snapshot.Best;
            _settlementView.Render(snapshot.Best);
            UpdateHud("Evolving settlement", snapshot.Generation, snapshot.MaxGenerations, snapshot.Statistics.AverageScore, snapshot.BestFitness);
            _hud.Details = new[]
            {
                $"{snapshot.Best.Buildings.Count} buildings · {snapshot.Best.TotalResidents} residents",
                $"floor area {snapshot.Best.TotalFloorArea:0.0} · prosperity {snapshot.Best.TotalProsperity:0.0}",
            };

            if (firstSettlement)
            {
                FrameSettlement();
            }
        }

        public void ShowCompleted(WorldGenerationResult result)
        {
            _hud.IsRunning = false;
            _hud.Phase = "Complete";
            _hud.Status = $"Terrain {result.TerrainFitness.Score:0.000} · settlement {result.SettlementFitness.Score:0.000}";
        }

        public void ShowFailure(Exception exception)
        {
            _hud.IsRunning = false;
            _hud.Phase = "Failed";
            _hud.Status = $"<color=#ff8080>{exception.GetType().Name}: {exception.Message}</color>";
        }

        public void ToggleFocus()
        {
            if (_focusedOnSettlement || _settlement == null)
            {
                FrameTerrain(immediately: false);
            }
            else
            {
                FrameSettlement();
            }
        }

        private void FrameTerrain(bool immediately)
        {
            if (_terrain == null)
            {
                return;
            }

            _focusedOnSettlement = false;
            var heightMap = _terrain.HeightMap;
            var focus = new Vector3(0, (float)(heightMap.MinHeight + heightMap.MaxHeight) / 4, 0);
            _camera.Frame(focus, (float)heightMap.WorldSize * 0.95f, immediately);
        }

        private void FrameSettlement()
        {
            if (_settlement == null)
            {
                return;
            }

            _focusedOnSettlement = true;
            _camera.Frame(_settlement.Centre.ToUnity(), Mathf.Max(25f, (float)_settlement.Radius * 7f));
        }

        private void UpdateHud(string phase, int generation, int maxGenerations, double averageScore, Fitness fitness)
        {
            _hud.Phase = phase;
            _hud.Generation = generation;
            _hud.MaxGenerations = maxGenerations;
            _hud.AverageScore = averageScore;
            _hud.Fitness = fitness;
        }
    }
}
