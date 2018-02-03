using Assets.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GAEngine.Map
{
    public class MapGenerationResult
    {
        public List<PointData> DataPoints { get; set; }
        public int VertexCount { get; set;}
        public List<int> Tris { get; set; }
        public float HighestVertex { get; set; }
        public float SumOfAllVertexHeights { get; set; }
        public List<Vector3[]> Verts { get; set; }
        public float WaterHeight { get; set; }
        public float MountainHeight { get; set; }
    }
}
