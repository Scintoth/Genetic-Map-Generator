using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm
{
    public interface IFitnessCalculator
    {
        float CalculateFitness(List<float> values, List<float> expectedValues);
    }
}
