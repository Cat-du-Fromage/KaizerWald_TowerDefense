using System;
using System.Diagnostics;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using static KWUtils.Vector3Extension;
using static TowerDefense.TowerDefenseUtils;
using static Unity.Mathematics.math;
using static KWUtils.InputSystemExtension;
using Debug = UnityEngine.Debug;

namespace TowerDefense
{
    public class AStarPathfinding2 : MonoBehaviour, IGridHandler<Node2>
    {
        public bool DebugAStar;
        //Interfaces
        public IGridSystem GridSystem { get; set; }
        public SimpleGrid<Node2> Grid { get; private set; }

        [SerializeField] private Terrain terrain;
        private int2 mapBounds;
        
        private const int DiagonalCostMove = 14;
        private const int StraightCostMove = 10;

        private const int CellSize = 4;
        private const float HalfCell = 2f;

        [SerializeField] private Transform agentStart;
        private Vector3 startPosition;
        private Vector3 destination;
        private RaycastHit[] hits = new RaycastHit[1];

        private int startIndex = -1;
        private int endIndex = -1;

        private int[] path;
        
        private void Awake()
        {
            mapBounds = (int2)terrain.terrainData.size.XZ();
            
            //mapBounds = new int2((int) terrain.terrainData.size.x, (int) terrain.terrainData.size.z);
        }

        private void Start()
        {
            Grid = new SimpleGrid<Node2>(mapBounds, CellSize, (coord) => new Node2(coord));
        }
        
        private void Update() 
        {
            if (startPosition != agentStart.position)
            {
                startPosition = agentStart.position;
                startIndex = Grid.GetIndexFromPosition(startPosition);
            }
                
            
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Ray ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
                if (Physics.RaycastNonAlloc(ray.origin, ray.direction, hits, math.INFINITY, TerrainLayerMask) != 0)
                {
                    destination = hits[0].point;
                    endIndex = Grid.GetIndexFromPosition(destination);
                }
            }
            
            if(Keyboard.current.pKey.wasPressedThisFrame && agentStart != null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                path = AStarProcess();
                sw.Stop();
                print($"Path found: {sw.ElapsedMilliseconds} ms");
            }
        }

        public void OnObstacleAdded(int index)
        {
            //Get the Node affected
            if (!path.IsNullOrEmpty())
            {
                for (int i = 0; i < path.Length; i++)
                {
                    if (path[i] != index) continue;
                    
                    int lengthUntilChange = i;
                    startIndex = path[i-1];
                    int[] segment = AStarProcess();
                    
                    int newLength = lengthUntilChange + segment.Length;
                    Array.Resize(ref path, newLength);
                    segment.CopyTo(path, i);
                    startIndex = Grid.GetIndexFromPosition(startPosition);
                    break;
                }
            }
            
            //Check if current Path contain the Node Index
        }

        public int[] AStarProcess()
        {
            //Get Start And End in the Grid
            
            

            //Get Cost Field 1 if free 255 if Obstacle => get from BuildManager?
            using NativeArray<bool> obstacles = GridSystem.RequestGrid<bool>(GridType.Turret).GetGridArray.ToNativeArray();
            using NativeArray<Node2> nodes = Grid.GetGridArray.ToNativeArray();

            using NativeList<int> pathList = new NativeList<int>(16, Allocator.TempJob);

            //Get Path from Start -> End
            JAStar2 job = new JAStar2
            {
                MapSizeX = Grid.GetGridWidth,
                StartNodeIndex = startIndex,
                EndNodeIndex = endIndex,
                Nodes = nodes,
                ObstaclesGrid = obstacles,
                PathList = pathList
            };
            JobHandle jh = job.Schedule(default);
            jh.Complete();
            
            int[] pathToFollow = pathList.ToArray();
            Array.Reverse(pathToFollow);
            return pathToFollow;
        }

        private void OnDrawGizmos()
        {
            if (!DebugAStar) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            
            if (Grid != null)
            {
                Vector3 pos;
                Vector3 cellBounds = new Vector3(CellSize, 0.05f, CellSize);
                for (int i = 0; i < Grid.GetGridArray.Length / 4; i++)
                {
                    pos = Grid.GetCenterCellAt(i);
                    Gizmos.DrawWireCube(pos, cellBounds);
                    int index = Grid.GetGridArray[i].Coord.y * Grid.GetGridWidth + Grid.GetGridArray[i].Coord.x;
                    Handles.Label(pos, index.ToString(), style);
                }
            }
            
            if (path == null) return;
            if (path.Length != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Grid.GetCenterCellAt(startIndex), 1f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(Grid.GetCenterCellAt(endIndex), 1f);
                Gizmos.color = Color.black;
                for (int i = 0; i < path.Length; i++)
                {
                    Handles.Label(Grid.GetCenterCellAt(path[i]), path[i].ToString(), style);
                    Gizmos.DrawWireSphere(Grid.GetCenterCellAt(path[i]), 1f);
                }
            }
        }

