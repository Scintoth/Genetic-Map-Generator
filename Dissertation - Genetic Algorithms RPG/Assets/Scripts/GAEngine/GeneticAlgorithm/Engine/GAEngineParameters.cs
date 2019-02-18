using Assets.Scripts.GAEngine.GeneticAlgorithm.Engine;
using GeneticAlgorithmEngine;
using Zenject;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm
{
    public class GAEngineParameters
    {
        public int MaxPopulation;
        public float FitnessThreshold;
        public IGeneInfo InitialChromosome;
        public bool UseElitism;
        public MethodOfSelection SelectionMethod;
        public float MutationRate;
        public DiContainer Container;
        public string NameOfObject;
    }
}
