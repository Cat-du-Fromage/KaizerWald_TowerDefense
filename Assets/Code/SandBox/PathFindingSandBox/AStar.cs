using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;

namespace TowerDefense
{
#if !(UNITY_EDITOR)
    
#endif
    public class AStar : MonoBehaviour
    {
        public Transform player;
        public bool onlyDisplayPathGizmos;
        [SerializeField] private Terrain terrain;
        private int2 mapBounds;

        private const int CellSize = 1;
        private const float HalfCell = 0.5f;
        
        private SimpleGrid<Node> nodeGrid;
        
        public List<Node> PathList;

        private void Awake()
        {
            mapBounds = (int2)terrain.terrainData.size.XZ();
            nodeGrid = new SimpleGrid<Node>(mapBounds, 1);
        }

        private void Start()
        {
            InitializeNodeGrid();
        }

        public int GridLength => nodeGrid.GridLength;
        public int2 GridBounds => mapBounds;

        private void InitializeNodeGrid()
        {
            Vector3 worldPoint = Vector3.zero;
            for (int i = 0; i < nodeGrid.GridLength; i++)
            {
                (int x, int y) = i.GetXY(mapBounds.x);
                worldPoint = Vector3.right * (x + HalfCell) + Vector3.forward * (y + HalfCell);
                nodeGrid.SetValue(i, new Node(true, worldPoint, x, y));
            }
        }
        
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if(checkX >= 0 && checkX < mapBounds.x && checkY >= 0 && checkY < mapBounds.y)
                    {
                        neighbours.Add(nodeGrid.GetValueAt(checkX,checkY));
                    }
                }
            }
            return neighbours;
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition) => nodeGrid.GetValueFromWorldPosition(worldPosition);

        private void OnDrawGizmos()
        {
            if (nodeGrid == null) return;
            /*
            
            int iteration = nodeGrid.GridLength / 32;
            Vector3 size = new Vector3(1f,0.05f,1f);
            for (int i = 0; i < iteration; i++)
            {
                Gizmos.DrawWireCube(nodeGrid.GetCenterCellAt(i), size);
            }
*/
            DebugPathfinding();
        }

        private void DebugPathfinding()
        {
            //Gizmos.DrawWireCube(transform.position, new Vector3(mapBounds.x, 1, mapBounds.y)); //y on z is normal
            Vector3 size = new Vector3(1f,0.05f,1f);
            if(onlyDisplayPathGizmos == true)
            {
                if(PathList != null)
                {
                    Gizmos.color = Color.black;
                    foreach(Node n in PathList)
                    {
                        
                        Gizmos.DrawWireSphere(n.worldPosition, 0.2f); //Vector3.one = Vector3(1,1,1)
                    }
                }
            }
            else
            {
                if(nodeGrid != null)
                {
                    Node playerNode = NodeFromWorldPoint(player.position);
                    
                    for (int i = 0; i < nodeGrid.GridLength; i++)
                    {
                        Node n = nodeGrid.GetValueAt(i);
                        Gizmos.color = (n.walkable) ? Color.white : Color.red;
                        if(playerNode == n)
                        {
                            Gizmos.color = Color.cyan;
                        }

                        if (PathList != null)
                        {
                            if(PathList.Contains(n))
                            {
                                Gizmos.color = Color.black;
                                Gizmos.DrawSphere(n.worldPosition, 0.2f);
                            }
                        }
                        Gizmos.DrawWireCube(n.worldPosition, size); //Vector3.one = Vector3(1,1,1)
                    }
                }
            }
        }
    }
}
