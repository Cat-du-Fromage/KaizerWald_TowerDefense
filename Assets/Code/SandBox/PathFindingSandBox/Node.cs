using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class Node : IHeapItem<Node>
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public Node parent;
        public int gCost; //distance from starting position
        public int hCost; //distance from target position

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
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
        public int CompareTo(Node nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);
            if(compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }
    }
}
