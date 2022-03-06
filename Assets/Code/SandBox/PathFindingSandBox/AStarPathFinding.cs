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

            Node1 startNode1 = grid.NodeFromWorldPoint(startPos);
            Node1 targetNode1 = grid.NodeFromWorldPoint(targetPos);

            if(startNode1.walkable && targetNode1.walkable)
            {
                Heap<Node1> openSet = new Heap<Node1>(grid.GridLength);
                HashSet<Node1> closedSet = new HashSet<Node1>();

                openSet.Add(startNode1);
                while(openSet.Count > 0)
                {
                    Node1 currentNode1 = openSet.RemoveFirst(); //starting node
                    closedSet.Add(currentNode1);

                    if(currentNode1 == targetNode1) 
                    {
                        //========================
                        sw.Stop();
                        print($"Path HEAP found: {sw.Elapsed} ms");
                        //========================
                        RetracePath(startNode1, targetNode1);
                        return;
                    }

                    foreach(Node1 neighbour in grid.GetNeighbours(currentNode1)) // find the nearest valid node and add it to the opensetlist
                    {
                        if(!neighbour.walkable || closedSet.Contains(neighbour)) continue; // check if not walkable or not part of list closedSet
                        int newMovementCostToNeighbour = currentNode1.gCost + GetDistance(currentNode1, neighbour); // calcul length beetween position and adjacent
                        if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode1); 
                            neighbour.parent = currentNode1;

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
        /// <param name="startNode1"></param>
        /// <param name="endNode1"></param>
        private void RetracePath(Node1 startNode1, Node1 endNode1)
        {
            List<Node1> path = new List<Node1>();
            Node1 currentNode1 = endNode1;

            while(currentNode1 != startNode1)
            {
                path.Add(currentNode1);
                currentNode1 = currentNode1.parent;
            }
            path.Reverse();

            grid.PathList = path;
        }

        private int GetDistance(Node1 node1A, Node1 node1B)
        {
            int distX = abs(node1A.gridX - node1B.gridX);
            int distY = abs(node1A.gridY - node1B.gridY);
            
            return (distX > distY) ? 14 * distY + 10 * (distX - distY) : 14 * distX + 10 * (distY - distX);
        }
    }

}
