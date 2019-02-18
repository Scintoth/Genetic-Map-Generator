using System;
using System.Threading.Tasks;
using Assets.Entities;
using UnityEngine;

namespace Assets.Scripts.GAEngine.Map
{
    class HeightMapGenerator : IHeightMapGenerator
    {
        private IHeightCalculator _heightCalculator;

        public HeightMapGenerator(IHeightCalculator heightCalculator)
        {
            _heightCalculator = heightCalculator;
        }

        private object TriLock = new object();

        public MapGenerationResult GenerateHeightMap(MapGenerationParameters parameters)
        {
            var result = new MapGenerationResult();
            //List<Vector3[]> verts = new List<Vector3[]>();
            PointData[] verts = new PointData[parameters.Width * parameters.Width];
            var ceiling = parameters.MaximumHeight * 5;
            
            float waterHeight = 0.0f;
            float mountainHeight = 0.0f;

            float zOff = 0;
            //for (int z = 0; z < parameters.Width; z++)
            Parallel.For(0, parameters.Width, (z) =>
            {
                float xOff = 0;
                //verts.Add(new Vector3[parameters.Width]);
                //for (int x = 0; x < parameters.Width; x++)
                Parallel.For(0, parameters.Width, (x) =>
                {
                    lock (TriLock)
                    {
                        verts[(x * parameters.Width) + z] = new PointData();
                    }

                    Vector3 currentPoint = new Vector3
                    {
                        x = Math.Abs((x * parameters.Spacing) - parameters.Width), // / 2f;
                        z = Math.Abs((z * parameters.Spacing) - parameters.Width) // / 2f;
                    };

                    lock (TriLock)
                    {
                        var heightCalculationParameters = new HeightCalculationParameters
                        {
                            Genes = parameters.Genes,
                            NumberOfOctaves = parameters.NumberOfOctaves,
                            Wavelength = parameters.Wavelength,
                            XFrequency = parameters.XFrequency,
                            ZFrequency = parameters.ZFrequency,
                            XLocation = currentPoint.x,
                            ZLocation = currentPoint.z
                        };

                        float e = _heightCalculator.GetHeight(heightCalculationParameters);
                        currentPoint.y = Mathf.Pow(e, parameters.Exponent);

                        if (parameters.FlattenTerrain)
                        {
                            currentPoint.y =
                                Mathf.Clamp(currentPoint.y, -parameters.MaximumHeight, parameters.MaximumHeight) *
                                (_heightCalculator.GetHeight(heightCalculationParameters) * 0.3f);
                        }

                        if (parameters.AdditionalHills && parameters.FlattenTerrain)
                        {
                            if (Mathf.Pow(e, parameters.Exponent) > parameters.MaximumHeight * 3)
                            {
                                currentPoint.y += (Mathf.Pow(e, parameters.Exponent) - (parameters.MaximumHeight * ((parameters.Exponent + parameters.Wavelength) / 2) - 3)) / (parameters.MaximumHeight);
                            }
                            if (currentPoint.y >= ceiling)
                            {
                                currentPoint.y = Mathf.Clamp(currentPoint.y, parameters.MaximumHeight * 4, ceiling);
                            }
                        }
                    }

                    lock (TriLock)
                    {
                        result.SumOfAllVertexHeights += currentPoint.y;
                    }

                    lock (TriLock)
                    {
                        //result.Verts[x, z].Location = currentPoint;
                        verts[(x * parameters.Width) + z].Location = currentPoint;
                    }

                    lock (TriLock)
                    {
                        if (currentPoint.y >= result.HighestVertex)
                        {
                            result.HighestVertex = currentPoint.y;
                        }
                    }

                    lock (TriLock)
                    {
                        result.WaterHeight = parameters.WaterLevel * result.HighestVertex;
                        result.MountainHeight = parameters.MountainLevel * result.HighestVertex;
                    }

                    lock (TriLock)
                    {
                        verts[(x * parameters.Width) + z].XIndex = x;
                        verts[(x * parameters.Width) + z].YIndex = z;
                    }

                    // Don't generate a triangle if it would be out of bounds.
                    int current_x = x;
                    if (current_x - 1 <= 0 || z <= 0 || current_x >= parameters.Width)
                    {
                        return;
                    }

                    lock (TriLock)
                    {
                        result.Tris.Add(x + z * parameters.Width);
                        result.Tris.Add(current_x + (z - 1) * parameters.Width);
                        result.Tris.Add((current_x - 1) + (z - 1) * parameters.Width);

                        if (x - 1 <= 0 || z <= 0)
                        {
                            return;
                        }
                        result.Tris.Add(x + z * parameters.Width);
                        result.Tris.Add((current_x - 1) + (z - 1) * parameters.Width);
                        result.Tris.Add((x - 1) + z * parameters.Width);
                    
                        xOff += x / parameters.Width - 0.5f;
                        result.VertexCount++;
                    }
                });
                zOff += z / parameters.Width - 0.5f;
            });
            result.Verts = verts;
            for (var i = 0; i < parameters.Width; i++)
            {
                for (var j = 0; j < parameters.Width; j++)
                {
                    var vert = verts[(i * parameters.Width) + j];
                    var resultVert = result.Verts[(i * parameters.Width) + j];

                    if (vert.Location.y > result.MountainHeight)
                    {
                        resultVert.SetTerrain(PointData.TerrainType.Mountain);
                    }
                    if (vert.Location.y < result.WaterHeight)
                    {
                        resultVert.SetTerrain(PointData.TerrainType.Underwater);
                    }
                    if (vert.Terrain == PointData.TerrainType.Unassigned)
                    {
                        resultVert.SetTerrain(PointData.TerrainType.Grassland);
                    }

                    var upperBound = 254;
                    var lowerBound = 0;
                    for (var x = -1; x < 2; x++)
                    {
                        for (var y = -1; y < 2; y++)
                        {
                            var currentX = i + x;
                            var currentY = j + y;
                            if(x == 0 && y == 0) continue;
                            if(currentY < lowerBound || currentY > upperBound)continue;
                            if(currentX < lowerBound || currentX > upperBound) continue;

                            var thisVert = result.Verts[(currentX * parameters.Width) + currentY];
                            if (resultVert.Location.y > thisVert.Location.y)
                                if (resultVert.HighestNeighbour == null || resultVert.HighestNeighbour.Location.y < thisVert.Location.y)
                                    resultVert.HighestNeighbour = thisVert;
                            if (resultVert.Location.y < thisVert.Location.y)
                                if (resultVert.LowestNeighbour == null || resultVert.LowestNeighbour.Location.y > thisVert.Location.y)
                                    resultVert.LowestNeighbour = thisVert;
                        }
                    }
                }
            }

            return result;
        }
    }
}
