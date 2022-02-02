using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public static class Vector3Extension
    {
        /// <summary>
        /// flatten the coordinate by setting y to 0
        /// </summary>
        /// <param name="coordToFlat">coord to flatten</param>
        /// <returns></returns>
        public static Vector3 Flat(this Vector3 coordToFlat)
        {
            return new Vector3(coordToFlat.x, 0, coordToFlat.z);
        }
        
        public static Vector3 SetX(this Vector3 target, float xVal)
        {
            return new Vector3(xVal, target.y, target.z);
        }

        /// <summary>
        /// Return normalize direction
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static Vector3 DirectionTo(this Vector3 source, Vector3 destination)
        {
            return Vector3.Normalize(destination - source);
        }
        
        /// <summary>
        /// Return distance from 2 vector
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static float DistanceTo(this Vector3 source, Vector3 destination)
        {
            return Vector3.Magnitude(destination - source);
        }
    }
}
