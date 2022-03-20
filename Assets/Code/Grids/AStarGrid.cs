using System;
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

        private bool jobSchedule;
        private JobHandle lastJobScheduled;
        
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
            startIndex = Grid.IndexFromPosition(StartCell.position);
            endIndex = Grid.IndexFromPosition(DestinationCell.position);
            AStarProcess(GridSystem.RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles));
        }

        private void OnDestroy()
        {
            DisposeAll();
        }

        private void Update()
        {
            if (!jobSchedule) return;
            if (!lastJobScheduled.IsCompleted) return;
            jobSchedule = CompleteJob();
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

                //currentPath = nativePathList.ToArray().Reverse();
                
                DisposeAll();
                return false;
            }
            DisposeAll();
            //NOTIFY BUILD MANAGER IT'S NOT VALID
            return false;
        }

        public bool IsPathAffected(int chunkIndex, in GridData obstaclesGridData)
        {
            GridData fakeChunk = new GridData(Grid.GridData.MapSize, CellSize, obstaclesGridData.CellSize);
            int[] indices = new int[fakeChunk.TotalCellInChunk];
            //USE LIST
            for (int i = 0; i < fakeChunk.TotalCellInChunk; i++)
            {
                indices[i] = chunkIndex.GetGridCellIndexFromChunkCellIndex(obstaclesGridData, i);
            }

            //multithreadThis?
            for (int i = 0; i < currentPath.Length; i++)
            {
                for (int j = 0; j < indices.Length; j++)
                {
                    if (currentPath[i].Equals(indices[j]))
                    {
                        Debug.Log("TRUE");
                        return true;
                    }
                }
            }
            Debug.Log("false");
            return false;
        }
        
        /// <summary>
        /// EVENT : From BuildManager.cs
        /// </summary>
        public void OnBuildCursorMove(int chunkIndex, in GridData obstaclesGrid)
        {
            if (!IsPathAffected(chunkIndex, obstaclesGrid))
            {
                Debug.Log("FALSE");
                return;
            }
            Debug.Log("TRUE");
            AStarProcess(GridSystem.RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles));
        }
        
/*
        public void OnObstacleAdded(int index, in GridData gridData)
        {
            //We didn't calculate anyPath
            if (currentPath.IsNullOrEmpty()) return;
            
            for (int pathIndex = 0; pathIndex < currentPath.Length; pathIndex++)
            {
                if (currentPath[pathIndex] == index)
                {
                    startIndex = currentPath[pathIndex-1];
                    int[] segment = AStarProcess(gridData);
                
                    if (segment.IsNullOrEmpty()) return; // CAREFUL IT MEANS THERE IS NOS PATH!

                    currentPath.Resize(pathIndex + segment.Length);
                    segment.CopyTo(currentPath, pathIndex);
                    startIndex = Grid.IndexFromPosition(startPosition);
                    break;
                }
            }
        }
*/
        private void AStarProcess(GenericGrid<bool> obstaclesGrid)
        {
            //Safety if player move cursor too fast
            if (!lastJobScheduled.IsCompleted) jobSchedule = CompleteJob();
            
            nativeObstacles = new(obstaclesGrid.AdaptGrid(Grid), Allocator.TempJob);
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

            jobSchedule = true;
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