using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities
{
    public static class ExtensionMethods
    {
        public static List<T> Only<T>(this T obj)
        {
            return new List<T> { obj };
        }

        public static void ShouldEqual(this Vector3 vector, Vector3 otherVector)
        {
            if(vector.x != otherVector.x)
                throw new Exception("X values do not match.");
            if (vector.y != otherVector.y)
                throw new Exception("Y values do not match.");
            if (vector.z != otherVector.z)
                throw new Exception("Z values do not match.");
        }

        public static void ShouldEqual(this float value, float otherValue)
        {
            Assert.AreEqual(value, otherValue);
        }

        public static void ShouldBeNull<T>(this T obj)
        {
            if(obj != null)
                throw new Exception("Object has a value");
        }

        public static void ShouldNotBeNull<T>(this T obj)
        {
            if(obj == null)
                throw new Exception("Object is null");
        }
    }
}
