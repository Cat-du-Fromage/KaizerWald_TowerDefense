using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{
    public struct Node2
    {
        public int CameFromNodeIndex;
        
        public int GCost;
        public int HCost;
        public int2 Coord;

        public Node2(int cameFromNodeIndex, int gCost, int hCost, int2 coord)
        {
            CameFromNodeIndex = cameFromNodeIndex;
            GCost = gCost;
            HCost = hCost;
            Coord = coord;
        }
        
        public Node2(int2 coord)
        {
            CameFromNodeIndex = default;
            GCost = default;
            HCost = default;
            Coord = coord;
        }
        
        public readonly int FCost => GCost + HCost;
    }
}
