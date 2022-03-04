using System;
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
    public enum Road : byte
    {
        Vertical,
        Horizontal,

        //I = Input(Enter)
        //O = Output(Exit)
        
        //Configuration : x : inverse (9, 8, 7, .., 0) | y : normal (0, 1, 2, .., 9)
        //  ________
        // |       |
        // |       O
        // |___I___|
        BotRight, //use MAX(x,y)
        //  ___O____
        // |       |
        // I       |
        // |_______|
        TopLeft, //use MIN(x,y)
        
        //Configuration : x : y : normal (0, 1, 2, .., 9) | y : normal (0, 1, 2, .., 9)
        
        //  ________
        // |       |
        // O       |
        // |___I___|
        BotLeft, //use MIN(x,y)
        //  ___O____
        // |       |
        // |       I
        // |_______|
        TopRight, //use MAX(x,y)
        
        None,
    }
    
    public class FlowField
    {
        private readonly int2 gridSize;
        private readonly int chunkSize;
        private readonly int totalNumCells;

        //CostField
        private NativeArray<int> nativeWalkableChunk;
        private NativeArray<byte> nativeCostField;

        private NativeArray<Road> nativeRoadConfig;

        //IntegrationField
        private NativeArray<int> nativeBestCostField;
        //public int[] BestCostField;
        
        //FlowField
        private NativeArray<float3> nativeBestDirection;
        
        //public byte[] CostField;

        public FlowField(int2 gridSize, int chunkSize)
        {
            this.gridSize = gridSize;
            this.chunkSize = chunkSize;
            totalNumCells = gridSize.x * gridSize.y;
        }
        
        public Vector3[] GetFlowField(int targetCell, int[] walkableChunk, Road[] walkableRoad)
        {
            Vector3[] directionField = new Vector3[totalNumCells];
            
            //Cost Field
            nativeWalkableChunk = walkableChunk.ToNativeArray();
            nativeCostField = AllocNtvAry<byte>(totalNumCells);
            JobHandle jHCostField = GetCostField();
            
            //Smooth Cost Field
            nativeRoadConfig = walkableRoad.ToNativeArray();
            JobHandle jHSmoothCostField = GetSmoothCostField(jHCostField);

            //Integration Field
            nativeBestCostField = AllocFillNtvAry<int>(totalNumCells, ushort.MaxValue);
            JobHandle jHIntegrationField = GetIntegrationField(targetCell, jHSmoothCostField);
            
            //Direction Field
            nativeBestDirection = AllocNtvAry<float3>(totalNumCells);
            JobHandle jHDirectionField = GetDirectionField(jHIntegrationField);
            jHDirectionField.Complete();

            //CostField = new byte[nativeCostField.Length];
            //nativeCostField.CopyTo(CostField);

            //Return value
            nativeBestDirection.Reinterpret<Vector3>().CopyTo(directionField);
            DisposeAll();
            return directionField;
        }

        private void DisposeAll()
        {
            if (nativeWalkableChunk.IsCreated) nativeWalkableChunk.Dispose();
            if (nativeCostField.IsCreated)     nativeCostField.Dispose();
            if (nativeRoadConfig.IsCreated)    nativeRoadConfig.Dispose();
            if (nativeBestCostField.IsCreated) nativeBestCostField.Dispose();
            if (nativeBestDirection.IsCreated) nativeBestDirection.Dispose();
        }
        
        public JobHandle GetCostField(JobHandle dependency = default)
        {
            JCostField job = new JCostField
            {
                MapSize = gridSize,
                ChunkSize = chunkSize,
                WalkableChunk = nativeWalkableChunk,
                CostField = nativeCostField,
            };
            return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
        }

        public JobHandle GetSmoothCostField(JobHandle dependency = default)
        {
            JSmoothCostField job = new JSmoothCostField
            {
                MapSize = gridSize,
                ChunkSize = chunkSize,
                RoadConfig = nativeRoadConfig,
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