        [BurstCompile]
        private struct JAStar2 : IJob
        {
            [ReadOnly] public int MapSizeX;
            
            [ReadOnly] public int StartNodeIndex; //we write to it BEFORE!
            [ReadOnly] public int EndNodeIndex;
            
            public NativeArray<Node2> Nodes;
            public NativeArray<bool> ObstaclesGrid;

            public NativeList<int> PathList; // if PathNode.Length == 0 means No Path!

            //private NativeHashSet<int> openSet;

            public void Execute()
            {
                NativeHashSet<int> openSet = new NativeHashSet<int>(16, Allocator.Temp);
                NativeHashSet<int> closeSet = new NativeHashSet<int>(16, Allocator.Temp);
                
                Nodes[StartNodeIndex] = StartNode(Nodes[StartNodeIndex], Nodes[EndNodeIndex]);
                openSet.Add(StartNodeIndex);

                NativeList<int> neighbors = new NativeList<int>(4,Allocator.Temp);
                int currentNode = 0;
                
                while (!openSet.IsEmpty)
                {
                    currentNode = GetLowestFCostNodeIndex(openSet);
                    //Check if we already arrived
                    if (currentNode == EndNodeIndex)
                    {
                        CalculatePath();
                        openSet.Clear();
                        return;
                    }

                    //Add "already check" Node AND remove from "To check"
                    openSet.Remove(currentNode);
                    closeSet.Add(currentNode);

                    //Add Neighbors to OpenSet
                    GetNeighborCells(currentNode, ref neighbors, closeSet);
                    if (neighbors.Length > 0)
                    {
                        for (int i = 0; i < neighbors.Length; i++)
                        {
                            if (closeSet.Contains(neighbors[i])) continue;
                            openSet.Add(neighbors[i]);
                        }
                    }
                    neighbors.Clear();
                }
                
            }

            private void CalculatePath()
            {
                PathList.Add(EndNodeIndex);
                int currentNode = EndNodeIndex;
                while(currentNode != StartNodeIndex)
                {
                    PathList.Add(currentNode);
                    currentNode = Nodes[currentNode].CameFromNodeIndex;
                    if (currentNode == StartNodeIndex)
                        break;
                }
                
            }
            
            private void GetNeighborCells(int index, ref NativeList<int> curNeighbors, in NativeHashSet<int> closeSet)
            {
                int2 coord = index.GetXY2(MapSizeX);
                int gCost;
                int hCost;
                for (int i = 0; i < 4; i++)
                {
                    int neighborId = index.AdjCellFromIndex((1 << i), coord, MapSizeX);
                    if (neighborId == -1 || ObstaclesGrid[neighborId] == true || closeSet.Contains(neighborId)) continue;

                    int tentativeCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],Nodes[neighborId]);
                    if (tentativeCost < Nodes[neighborId].GCost)
                    {
                        curNeighbors.Add(neighborId);
                    
                        gCost = CalculateDistanceCost(Nodes[neighborId], Nodes[StartNodeIndex]);
                        hCost = CalculateDistanceCost(Nodes[neighborId], Nodes[EndNodeIndex]);
                        Nodes[neighborId] = new Node2
                        (
                            index,
                            gCost,
                            hCost,
                            gCost + hCost,
                            Nodes[neighborId].Coord
                        );
                    }
                }
            }

            private int GetLowestFCostNodeIndex(in NativeHashSet<int> openSet)
            {
                int lowest = -1;
                using NativeArray<int> tempOpenSet = openSet.ToNativeArray(Allocator.Temp);
                for (int i = 0; i < tempOpenSet.Length; i++)
                {
                    int index = tempOpenSet[i];
                    if (lowest != -1)
                    {
                        if (Nodes[index].FCost < Nodes[lowest].FCost)
                        {
                            lowest = index;
                        }
                    }
                    else
                    {
                        lowest = index;
                    }
                }
                return lowest;
            }

            private Node2 StartNode(in Node2 start, in Node2 end)
            {
                int hCost = CalculateDistanceCost(start, end);
                return new Node2(-1, 0, hCost, 0+hCost,start.Coord);
            }

            private int CalculateDistanceCost(in Node2 a, in Node2 b)
            {
                int2 xyDistance = abs(a.Coord - b.Coord);
                int remaining = abs(xyDistance.x - xyDistance.y);
                return DiagonalCostMove * cmin(xyDistance) + StraightCostMove * remaining;
            }
        }

        
    }
}