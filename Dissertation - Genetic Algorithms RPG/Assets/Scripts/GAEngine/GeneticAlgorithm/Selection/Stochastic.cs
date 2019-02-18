using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using Assets.Scripts.Utilities;
using GeneticAlgorithmEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Selection
{
    public class Stochastic : ISelectionMethod
    {

        public Stochastic()
        {
        }

        public List<IChromosome> Select(List<IChromosome> chromosomes)
        {
            var rng = RandomService.GetRandomNumberGenerator();

            var totalFitness = chromosomes.Sum(x => x.Fitness);

            var selectionPool = new List<IChromosome>();

            for (var i = 0; i < chromosomes.Count; i++)
            {
                var chances = chromosomes[i].Fitness * 10;
                for (int j = 0; j < chances; j++)
                {
                    selectionPool.Add(chromosomes[i]);
                }
            }

            var returnList = selectionPool[rng.Next(selectionPool.Count)].ListWith(selectionPool[rng.Next(selectionPool.Count)]);
            return returnList;
        }

        public List<IGeneInfo> Select(List<IGeneInfo> population)
        {
            var rng = RandomService.GetRandomNumberGenerator();

            var totalFitness = population.Sum(x => x.Fitness);

            var selectionPool = new List<IGeneInfo>();

            foreach (var individual in population)
            {
                var exponent = 2;
                var modifier = 10;

                // Since Fitness is meant to sit between 0-1
                // increasing the exponent minimises low values and maximises high ones
                // values below 0.33 are given 1 chance, 0.9 gives 9 chances
                
                var chances = Math.Pow(individual.Fitness, exponent) * modifier;
                for(int j = 0; j < chances; j++)
                {
                    selectionPool.Add(individual);
                }
            }

            var returnList = selectionPool[rng.Next(selectionPool.Count)].ListWith(selectionPool[rng.Next(selectionPool.Count)]);
            return returnList;
        }
    }
}
