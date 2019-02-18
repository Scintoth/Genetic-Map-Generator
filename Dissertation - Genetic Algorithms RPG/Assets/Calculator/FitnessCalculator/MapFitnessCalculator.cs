using Assets.Scripts.GAEngine.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Entities;
using GeneticAlgorithmEngine;
using UnityEngine;

namespace Assets.Calculator
{
    class MapFitnessCalculator : IFitnessCalculator
    {
        private object _grassLock = new object();
        private object _mountainLock = new object();
        private object _waterLock = new object();

        public MapFitnessCalculator()
        {
            
        }

        /*public float CalculateFitness(IChromosome chromosome)
        {
            float temp = TotalVertices;
            temp /= 100;
            float tempM = MountainousVertices / 100;
            float tempG = GrasslandVertices / 100;
            float tempU = UnderwaterVertices / 100;
            float mountainPer = (tempM) / (temp);
            float grasslandPer = (tempG) / (temp);
            float underwaterPer = (tempU) / (temp);
            float peakPer = (float)numberOfPeaks / (float)MapParameters.MP.TargetPeaks;


            float mountFitness = 1 - Math.Abs(mountainPer - MapParameters.MP.Mountainous);
            float grassFitness = 1 - Math.Abs(grasslandPer - MapParameters.MP.Grassland);
            float underwaterFitness = 1 - Math.Abs(underwaterPer - MapParameters.MP.Underwater);
            float peakFitness = 1 - Math.Abs(peakPer - 1);

            // Requires work on flatness using AverageVertexHeight
            //Fitness = mountFitness + grassFitness + underwaterFitness;
            return GAFunctions.Average(mountFitness, grassFitness, underwaterFitness, peakFitness);
        }*/

        public float CalculateFitness(List<float> values)
        {
            /*List<float> fitnesses = new List<float>();

            for(int i = 0; i < values.Count; i++)
            {
                var mathableValue = (double)values[i] / (double)expectedValues[i];
                mathableValue = 1 + Math.Exp(-3 * Math.Pow(mathableValue, 2)); /*(decimal)Math.Tanh(Math.Pow(0.3 / (double)Math.Abs(mathableValue - 1), 1.57))#1#;
                fitnesses.Add((float)mathableValue);
            }*/
/*

            var fitness = GAFunctions.Average(fitnesses.ToArray());*/
            return 0;
        }

        public float CalculateFitness(IExpressedGeneData expressedGeneData/*, Dictionary<string,object> expectedData*/)
        {
            //var expressedDataAsJson = expressedGeneData.RetrieveValuesAsJson();
            //var expressedData = JsonUtility.FromJson<ExpressedMapData>(expressedDataAsJson);
            var expressedData = (ExpressedMapData)expressedGeneData.RetrieveObject();

            var grasslandPercentage = 0f;
            var mountainousPercentage = 0f;
            var underwaterPercentage = 0f;
            var numberOfVertices = (float)expressedData.Verts/*.verts*/.Length;


            Parallel.For(0, expressedData.Verts.Length, (x) =>
            {
                switch (expressedData.Verts[x].Terrain)
                {
                    case PointData.TerrainType.Grassland:
                        lock (_grassLock)
                            grasslandPercentage++;
                        break;
                    case PointData.TerrainType.Mountain:
                        lock (_mountainLock)
                            mountainousPercentage++;
                        break;
                    case PointData.TerrainType.Underwater:
                        lock (_waterLock)
                            underwaterPercentage++;
                        break;
                }
            });
            /*foreach (var vert in expressedData.Verts/*.verts#1#)
            {
                
                /*throw new NotImplementedException("Get the data from a static class (laer the UI), then pass it to the ");#1#
            }*/

            grasslandPercentage = (grasslandPercentage / numberOfVertices);
            mountainousPercentage = (mountainousPercentage / numberOfVertices);
            underwaterPercentage = (underwaterPercentage / numberOfVertices);

            var grasslandMatch = Math.Abs(grasslandPercentage / MapParameters.MP.Grassland);
            var underwaterMatch = Math.Abs(grasslandPercentage / MapParameters.MP.Underwater) ;
            var mountainousMatch = Math.Abs(grasslandPercentage / MapParameters.MP.Mountainous);
            
            var grasslandFitness = 3 * (Math.Pow(0.55 * grasslandMatch, 2) * (Math.Pow(-0.4 * grasslandMatch, 3) + 1));
            var mountatinousFitness = 3 * (Math.Pow(0.55 * mountainousMatch, 2) * (Math.Pow(-0.4 * mountainousMatch, 3) + 1));
            var waterFitness = 3 * (Math.Pow(0.55 * underwaterMatch, 2) * (Math.Pow(-0.4 * underwaterMatch, 3) + 1));

            var averageFitness = (float)(grasslandFitness + mountatinousFitness + waterFitness) / 3f;

            return averageFitness;
        }
    }
}