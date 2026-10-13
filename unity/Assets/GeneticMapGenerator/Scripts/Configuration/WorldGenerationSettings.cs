using System;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Domain.Evolution;
using GeneticMapGenerator.Domain.Evolution.Selection;
using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Domain.Terrains;
using UnityEngine;

namespace GeneticMapGenerator.Unity.Configuration
{
    // Inspector-facing settings. Unity serialises plain fields, so these are mutable DTOs that map onto
    // the core's immutable command at the boundary.

    [Serializable]
    public sealed class TerrainTargetSettings
    {
        [Tooltip("Relative share of water; the three biome weights are normalised.")]
        [Min(0)] public float water = 0.3f;

        [Min(0)] public float grassland = 0.5f;

        [Min(0)] public float mountain = 0.2f;

        [Range(0, 30)] public int peaks = 5;

        public TerrainTargets ToDomain() => TerrainTargets.Create(water, grassland, mountain, peaks);
    }

    [Serializable]
    public sealed class SettlementTargetSettings
    {
        [Min(1)] public float floorArea = 20;

        [Range(1, 20)] public int buildings = 8;

        [Range(0, 80)] public int inhabitants = 20;

        [Min(1)] public float prosperity = 12;

        public SettlementTargets ToDomain() => new SettlementTargets(floorArea, buildings, inhabitants, prosperity);
    }

    [Serializable]
    public sealed class EvolutionSettingsData
    {
        [Range(2, 200)] public int populationSize = 40;

        [Range(1, 500)] public int maxGenerations = 80;

        [Range(0, 1)] public float mutationRate = 0.1f;

        [Range(0, 1)] public float crossoverRate = 0.9f;

        [Range(0, 10)] public int eliteCount = 2;

        [Range(0.5f, 1)] public float targetFitness = 0.95f;

        public SelectionMethod selection = SelectionMethod.Tournament;

        public EvolutionSettingsData()
        {
        }

        public EvolutionSettingsData(int maxGenerations, float targetFitness)
        {
            this.maxGenerations = maxGenerations;
            this.targetFitness = targetFitness;
        }

        public EvolutionSettings ApplyTo(EvolutionSettings settings) => settings with
        {
            PopulationSize = populationSize,
            MaxGenerations = maxGenerations,
            MutationRate = mutationRate,
            CrossoverRate = crossoverRate,
            EliteCount = Mathf.Min(eliteCount, populationSize - 1),
            TargetFitness = targetFitness,
            Selection = selection,
        };
    }

    [Serializable]
    public sealed class WorldGenerationSettings
    {
        [Tooltip("Seed for the first world. Regenerating picks a new seed unless 'Reuse Seed' is set.")]
        public int seed = 1;

        public bool reuseSeed;

        [Tooltip("Vertices per side; higher is more detailed and slower to evolve.")]
        [Range(17, 255)] public int resolution = 161;

        public TerrainTargetSettings terrain = new TerrainTargetSettings();

        public SettlementTargetSettings settlement = new SettlementTargetSettings();

        public EvolutionSettingsData terrainEvolution = new EvolutionSettingsData(maxGenerations: 80, targetFitness: 0.95f);

        public EvolutionSettingsData settlementEvolution = new EvolutionSettingsData(maxGenerations: 60, targetFitness: 0.97f);

        public GenerateWorldCommand ToCommand(int worldSeed)
        {
            var defaults = new GenerateWorldCommand();
            return new GenerateWorldCommand
            {
                TerrainTargets = terrain.ToDomain(),
                SettlementTargets = settlement.ToDomain(),
                Grid = defaults.Grid with { Resolution = resolution },
                TerrainEvolution = terrainEvolution.ApplyTo(defaults.TerrainEvolution),
                SettlementEvolution = settlementEvolution.ApplyTo(defaults.SettlementEvolution),
            }.WithSeed(unchecked((ulong)worldSeed));
        }
    }
}
