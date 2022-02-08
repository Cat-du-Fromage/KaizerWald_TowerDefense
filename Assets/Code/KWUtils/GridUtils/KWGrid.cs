using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KWUtils
{
    public static class KWGrid
    {
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
            int y = (int)floor((float)i / w);
            int x = i - (y * w);
            return (x, y);
        }
        
        public static int GetIndexFromPosition(this Vector3 pointPos, int2 gridSize, int cellSize)
        {
            //Vector2 percentXY = new Vector2(pointPos.x,pointPos.z) / (new Vector2(gridSize.x,gridSize.y) * cellSize);
            
            float percentX = (pointPos.x) / (gridSize.x * cellSize);
            float percentY = (pointPos.z) / (gridSize.y * cellSize);
            
            percentX = Clamp01(percentX); //CAREFUL NEED ABS!
            percentY = Clamp01(percentY); //CAREFUL NEED ABS!
 
            int x = Clamp(FloorToInt(gridSize.x * percentX), 0, gridSize.x-1);
            int y = Clamp(FloorToInt(gridSize.y * percentY), 0, gridSize.y-1);
            
            return (y * (gridSize.x)) + x;
        }
    }
}
