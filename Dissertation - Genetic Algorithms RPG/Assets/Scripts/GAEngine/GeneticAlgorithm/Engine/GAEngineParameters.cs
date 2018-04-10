using Assets.Scripts.GAEngine.GeneticAlgorithm.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm
{
    public class GAEngineParameters
    {
        public int MaxPopulation;
        public float FitnessThreshold;
        public IChromosome InitialChromosome;
        public bool UseElitism;
        public MethodOfSelection SelectionMethod;
        public float MutationRate;
        public DiContainer Container;
        public string NameOfObject;
    }

    public enum MethodOfSelection { TwoFittest, FittestAndRandom, BestAndWorst, Stochastic}
}
