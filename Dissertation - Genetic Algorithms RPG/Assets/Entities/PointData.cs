using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Entities
{
    public class PointData
    {
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public Vector3 Location { get; set; }
        public PointData HighestNeighbour { get; set; }
        public PointData LowestNeighbour { get; set; }
        public float DeviationFromAverageFlatness { get; set; }
    }
}
