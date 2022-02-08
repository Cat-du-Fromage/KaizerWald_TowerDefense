using System;
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

        /// <summary>
        /// Set wanted axis
        /// </summary>
        /// <param name="target"></param>
        /// <param name="axis">component to change</param>
        /// <param name="val">value to assign</param>
        /// <returns>changed axis</returns>
        public static void SetAxis(ref this Vector3 target, Axis axis, float val)
        {
            switch (axis)
            {
                case Axis.X:
                    target.Set(val, target.y, target.z);
                    break;
                case Axis.Y:
                    target.Set(target.x, val, target.z);
                    break;
                case Axis.Z:
                    target.Set(target.x, target.y, val);
                    break;
            }
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