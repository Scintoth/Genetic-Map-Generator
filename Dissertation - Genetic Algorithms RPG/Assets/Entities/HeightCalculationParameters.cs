using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticAlgorithmEngine;

namespace Assets.Entities
{
    class HeightCalculationParameters
    {
        public float XLocation { get; set; }
        public float ZLocation { get; set; }
        public List<RandomInt> Genes { get; set; }
        public int NumberOfOctaves { get; set; }
        public float Wavelength { get; set; }
        public float XFrequency { get; set; }
        public float ZFrequency { get; set; }
    }
}
