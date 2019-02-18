using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithmEngine;
using UnityEngine;

namespace Assets.Scripts.GAEngine.GeneticAlgorithm.Engine
{
    public interface IGeneticAlgorithmEngine
    {
        void SetParameters(GAEngineParameters parameters);

        IGeneInfo Update();

        bool StopGenerator();

        void Reset();

        GameObject GetGameObject();
        IExpressedGeneData GetFittest();
    }
}
