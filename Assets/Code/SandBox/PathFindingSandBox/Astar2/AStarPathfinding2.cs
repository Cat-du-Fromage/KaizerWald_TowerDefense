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


namespace TowerDefense
{
    public partial class AStarPathfinding2 : MonoBehaviour, IGridHandler<Node>
    {
        [SerializeField] private Transform DestinationGate;
        [SerializeField] private Transform agentStart;
        //Interfaces
        public IGridSystem GridSystem { get; set; }
        public SimpleGrid<Node> Grid { get; private set; }

        [SerializeField] private Terrain terrain;
        private int2 mapBounds;
        
        private const int DiagonalCostMove = 14;
        private const int StraightCostMove = 10;

        private const int CellSize = 4;

        private Vector3 startPosition;
        private Vector3 destination;
        
        private int startIndex = -1;
        private int endIndex = -1;
        
        private RaycastHit[] hits = new RaycastHit[1];
        
        private int[] path;

        private void Awake()
        {
            mapBounds = (int2)terrain.terrainData.size.XZ();
            DestinationGate ??= FindObjectOfType<EndGateComponent>().transform;
            Grid = new SimpleGrid<Node>(mapBounds, CellSize, (coord) => new Node(coord));
            destination = DestinationGate.position;
            endIndex = Grid.IndexFromPosition(destination);
        }

        private void Start()
        {
            //startPosition = agentStart.position;
            //startIndex = Grid.GetIndexFromPosition(startPosition);
            
            
        }
        
        private void Update() 
        {
            /*
            if (startPosition != agentStart.position)
            {
                startPosition = agentStart.position;
                startIndex = Grid.GetIndexFromPosition(startPosition);
            }
                */
            
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Ray ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
                if (Physics.RaycastNonAlloc(ray.origin, ray.direction, hits, math.INFINITY, TerrainLayerMask) != 0)
                {
                    destination = hits[0].point;
                    endIndex = Grid.IndexFromPosition(destination);
                }
            }
            
            if(Keyboard.current.pKey.wasPressedThisFrame && agentStart != null)
            {
#if UNITY_EDITOR
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                startPosition = agentStart.position;
                startIndex = Grid.IndexFromPosition(startPosition);
                path = AStarProcess();
                
#if UNITY_EDITOR
                sw.Stop();
                print($"Path found: {sw.Elapsed} ms");          
#endif
                
            }
        }

        public void OnObstacleAdded(int index)
        {
            //Get the Node affected
            if (!path.IsNullOrEmpty())
            {
                for (int pathIndex = 0; pathIndex < path.Length; pathIndex++)
                {
                    if (path[pathIndex] != index) continue;
                    
                    startIndex = path[pathIndex-1];
                    int[] segment = AStarProcess();
                    
                    if (segment.IsNullOrEmpty()) return; // CAREFUL IT MEANS THERE IS NOS PATH!
                    
                    Array.Resize(ref path, pathIndex + segment.Length);
                    segment.CopyTo(path, pathIndex);
                    startIndex = Grid.IndexFromPosition(startPosition);
                    break;
                }
            }
            
            //Check if current Path contain the Node Index
        }

