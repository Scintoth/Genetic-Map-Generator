using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using GeneticAlgorithmEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Selection
{
    public interface ISelectionMethod
    {
        List<IChromosome> Select(List<IChromosome> chromosomes);

        List<IGeneInfo> Select(List<IGeneInfo> population);

    }
}
