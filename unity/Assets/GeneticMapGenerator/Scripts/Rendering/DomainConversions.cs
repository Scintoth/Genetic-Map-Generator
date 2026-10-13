using GeneticMapGenerator.Domain.Common;
using UnityEngine;

namespace GeneticMapGenerator.Unity.Rendering
{
    public static class DomainConversions
    {
        public static Vector3 ToUnity(this Vector3D vector) => new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
    }
}
