using System.Collections.Generic;
using UnityEngine;

namespace GeneticMapGenerator.Unity.Rendering
{
    /// <summary>
    /// Reuses primitive GameObjects across redraws so per-generation previews don't churn the scene.
    /// Colliders are stripped: the views are purely visual.
    /// </summary>
    public sealed class PrimitivePool
    {
        private readonly PrimitiveType _type;
        private readonly Transform _parent;
        private readonly Material _material;
        private readonly List<GameObject> _instances = new List<GameObject>();
        private int _inUse;

        public PrimitivePool(PrimitiveType type, Transform parent, Material material)
        {
            _type = type;
            _parent = parent;
            _material = material;
        }

        /// <summary>Starts a redraw: every instance becomes available again.</summary>
        public void BeginFrame() => _inUse = 0;

        public Transform Next()
        {
            if (_inUse == _instances.Count)
            {
                var instance = GameObject.CreatePrimitive(_type);
                Object.Destroy(instance.GetComponent<Collider>());
                instance.GetComponent<MeshRenderer>().sharedMaterial = _material;
                instance.transform.SetParent(_parent, worldPositionStays: false);
                _instances.Add(instance);
            }

            var next = _instances[_inUse++];
            next.SetActive(true);
            return next.transform;
        }

        /// <summary>Ends a redraw: hides whatever was not used this time.</summary>
        public void EndFrame()
        {
            for (var i = _inUse; i < _instances.Count; i++)
            {
                _instances[i].SetActive(false);
            }
        }
    }
}
