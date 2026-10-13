using GeneticMapGenerator.Unity.Rendering;
using UnityEngine;
using Terrain = GeneticMapGenerator.Domain.Terrains.Terrain;

namespace GeneticMapGenerator.Unity.Views
{
    /// <summary>Passive view of the terrain surface plus its water plane and peak markers.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class TerrainView : MonoBehaviour
    {
        private Mesh? _mesh;
        private Texture2D? _texture;
        private Transform? _water;
        private PrimitivePool? _peakMarkers;

        public void Render(Terrain terrain)
        {
            EnsureInitialised();
            var heightMap = terrain.HeightMap;

            TerrainMeshBuilder.Populate(_mesh!, heightMap);

            if (_texture!.width != heightMap.Resolution)
            {
                _texture.Reinitialize(heightMap.Resolution, heightMap.Resolution);
            }

            TerrainMeshBuilder.Paint(_texture, terrain);
            RenderWater(terrain);
            RenderPeaks(terrain);
        }

        private void RenderWater(Terrain terrain)
        {
            var size = (float)terrain.HeightMap.WorldSize;
            _water!.gameObject.SetActive(terrain.WaterHeight > terrain.HeightMap.MinHeight);
            _water.localPosition = new Vector3(0, (float)terrain.WaterHeight, 0);
            _water.localScale = new Vector3(size, size, 1);
        }

        private void RenderPeaks(Terrain terrain)
        {
            var heightMap = terrain.HeightMap;
            var markerSize = (float)heightMap.WorldSize * 0.012f;

            _peakMarkers!.BeginFrame();
            foreach (var peak in terrain.Peaks)
            {
                var marker = _peakMarkers.Next();
                marker.localPosition = heightMap.WorldPosition(peak).ToUnity() + Vector3.up * markerSize;
                marker.localScale = Vector3.one * markerSize;
            }

            _peakMarkers.EndFrame();
        }

        private void EnsureInitialised()
        {
            if (_mesh != null)
            {
                return;
            }

            _mesh = new Mesh { name = "Evolved Terrain" };
            _mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = _mesh;

            _texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false)
            {
                name = "Biome Colours",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.CreateOpaque(Color.white, _texture);

            var water = GameObject.CreatePrimitive(PrimitiveType.Quad);
            water.name = "Water";
            Destroy(water.GetComponent<Collider>());
            water.GetComponent<MeshRenderer>().sharedMaterial = MaterialFactory.CreateTransparent(TerrainPalette.Water);
            _water = water.transform;
            _water.SetParent(transform, worldPositionStays: false);
            _water.localRotation = Quaternion.Euler(90, 0, 0);

            var peaks = new GameObject("Peaks").transform;
            peaks.SetParent(transform, worldPositionStays: false);
            _peakMarkers = new PrimitivePool(PrimitiveType.Sphere, peaks, MaterialFactory.CreateOpaque(TerrainPalette.Peak));
        }
    }
}
