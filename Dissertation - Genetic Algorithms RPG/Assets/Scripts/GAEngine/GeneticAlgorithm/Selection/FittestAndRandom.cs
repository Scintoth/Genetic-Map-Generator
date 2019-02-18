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
    public class FittestAndRandom : ISelectionMethod
    {
        public FittestAndRandom()
        {
        }

        public List<IChromosome> Select(List<IChromosome> chromosomes)
        {
            var sortedPopulation = chromosomes.OrderBy(x => x.Fitness).ToList();

            var rng = RandomService.GetRandomNumberGenerator();

            var randomPopulationMember = sortedPopulation[rng.Next(1, sortedPopulation.Count)];

            return sortedPopulation.First().ListWith(randomPopulationMember);
        }

        public List<IGeneInfo> Select(List<IGeneInfo> population)
        {
            if (population.Count == 0)
                return new List<IGeneInfo>(); 
            var sortedPopulation = population.OrderBy(x => x.Fitness).ToList();

            var rng = RandomService.GetRandomNumberGenerator();
            //throw new Exception("Yo dumbass, you need to generate a population");
            var randomPopulationMember = sortedPopulation[rng.Next(1, sortedPopulation.Count)];

            return sortedPopulation.First().ListWith(randomPopulationMember);
        }
    }
}
