using GeneticMapGenerator.Domain.Settlements;
using GeneticMapGenerator.Unity.Rendering;
using UnityEngine;

namespace GeneticMapGenerator.Unity.Views
{
    /// <summary>Passive view of a settlement: a walled block with a roof per building, and a figure per resident.</summary>
    public sealed class SettlementView : MonoBehaviour
    {
        private const float RoofThickness = 0.35f;
        private const float ResidentHeight = 0.45f;

        private PrimitivePool? _walls;
        private PrimitivePool? _roofs;
        private PrimitivePool? _residents;

        public void Render(SettlementPlan plan)
        {
            EnsureInitialised();
            BeginFrame();

            foreach (var building in plan.Buildings)
            {
                var ground = building.Position.ToUnity();
                var rotation = Quaternion.Euler(0, (float)building.RotationDegrees, 0);
                var size = new Vector3((float)building.Width, (float)building.Height, (float)building.Depth);

                var walls = _walls!.Next();
                walls.localPosition = ground + Vector3.up * (size.y / 2);
                walls.localRotation = rotation;
                walls.localScale = size;

                var roof = _roofs!.Next();
                roof.localPosition = ground + Vector3.up * (size.y + RoofThickness / 2);
                roof.localRotation = rotation;
                roof.localScale = new Vector3(size.x * 1.1f, RoofThickness, size.z * 1.1f);

                for (var i = 0; i < building.Residents; i++)
                {
                    // Residents stand in a ring just outside their home.
                    var angle = (i / (float)building.Residents) * Mathf.PI * 2 + (float)building.RotationDegrees * Mathf.Deg2Rad;
                    var reach = (float)building.ClearanceRadius + 0.35f;
                    var resident = _residents!.Next();
                    resident.localPosition = ground + new Vector3(Mathf.Cos(angle) * reach, ResidentHeight / 2, Mathf.Sin(angle) * reach);
                    resident.localScale = new Vector3(0.2f, ResidentHeight / 2, 0.2f);
                }
            }

            EndFrame();
        }

        public void Clear()
        {
            if (_walls == null)
            {
                return;
            }

            BeginFrame();
            EndFrame();
        }

        private void BeginFrame()
        {
            _walls!.BeginFrame();
            _roofs!.BeginFrame();
            _residents!.BeginFrame();
        }

        private void EndFrame()
        {
            _walls!.EndFrame();
            _roofs!.EndFrame();
            _residents!.EndFrame();
        }

        private void EnsureInitialised()
        {
            if (_walls != null)
            {
                return;
            }

            _walls = new PrimitivePool(PrimitiveType.Cube, transform, MaterialFactory.CreateOpaque(TerrainPalette.Walls));
            _roofs = new PrimitivePool(PrimitiveType.Cube, transform, MaterialFactory.CreateOpaque(TerrainPalette.Roof));
            _residents = new PrimitivePool(PrimitiveType.Capsule, transform, MaterialFactory.CreateOpaque(TerrainPalette.Resident));
        }
    }
}
