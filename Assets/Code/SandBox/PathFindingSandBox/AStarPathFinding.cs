using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static KWUtils.KWmath;
using static TowerDefense.TowerDefenseUtils;
using static KWUtils.InputSystemExtension;
using static Unity.Mathematics.math;

namespace TowerDefense
{
    public class AStarPathFinding : MonoBehaviour
    {
        public Transform seeker;//, target;

        public Vector3 target = Vector3.zero;

        AStar grid;

        private RaycastHit[] hits = new RaycastHit[1];

        private void Awake() 
        {
            grid = GetComponent<AStar>();
        }

        private void Update() 
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
                if (Physics.RaycastNonAlloc(ray.origin, ray.direction, hits, math.INFINITY, TerrainLayerMask) != 0)
                {
                    target = hits[0].point;
                }
            }
            
            if(Keyboard.current.pKey.wasPressedThisFrame && seeker != null)
            {
                FindPath(seeker.position, target);
            }
        }
    
        private void FindPath(Vector3 startPos, Vector3 targetPos)
        {
            //========================
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //========================

            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);

            if(startNode.walkable && targetNode.walkable)
            {
                Heap<Node> openSet = new Heap<Node>(grid.GridLength);
                HashSet<Node> closedSet = new HashSet<Node>();

                openSet.Add(startNode);
                while(openSet.Count > 0)
                {
                    Node currentNode = openSet.RemoveFirst(); //starting node
                    closedSet.Add(currentNode);

                    if(currentNode == targetNode) 
                    {
                        //========================
                        sw.Stop();
                        print($"Path found: {sw.ElapsedMilliseconds} ms");
                        //========================
                        RetracePath(startNode, targetNode);
                        return;
                    }

                    foreach(Node neighbour in grid.GetNeighbours(currentNode)) // find the nearest valid node and add it to the opensetlist
                    {
                        if(!neighbour.walkable || closedSet.Contains(neighbour)) continue; // check if not walkable or not part of list closedSet
                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour); // calcul length beetween position and adjacent
                        if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode); 
                            neighbour.parent = currentNode;

                            if(!openSet.Contains(neighbour)) 
                                openSet.Add(neighbour);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all the node by retrieving each parent node previously store
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        private void RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while(currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();

            grid.PathList = path;
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distX = abs(nodeA.gridX - nodeB.gridX);
            int distY = abs(nodeA.gridY - nodeB.gridY);
            
            return (distX > distY) ? 14 * distY + 10 * (distX - distY) : 14 * distX + 10 * (distY - distX);
        }
    }

}
