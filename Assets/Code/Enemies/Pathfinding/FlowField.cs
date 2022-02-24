using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;

namespace TowerDefense
{
    public class FlowField
    {
        private readonly int2 gridSize;
        private readonly int chunkSize;
        private readonly int totalNumCells;

        //CostField
        private NativeArray<int> nativeWalkableChunk;
        private NativeArray<byte> nativeCostField;

        //IntegrationField
        private NativeArray<int> nativeBestCostField;
        public int[] BestCostField;
        
        //FlowField
        private NativeArray<float3> nativeBestDirection;

        public FlowField(int2 gridSize, int chunkSize)
        {
            this.gridSize = gridSize;
            this.chunkSize = chunkSize;
            totalNumCells = gridSize.x * gridSize.y;
        }
        
        public Vector3[] GetFlowField(int targetCell, int[] walkableChunk)
        {
            Vector3[] directionField = new Vector3[totalNumCells];

            //Cost Field
            nativeWalkableChunk = walkableChunk.ToNativeArray();
            nativeCostField = AllocNtvAry<byte>(totalNumCells);
            
            JobHandle jHCostField = GetCostField(walkableChunk);
            jHCostField.Complete();

            //Integration Field
            nativeBestCostField = AllocFillNtvAry<int>(totalNumCells, ushort.MaxValue);
            
            
            JobHandle jHIntegrationField = GetIntegrationField(targetCell, jHCostField);
            jHIntegrationField.Complete();
            BestCostField = new int[totalNumCells];
            nativeBestCostField.CopyTo(BestCostField);
            //Direction Field
            
            nativeBestDirection = AllocNtvAry<float3>(totalNumCells);
            JobHandle jHDirectionField = GetDirectionField(jHIntegrationField);
            jHDirectionField.Complete();

            nativeBestDirection.Reinterpret<Vector3>().CopyTo(directionField);
            DisposeAll();
            return directionField;
        }

        private void DisposeAll()
        {
            if (nativeWalkableChunk.IsCreated) nativeWalkableChunk.Dispose();
            if (nativeCostField.IsCreated)     nativeCostField.Dispose();
            if (nativeBestCostField.IsCreated) nativeBestCostField.Dispose();
            if (nativeBestDirection.IsCreated) nativeBestDirection.Dispose();
        }
        
        public JobHandle GetCostField(int[] walkableChunk, JobHandle dependency = default)
        {
            JCostField job = new JCostField
            {
                MapSize = gridSize,
                ChunkSize = chunkSize,
                WalkableChunk = nativeWalkableChunk,
                CostField = nativeCostField
            };
            return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
        }

        public JobHandle GetIntegrationField(int targetCellIndex, JobHandle dependency = default)
        {
            JIntegrationField job = new JIntegrationField
            {
                DestinationCellIndex = targetCellIndex,
                MapSizeX = gridSize.x,
                CostField = nativeCostField,
                BestCostField = nativeBestCostField
            };
            return job.Schedule(dependency);
        }

        public JobHandle GetDirectionField(JobHandle dependency = default)
        {
            JBestDirection job = new JBestDirection
            {
                MapSizeX = gridSize.x,
                BestCostField = nativeBestCostField,
                CellBestDirection = nativeBestDirection
            };
            return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
        }
    }
}
