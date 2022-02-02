using UnityEngine;

namespace KWUtils
{
    public static class QuaternionExt
    {
        public static Quaternion SetX(this Quaternion target, float xVal)
        {
            return new Quaternion(xVal, target.y, target.z, target.w);
        }
    }
}