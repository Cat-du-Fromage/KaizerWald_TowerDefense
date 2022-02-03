using System;
using UnityEngine;

namespace KWUtils
{
    [Flags]
    public enum Axis
    {
        X,
        Y,
        Z,
        W
    }
    
    public static class QuaternionExt
    {
        public static Quaternion SetAxis(this Quaternion target, Axis axis ,float val) 
        => axis switch
        {
            Axis.X => new Quaternion(val, target.y, target.z, target.w),
            Axis.Y => new Quaternion(target.x, val, target.z, target.w),
            Axis.Z => new Quaternion(target.x, target.y, val, target.w),
            Axis.W => new Quaternion(target.x, target.y, target.z, val),
            _ => target
        };
        
    }
}