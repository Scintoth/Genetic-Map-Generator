using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithmEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm
{
    public interface IFitnessCalculator
    {
        float CalculateFitness(List<float> values);
        float CalculateFitness(IExpressedGeneData expressedGeneData);
    }
}
