#define DEBUG_PERFORMANCE_TEST

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

#if DEBUG_PERFORMANCE_TEST
using System.Diagnostics;
#endif

namespace KWUtils
{
    public static class KWGrid
    {
#if DEBUG_PERFORMANCE_TEST
        private static Stopwatch sw = new Stopwatch();

        private static void printTimeWatch(string call)
        {
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            UnityEngine.Debug.Log($"time {call} {ts}");
            sw.Reset();
        }
#endif
        /// <summary>
        /// Get position (in Int2) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the grid</param>
        /// <returns>Int2 Pos</returns>
        public static int2 GetXY2(this int i, int w)
        {
            int y = (int)floor((float)i/w);
            int x = i - (y * w);
            return int2(x, y);
        }
        
        /// <summary>
        /// Get position (in Int, Int) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the Grid</param>
        /// <returns>Int X, Int Y(return in this order)</returns>
        public static (int,int) GetXY(this int i, int w)
        {
//#if DEBUG_PERFORMANCE_TEST
            //sw.Start();
//#endif
            int y = (int)floor((float)i / w);
            int x = i - (y * w);
//#if DEBUG_PERFORMANCE_TEST
            //printTimeWatch("GetXY");
//#endif
            return (x, y);
        }
        
        public static int GetIndexFromPosition(this Vector3 pointPos, int2 numCellsOnAxis, int cellSize)
        {
            float percentX = pointPos.x / (numCellsOnAxis.x * cellSize);
            float percentY = pointPos.z / (numCellsOnAxis.y * cellSize);

            percentX = Clamp01(percentX); //CAREFUL NEED ABS!
            percentY = Clamp01(percentY); //CAREFUL NEED ABS!
            
            int x = Clamp(FloorToInt(numCellsOnAxis.x * percentX), 0, numCellsOnAxis.x-1);
            int y = Clamp(FloorToInt(numCellsOnAxis.y * percentY), 0, numCellsOnAxis.y-1);

            return (y * numCellsOnAxis.x) + x;
        }
    }
}
