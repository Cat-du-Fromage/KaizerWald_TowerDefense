using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class Node1 : IHeapItem<Node1>
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public Node1 parent;
        public int gCost; //distance from starting position
        public int hCost; //distance from target position

        public Node1(bool walkable, Vector3 worldPosition, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
        }

        public int FCost => gCost + hCost;

        public int HeapIndex { get; set; }
/*
        public int PseudoCompareTo(Node node, Node nodeToCompare)
        {
            int compare;
            if (node.FCost == nodeToCompare.FCost)
            {
                compare = node.hCost > nodeToCompare.hCost ? 1 : -1;
                return -compare;
            }
            compare = node.FCost > nodeToCompare.FCost ? 1 : -1;
            return -compare;
        }
*/
        public int CompareTo(Node1 node1ToCompare)
        {
            int compare = FCost.CompareTo(node1ToCompare.FCost);
            if(compare == 0)
            {
                compare = hCost.CompareTo(node1ToCompare.hCost);
            }
            return -compare;
        }
    }
}
