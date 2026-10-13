using GeneticMapGenerator.Domain.Terrains;
using UnityEngine;
using UnityEngine.Rendering;
using Terrain = GeneticMapGenerator.Domain.Terrains.Terrain;

namespace GeneticMapGenerator.Unity.Rendering
{
    /// <summary>Adapts a domain <see cref="HeightMap"/> onto a Unity <see cref="Mesh"/> (and its colour texture).</summary>
    public static class TerrainMeshBuilder
    {
        /// <summary>Writes the terrain into <paramref name="mesh"/>, reusing its buffers when the size is unchanged.</summary>
        public static void Populate(Mesh mesh, HeightMap heightMap)
        {
            var resolution = heightMap.Resolution;
            var vertexCount = resolution * resolution;
            var vertices = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var uvScale = 1f / (resolution - 1);

            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var index = heightMap.Index(x, z);
                    vertices[index] = new Vector3((float)heightMap.WorldX(x), heightMap[index], (float)heightMap.WorldZ(z));
                    uvs[index] = new Vector2(x * uvScale, z * uvScale);
                }
            }

            var sameTopology = mesh.vertexCount == vertexCount;
            if (!sameTopology)
            {
                mesh.Clear();
                mesh.indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            if (!sameTopology)
            {
                mesh.triangles = Triangles(resolution);
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        public static void Paint(Texture2D texture, Terrain terrain)
        {
            var heightMap = terrain.HeightMap;
            var pixels = new Color32[heightMap.VertexCount];
            for (var i = 0; i < pixels.Length; i++)
            {
                // Texture rows run bottom-to-top, matching the grid's z order, so indices line up directly.
                pixels[i] = TerrainPalette.ColourAt(terrain, heightMap[i]);
            }

            texture.SetPixels32(pixels);
            texture.Apply(updateMipmaps: false);
        }

        private static int[] Triangles(int resolution)
        {
            var cells = resolution - 1;
            var triangles = new int[cells * cells * 6];
            var t = 0;
            for (var z = 0; z < cells; z++)
            {
                for (var x = 0; x < cells; x++)
                {
                    var i = z * resolution + x;

                    // Clockwise seen from above, so faces point up in Unity's left-handed space.
                    triangles[t++] = i;
                    triangles[t++] = i + resolution;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + resolution;
                    triangles[t++] = i + resolution + 1;
                }
            }

            return triangles;
        }
    }
}
