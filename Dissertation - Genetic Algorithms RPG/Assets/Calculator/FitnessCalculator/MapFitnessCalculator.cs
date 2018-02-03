using Assets.Scripts.GAEngine.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Calculator
{
    class MapFitnessCalculator : IFitnessCalculator
    {
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

        public float CalculateFitness(List<float> values, List<float> expectedValues)
        {
            List<float> fitnesses = new List<float>();

            for(int i = 0; i < values.Count; i++)
            {
                var mathableValue = (decimal)values[i] / (decimal)expectedValues[i];
                mathableValue = (decimal)Math.Tanh(Math.Pow(0.3 / (double)Math.Abs(mathableValue - 1), 1.57));
                fitnesses.Add((float)mathableValue);
            }

            var fitness = GAFunctions.Average(fitnesses.ToArray());
            return fitness;
        }
    }
}
