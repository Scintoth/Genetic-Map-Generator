using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Selection
{
    public interface ISelectionMethod
    {
        List<IChromosome> Select(List<IChromosome> chromosomes); 
    }
}
