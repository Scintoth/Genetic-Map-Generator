using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MapGenerationResult GenerateHeightMap(MapGenerationParameters parameters)
        {
            var result = new MapGenerationResult();
            List<Vector3[]> verts = new List<Vector3[]>();
            var ceiling = parameters.MaximumHeight * 5;
            
            float waterHeight = 0.0f;
            float mountainHeight = 0.0f;

            float zOff = 0;
            for (int z = 0; z < parameters.Width; z++)
            {
                float xOff = 0;
                verts.Add(new Vector3[parameters.Width]);
                for (int x = 0; x < parameters.Width; x++)
                {

                    Vector3 currentPoint = new Vector3
                    {
                        x = (x * parameters.Spacing - parameters.Width), // / 2f;
                        z = (z * parameters.Spacing - parameters.Width) // / 2f;
                    };

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
                        currentPoint.y = Mathf.Clamp(currentPoint.y, -parameters.MaximumHeight, parameters.MaximumHeight) * (_heightCalculator.GetHeight(heightCalculationParameters) * 0.3f);
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

                    result.SumOfAllVertexHeights += currentPoint.y;

                    verts[z][x] = currentPoint;

                    if (currentPoint.y >= result.HighestVertex)
                    {
                        result.HighestVertex = currentPoint.y;
                    }

                    result.WaterHeight = parameters.WaterLevel * result.HighestVertex;
                    result.MountainHeight = parameters.MountainLevel * result.HighestVertex;


                    // Don't generate a triangle if it would be out of bounds.
                    int current_x = x;
                    if (current_x - 1 <= 0 || z <= 0 || current_x >= parameters.Width)
                    {
                        continue;
                    }

                    result.Tris.Add(x + z * parameters.Width);
                    result.Tris.Add(current_x + (z - 1) * parameters.Width);
                    result.Tris.Add((current_x - 1) + (z - 1) * parameters.Width);

                    if (x - 1 <= 0 || z <= 0)
                    {
                        continue;
                    }
                    result.Tris.Add(x + z * parameters.Width);
                    result.Tris.Add((current_x - 1) + (z - 1) * parameters.Width);
                    result.Tris.Add((x - 1) + z * parameters.Width);
                    xOff += x / parameters.Width - 0.5f;
                    result.VertexCount++;
                }
                zOff += z / parameters.Width - 0.5f;
            }
            result.Verts = verts;
            return result;
        }
    }
}
