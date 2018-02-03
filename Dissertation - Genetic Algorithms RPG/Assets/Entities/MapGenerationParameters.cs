using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Entities
{
    public class MapGenerationParameters
    {
        public float Spacing { get; set;}
        public int Width { get; set;}
        public bool AdditionalHills { get; set;}
        public bool FlattenTerrain { get; set;}
        public float MaximumHeight { get; set;}
        public float Exponent { get; set;}
        public float Wavelength { get; set; }
        public float WaterLevel { get; set;}
        public float MountainLevel { get; set;}    
        public List<int> Genes { get; set; }
        public int NumberOfOctaves { get; set; }
        public float XFrequency { get; set; }
        public float ZFrequency { get; set; }
    }
}
