using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Entities;
using UnityEngine;

namespace Assets.Scripts.GAEngine.Map
{
    class PerlinHeightCalculator : IHeightCalculator
    {
        public float GetHeight(HeightCalculationParameters parameters)
        {
            int leftGeneSum = 0;
            int rightGeneSum = 0;
            for (int i = 0; i < parameters.Genes.Count; ++i)
            {
                if (i < (parameters.Genes.Count / 2))
                {
                    leftGeneSum += parameters.Genes[i];
                }
                else
                {
                    rightGeneSum += parameters.Genes[i];
                }
            }
            float returnVal = parameters.Wavelength * Mathf.PerlinNoise(parameters.XFrequency * (parameters.XLocation + leftGeneSum),
                                parameters.ZFrequency * (parameters.ZLocation + rightGeneSum));
            for (int i = 1; i < parameters.NumberOfOctaves; i++)
            {
                returnVal += (1 / Mathf.Pow(2, i)) 
                                * parameters.Wavelength 
                                * Mathf.PerlinNoise(i * Mathf.Sin(parameters.XFrequency)
                                * (parameters.XLocation - leftGeneSum)
                                ,
                                (i * Mathf.Sin(parameters.ZFrequency)) 
                                * (parameters.ZLocation - rightGeneSum));
            }
            return returnVal;
        }
    }
}
