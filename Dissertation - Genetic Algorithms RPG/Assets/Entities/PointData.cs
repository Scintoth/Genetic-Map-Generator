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
        public TerrainType Terrain { get; set; }
        public bool IsPeak { get; set; }
        public Vector2 UV1 { get; set; }
        public Vector2 UV2 { get; set; }
        public Vector2 UV3 { get; set; }
        public Vector2 UV4 { get; set; }
        

        public void SetTerrain(TerrainType terrainType)
        {
            Terrain = terrainType;
            switch (terrainType)
            {
                case TerrainType.Unassigned:
                    throw new TerrainUndefinedException();
                case TerrainType.Grassland:
                    UV1 = new Vector2(0f, 1f);
                    UV2 = new Vector2(0.5f, 1f);
                    UV3 = new Vector2(0f, 0.5f);
                    UV4 = new Vector2(0.5f, 0.5f);
                    //UVy = new Vector2(0.5f, 1);
                    break;
                case TerrainType.Mountain:
                    UV1 = new Vector2(0.5f, 1f);
                    UV2 = new Vector2(1f, 1f);
                    UV3 = new Vector2(0.5f, 0.5f);
                    UV4 = new Vector2(1f, 0.5f);
                    //UVy = new Vector2(1f, 1f);
                    break;
                case TerrainType.Underwater:
                    UV1 = new Vector2(0f, 0.5f);
                    UV2 = new Vector2(0.25f, 0.5f);
                    UV3 = new Vector2(0f, 0.25f);
                    UV4 = new Vector2(0.25f, 0.25f);
                    //UVy = new Vector2(0.25f, 0.5f);
                    break;
            }

        }

        public enum TerrainType
        {
            Unassigned,
            Grassland,
            Mountain,
            Underwater
        }

        public class TerrainUndefinedException : Exception
        {
            public TerrainUndefinedException()
            {
                
            }

            public TerrainUndefinedException(string message) : base(message)
            {
                
            }
        }
    }
}