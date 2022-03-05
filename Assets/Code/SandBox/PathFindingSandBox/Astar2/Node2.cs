using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{
    public struct Node2
    {
        public int CameFromNodeIndex;
        
        public int GCost; //Distance from Start Node
        public int HCost; // distance from End Node
        public int FCost;
        public int2 Coord;

        public Node2(int cameFromNodeIndex, int gCost, int hCost, int2 coord)
        {
            CameFromNodeIndex = cameFromNodeIndex;
            GCost = gCost;
            HCost = hCost;
            FCost = GCost + HCost;
            Coord = coord;
        }
        
        public Node2(int cameFromNodeIndex, int gCost, int hCost, int fCost, int2 coord)
        {
            CameFromNodeIndex = cameFromNodeIndex;
            GCost = gCost;
            HCost = hCost;
            FCost = fCost;
            Coord = coord;
        }
        
        public Node2(int2 coord)
        {
            CameFromNodeIndex = -1;
            GCost = int.MaxValue;
            HCost = default;
            FCost = default;
            Coord = coord;
        }
        
        public void UpdateFCost() => FCost = GCost + HCost;
    }
}