        public (Vector3[], int[]) RequestPath(in Vector3 currentPosition)
        {
            startIndex = Grid.IndexFromPosition(currentPosition);
            path = AStarProcess();
            Vector3[] nodesPosition = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                nodesPosition[i] = Grid.GetCenterCellAt(path[i]);
            }
            return (nodesPosition, path);
        }

        public int[] AStarProcess()
        {
            // TODO : Get Cost Field 1 if free 255 if Obstacle => get from BuildManager?
            
            using NativeArray<bool> obstacles = GridSystem.RequestGrid<bool>(GridType.Turret).GetGridArray.ToNativeArray();
            using NativeArray<Node> nodes = Grid.GetGridArray.ToNativeArray();

            using NativeList<int> pathList = new NativeList<int>(16, Allocator.TempJob);

            //Get Path from Start -> End
            JAStar job = new JAStar
            {
                MapSizeX = Grid.GetGridWidth,
                StartNodeIndex = startIndex,
                EndNodeIndex = endIndex,
                Nodes = nodes,
                ObstaclesGrid = obstacles,
                PathList = pathList
            };
            JobHandle jh = job.Schedule();
            JobHandle.ScheduleBatchedJobs();
            
            // TODO : Complete the Job On the Next Frame (use a bool variable to say "jobscheduled" to check in the update
            //Otherwise we loose the benefits of the job being work in parallel
            jh.Complete(); 
            
            int[] pathToFollow = pathList.ToArray().Reverse();
            return pathToFollow;
        }

        

        [BurstCompile(CompileSynchronously = true)]
        private struct JAStar : IJob
        {
            [ReadOnly] public int MapSizeX;
            [ReadOnly] public int StartNodeIndex;
            [ReadOnly] public int EndNodeIndex;
            
            [ReadOnly] public NativeArray<bool> ObstaclesGrid;
            
            public NativeArray<Node> Nodes;
            [WriteOnly] public NativeList<int> PathList; // if PathNode.Length == 0 means No Path!
            
            public void Execute()
            {
                NativeHashSet<int> openSet = new NativeHashSet<int>(16, Allocator.Temp);
                NativeHashSet<int> closeSet = new NativeHashSet<int>(16, Allocator.Temp);
                
                Nodes[StartNodeIndex] = StartNode(Nodes[StartNodeIndex], Nodes[EndNodeIndex]);
                openSet.Add(StartNodeIndex);

                NativeList<int> neighbors = new NativeList<int>(8,Allocator.Temp);
                int currentNode = 0;
                
                while (!openSet.IsEmpty)
                {
                    currentNode = GetLowestFCostNodeIndex(openSet);
                    //Check if we already arrived
                    if (currentNode == EndNodeIndex)
                    {
                        CalculatePath();
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
                            //if (closeSet.Contains(neighbors[i])) continue;
                            openSet.Add(neighbors[i]);
                        }
                    }
                    neighbors.Clear();
                }
                
            }

            private void CalculatePath()
            {
                int currentNode = EndNodeIndex;

                while(currentNode != StartNodeIndex)
                {
                    PathList.Add(currentNode);
                    currentNode = Nodes[currentNode].CameFromNodeIndex;
                }

                PathList.Add(StartNodeIndex);
            }
            
            private void GetNeighborCells(int index, ref NativeList<int> curNeighbors, in NativeHashSet<int> closeSet)
            {
                int2 coord = index.GetXY2(MapSizeX);
                for (int i = 0; i < 8; i++)
                {
                    int neighborId = index.AdjCellFromIndex((1 << i), coord, MapSizeX);
                    if (neighborId == -1 || ObstaclesGrid[neighborId] == true || closeSet.Contains(neighborId)) continue;

                    int tentativeCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],Nodes[neighborId]);
                    if (tentativeCost < Nodes[neighborId].GCost)
                    {
                        curNeighbors.Add(neighborId);
                    
                        int gCost = CalculateDistanceCost(Nodes[neighborId], Nodes[StartNodeIndex]);
                        int hCost = CalculateDistanceCost(Nodes[neighborId], Nodes[EndNodeIndex]);
                        Nodes[neighborId] = new Node
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
                using NativeArray<int> tempOpenSet = openSet.ToNativeArray(Allocator.Temp);
                int lowest = tempOpenSet[0];
                for (int i = 1; i < tempOpenSet.Length; i++)
                {
                    int index = tempOpenSet[i];
                    lowest = select(lowest, index, Nodes[index].FCost < Nodes[lowest].FCost);
                }
                return lowest;
            }

            private Node StartNode(in Node start, in Node end)
            {
                int hCost = CalculateDistanceCost(start, end);
                return new Node(-1, 0, hCost, 0+hCost,start.Coord);
            }

            private int CalculateDistanceCost(in Node a, in Node b)
            {
                int2 xyDistance = abs(a.Coord - b.Coord);
                int remaining = abs(xyDistance.x - xyDistance.y);
                return DiagonalCostMove * cmin(xyDistance) + StraightCostMove * remaining;
            }
        }

        
    }
}