using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using GeneticAlgorithmEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Selection
{
    public class TwoFittest : ISelectionMethod
    {
        public List<IChromosome> Select(List<IChromosome> chromosomes)
        {
            var sortedPopulation = chromosomes.OrderBy(x => x.Fitness);

            return sortedPopulation.Take(2).ToList();
        }

        public List<IGeneInfo> Select(List<IGeneInfo> population)
        {
            var sortedPopulation = population.OrderBy(x => x.Fitness);

            return sortedPopulation.Take(2).ToList();
        }
    }
}
