using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Engine
{
    public interface IGeneticAlgorithmEngine
    {
        void SetParameters(GAEngineParameters parameters, IFitnessCalculator fitnessCalculator);

        void GAUpdate();

        bool StopGenerator();

        void Reset();

        GameObject GetGameObject();
    }
}
