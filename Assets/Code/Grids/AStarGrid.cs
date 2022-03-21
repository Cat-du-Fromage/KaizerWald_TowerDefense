using System;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace TowerDefense
{
    public class AStarGrid: MonoBehaviour, IGridHandler<GridType, Node, GenericGrid<Node>>
    {
        public bool DebugAstar;
        private const int CellSize = 1;

        [SerializeField] private Transform DestinationCell;
        [SerializeField] private Transform StartCell;
        
        private int startIndex = -1;
        private int endIndex = -1;
        
        private Vector3 startPosition;
        private Vector3 destination;
        
        private readonly RaycastHit[] hits = new RaycastHit[1];
        private int[] currentPath;

        private NativeArray<bool> nativeObstacles;
        private NativeArray<Node> nativeNodes;
        private NativeList<int> nativePathList;

        //private bool jobSchedule;
        private JobHandle lastJobScheduled;

        private List<int> indices;
        
        //==============================================================================================================
        /// Grid Interface
        public IGridSystem<GridType> GridSystem { get; set; }
        public GenericGrid<Node> Grid { get; private set; }
        public void InitializeGrid(int2 terrainBounds)
        {
            int numCellX = terrainBounds.x / CellSize;
            Grid = new GenericGrid<Node>(terrainBounds, CellSize, (index) => new Node(index.GetXY2(numCellX)));
        }
        //==============================================================================================================
        
        private void Awake()
        {
            if (DestinationCell == null)
                DestinationCell = FindObjectOfType<EndGateComponent>().transform;
            if (StartCell == null)
                StartCell = FindObjectOfType<StartSpawnComponent>().transform;
        }

        private void Start()
        {
            currentPath = new int[1];
            indices = new List<int>(16);
            startIndex = Grid.IndexFromPosition(StartCell.position);
            endIndex = Grid.IndexFromPosition(DestinationCell.position);
            CheckValidPath(GridSystem.RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles));
        }

        private void OnDestroy()
        {
            DisposeAll();
        }

        public bool IsPathAffected(int chunkIndex, in GridData obstaclesGridData)
        {
            GridData fakeChunk = new GridData(Grid.GridData.MapSize, CellSize, obstaclesGridData.CellSize);
            indices.Clear();
            for (int i = 0; i < fakeChunk.TotalCellInChunk; i++)
            {
                indices.Add(chunkIndex.GetGridCellIndexFromChunkCellIndex(fakeChunk, i));
            }
            
            for (int i = 0; i < currentPath.Length; i++)
            {
                if (!indices.Contains(currentPath[i])) continue;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// EVENT : From BuildManager.cs
        /// </summary>
        public bool OnBuildCursorMove(int chunkIndex, in GridData obstaclesGrid)
        {
            if (!IsPathAffected(chunkIndex, obstaclesGrid)) return true;
            return CheckValidPath(GridSystem.RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles), chunkIndex);
        }
        
        private bool CheckValidPath(GenericGrid<bool> obstaclesGrid, int simulateAtIndex = -1)
        {
            nativeObstacles = new(obstaclesGrid.AdaptGrid(Grid), Allocator.TempJob);
            
            if (simulateAtIndex != -1)
            {
                GridData fakeChunk = new GridData(Grid.GridData.MapSize, CellSize, obstaclesGrid.GridData.CellSize);
                for (int i = 0; i < fakeChunk.TotalCellInChunk; i++)
                {
                    int index = simulateAtIndex.GetGridCellIndexFromChunkCellIndex(fakeChunk, i);
                    nativeObstacles[index] = true;
                }
            }
            
            nativeNodes = new NativeArray<Node>(Grid.GridArray, Allocator.TempJob);
            nativePathList = new NativeList<int>(16, Allocator.TempJob);

            //Get Path from Start -> End
            JaStar job = new JaStar
            {
                NumCellX = Grid.GridData.NumCellXY.x,
                StartNodeIndex = startIndex,
                EndNodeIndex = endIndex,
                Nodes = nativeNodes,
                ObstaclesGrid = nativeObstacles,
                PathList = nativePathList
            };
            lastJobScheduled = job.Schedule();
            JobHandle.ScheduleBatchedJobs();
            return CompleteJob();
        }
        
        private bool CompleteJob()
        {
            lastJobScheduled.Complete();
            
            // Path Is Valid
            if (!nativePathList.IsEmpty) 
            {
                if (currentPath.Length != nativePathList.Length)
                {
                    Array.Resize(ref currentPath, nativePathList.Length);
                }
                nativePathList.ToArray().Reverse().CopyTo((Span<int>)currentPath);
                DisposeAll();
                return true;
            }
            DisposeAll();
            //NOTIFY BUILD MANAGER IT'S NOT VALID
            return false;
        }

        private void DisposeAll()
        {
            if (nativeNodes.IsCreated)     nativeNodes.Dispose();
            if (nativePathList.IsCreated)  nativePathList.Dispose();
            if (nativeObstacles.IsCreated) nativeObstacles.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (currentPath.IsNullOrEmpty()) return;
            
            Gizmos.color = Color.magenta;
            for (int i = 0; i < currentPath.Length; i++)
            {
                Gizmos.DrawWireSphere(Grid.GetCellCenter(currentPath[i]), 0.5f);
            }

            if (!DebugAstar) return;
            Gizmos.color = Color.black;
            for (int i = 0; i < Grid.GridArray.Length; i++)
            {
                Gizmos.DrawWireCube(Grid.GetCellCenter(i), new Vector3(1,0.5f,1));
            }
        }
    }
}