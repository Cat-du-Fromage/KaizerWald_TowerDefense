using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.Debug;
using KWUtils.KWGenericGrid;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;

namespace TowerDefense
{
    public class FlowFieldGrid : MonoBehaviour, IGridHandler<GridType, Vector3, GenericChunkedGrid<Vector3>>
    {
#if UNITY_EDITOR
        public bool DebugEnable;
#endif
        private const int ChunkSize = 16;
        private const int CellSize = 1;
        
        private int destinationCellIndex;
        private int startCellIndex;
        private Vector3 startPosition;
        
        //CostField
        private NativeArray<bool> nativeObstacles;
        private NativeArray<byte> nativeCostField;
        //IntegrationField
        private NativeArray<int> nativeBestCostField;
        //FlowField
        private NativeArray<float3> nativeBestDirection;
        private NativeArray<float3> nativeOrderedBestDirection;

        private bool jobSchedule;
        private JobHandle lastJobScheduled;
        
        //==============================================================================================================
        /// Grid Interface
        public IGridSystem<GridType> GridSystem { get; set; }
        public GenericChunkedGrid<Vector3> Grid { get; private set; }
        public void InitializeGrid(int2 terrainBounds)
        {
            Grid = new GenericChunkedGrid<Vector3>(terrainBounds, ChunkSize, CellSize);
        }
        //==============================================================================================================

        private void Start()
        {
            InitializeWaypoints();
            GridSystem.SubscribeToGrid(GridType.Obstacles, OnNewObstacles);
            CalculateFlowField(GridSystem.RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles));
        }

        private void OnDestroy()
        {
            lastJobScheduled.Complete();
            DisposeAll();
        }

        private void Update()
        {
            if (!jobSchedule) return;
            if (!lastJobScheduled.IsCompleted) return;
            CompleteJob();
        }

        private void InitializeWaypoints()
        {
            Transform destinationPath = FindObjectOfType<EndGateComponent>().transform;
            int destinationChunkIndex = destinationPath.position.GetIndexFromPosition(Grid.GridData.MapSize, ChunkSize);
            destinationCellIndex = destinationChunkIndex.GetCellIndexFromChunkEnterPoint(ChunkEnterPoint.Top, Grid.GridData);
            
            Vector3 startSpawnPosition = FindObjectOfType<StartSpawnComponent>().transform.position;
            Debug.Log($"startSpawnPosition = {startSpawnPosition}");
            startCellIndex = startSpawnPosition.XZ().GetIndexFromPositionOffset(Grid.GridData.MapSize, ChunkSize, new int2(ChunkSize/2));
        }

        public int GetChunkSpawn()
        {
            ArrayPool<Vector3> test = ArrayPool<Vector3>.Shared;
            return startCellIndex;
        }
        
        private void OnNewObstacles()
        {
            CalculateFlowField(GridSystem.RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles));
        }
        
        private void CompleteJob()
        {
            lastJobScheduled.Complete();
            int totalChunkCell = Grid.GridData.TotalCellInChunk;
            for (int i = 0; i < Grid.ChunkDictionary.Count; i++)
            {
                Grid.SetValues(i, nativeOrderedBestDirection
                    .Slice(i * totalChunkCell, totalChunkCell)
                    .SliceConvert<Vector3>());
            }
            DisposeAll();
            jobSchedule = false;
        }

        private void CalculateFlowField(GenericGrid<bool> obstacles)
        {
            //nativeObstacles = new NativeArray<bool>(obstacles.AdaptGrid(Grid), Allocator.TempJob);
            nativeObstacles = obstacles.NativeAdaptGrid(Grid);//.ToNativeArray();
            
            int totalCells = Grid.GridData.TotalCells;
            
            //Cost Field
            nativeCostField = AllocNtvAry<byte>(totalCells);
            JobHandle jHCostField = GetCostField();

            //Integration Field
            nativeBestCostField = AllocFillNtvAry<int>(totalCells, ushort.MaxValue);
            JobHandle jHIntegrationField = GetIntegrationField(destinationCellIndex, jHCostField);
        
            //Direction Field
            nativeBestDirection = AllocNtvAry<float3>(totalCells);
            JobHandle jHDirectionField = GetDirectionField(jHIntegrationField);
            
            //Chunk Slice
            nativeOrderedBestDirection = AllocNtvAry<float3>(totalCells);
            lastJobScheduled = nativeOrderedBestDirection.OrderNativeArrayByChunk(nativeBestDirection, Grid.GridData, jHDirectionField);
            
            JobHandle.ScheduleBatchedJobs();
            jobSchedule = true;
        }
        
        
        private JobHandle GetCostField(in JobHandle dependency = default)
        {
            JCostField job = new (nativeObstacles, nativeCostField);
            return job.ScheduleParallel(nativeCostField.Length, JobWorkerCount - 1, dependency);
        }

        private JobHandle GetIntegrationField(int targetCellIndex, in JobHandle dependency = default)
        {
            JIntegrationField job = new(targetCellIndex, Grid.GridData.NumCellXY.x, nativeCostField, nativeBestCostField);
            return job.Schedule(dependency);
        }

        private JobHandle GetDirectionField(in JobHandle dependency = default)
        {
            JBestDirection job = new(Grid.GridData.NumCellXY.x, nativeBestCostField, nativeBestDirection);
            return job.ScheduleParallel(nativeBestCostField.Length, JobWorkerCount - 1, dependency);
        }

        private void DisposeAll()
        {
            if (nativeObstacles.IsCreated)            nativeObstacles.Dispose();
            if (nativeCostField.IsCreated)            nativeCostField.Dispose();
            if (nativeBestCostField.IsCreated)        nativeBestCostField.Dispose();
            if (nativeBestDirection.IsCreated)        nativeBestDirection.Dispose();
            if (nativeOrderedBestDirection.IsCreated) nativeOrderedBestDirection.Dispose();
        }
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!DebugEnable || Grid.GridArray.IsNullOrEmpty() || Grid.ChunkDictionary is null) return;
            /*
            foreach ((int id, Vector3[] values)in Grid.ChunkDictionary)
            {
                //Gizmos.color = Color.red;
                //Gizmos.DrawWireCube(Grid.GetChunkCenter(id), new Vector3(ChunkSize,0.5f,ChunkSize));
                
                for (int i = 0; i < values.Length; i++)
                {
                    Gizmos.color = Color.green;
                    Vector3 cellPos = Grid.GetChunkCellCenter(id,i);
                    //Gizmos.DrawWireCube(cellPos, (Vector3.one * CellSize).Flat());
                    DrawArrow.ForGizmo(cellPos, values[i]);
                }
            }
            */
            
            for (int i = 0; i < Grid.GridArray.Length/8; i++)
            {
                Gizmos.color = Color.green;
                Vector3 cellPos = Grid.GetCellCenter(i);
                Gizmos.DrawWireCube(cellPos, (Vector3.one * CellSize).Flat());
                DrawArrow.ForGizmo(cellPos, Grid.GridArray[i]*0.5f);
            }
            
        }
        #endif
    }
}
