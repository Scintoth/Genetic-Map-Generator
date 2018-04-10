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
        public List<PointData> DataPoints { get; set; } = new List<PointData>();
        public int VertexCount { get; set;}
        public List<int> Tris { get; set; } = new List<int>();
        public float HighestVertex { get; set; }
        public float SumOfAllVertexHeights { get; set; }
        public List<Vector3[]> Verts { get; set; } = new List<Vector3[]>();
        public float WaterHeight { get; set; }
        public float MountainHeight { get; set; }
    }
}
