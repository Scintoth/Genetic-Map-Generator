using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using Assets.Entities;

namespace GeneticAlgorithmEngine
{
    public interface IExpressedGeneData
    {
        Dictionary<string, object> Data { get; set; }

        string RetrieveValuesAsJson();
        object RetrieveObject();
        void SaveValues();
    }

    public class ExpressedMapData : IExpressedGeneData
    {
        public List<PointData> DataPoints;
        public int VertexCount;
        public List<int> Tris;
        public float HighestVertex;
        public float SumOfAllVertexHeights;
        public PointData[] Verts;
        public float WaterHeight;
        public float MountainHeight;

        /*public List<Vector2> potentialPeaks = new List<Vector2>();
        public PointData[,] verts;
        public List<int> tris = new List<int>();
        public Vector2[] uvs;
        public Vector2[] uv2s;
        public Vector2[] uv3s;
        public Vector2[] uv4s;*/


        public Dictionary<string, object> Data { get; set; }

        public string RetrieveValuesAsJson()
        {
            var data = new
            {
                DataPoints,
                VertexCount,
                Tris,
                HighestVertex,
                SumOfAllVertexHeights,
                Verts,
                WaterHeight,
                MountainHeight
            };

            var json = JsonUtility.ToJson(this);

            return json;
        }

        public object RetrieveObject()
        {
            return this;
        }

        public void SaveValues()
        {
            var thisClassMembers = typeof(ExpressedMapData).GetFields();
            

            foreach (var datum in Data)
            {
                var relatedMember = thisClassMembers.First(x => x.Name.ToLower() == datum.Key.ToLower());
                relatedMember.SetValue(this, datum.Value);
            }
        }
    }
}
