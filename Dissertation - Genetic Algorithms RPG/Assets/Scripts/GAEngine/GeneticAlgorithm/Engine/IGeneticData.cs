using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using Assets.Scripts.Utilities;
using Zenject;

namespace GeneticAlgorithmEngine
{
    public interface IGeneticData
    {
        Dictionary<string, IRandomisable> Data { get; set; }

        void Mutate(float mutationRate);

        void Initialise();
    }

    class HeightMapParameters : IGeneticData
    {
        public Dictionary<string, IRandomisable> Data { get; set; } = new Dictionary<string, IRandomisable>();

        public void Mutate(float mutationRate)
        {
            
            var rng = RandomService.GetRandomNumberGenerator();

            foreach (var o in Data)
            {
                
            }

            throw new System.NotImplementedException();
        }

        public void Initialise()
        {
            var frequency = new RandomFloat(300000f, 400000f);
            frequency.Value /= 10000000;
            Data.Add(HeightMapFields.MaximumHeight.ToString(), new RandomFloat(50f, 100f));
            Data.Add(HeightMapFields.WaterLevel.ToString(), new RandomFloat(0f,25f));
            Data.Add(HeightMapFields.XFrequency.ToString(), frequency);
            Data.Add(HeightMapFields.ZFrequency.ToString(), frequency);
            Data.Add(HeightMapFields.WaveLength.ToString(), new RandomFloat(5f,50f));
            Data.Add(HeightMapFields.Exponent.ToString(), new RandomFloat(4f,4f));
            Data.Add(HeightMapFields.Octaves.ToString(), new RandomInt(8,12));
            Data.Add(HeightMapFields.MountainLevel.ToString(), new  RandomFloat(75f, 100f));
            Data.Add(HeightMapFields.AdditionalHills.ToString(), new RandomBool(50));
            Data.Add(HeightMapFields.FlattenTerrain.ToString(), new  RandomBool(50));
            Data.Add(HeightMapFields.Coordinates.ToString(), new RandomList<RandomInt>(1000));


        }
    }

    public enum HeightMapFields
    {
        MaximumHeight,
        WaterLevel,
        XFrequency,
        ZFrequency,
        WaveLength,
        Exponent,
        Octaves,
        MountainLevel,
        Coordinates,
        AdditionalHills,
        FlattenTerrain
    }
}
