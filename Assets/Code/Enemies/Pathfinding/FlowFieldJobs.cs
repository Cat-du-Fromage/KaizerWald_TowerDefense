using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace TowerDefense
{

    /// <summary>
    /// CAREFUL FOR BLENDING
    /// NEED TO KNOW which cell is directly near an unwalkable chunk
    /// So we can set a value of 2 for the costfield
    /// </summary>
    //[BurstCompile(CompileSynchronously = true)]
    public struct JCostField : IJobFor
    {
        [ReadOnly] public int2 MapSize;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public NativeArray<int> WalkableChunk;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<byte> CostField;
        public void Execute(int index)
        {
            int2 coord = index.GetXY2(MapSize.x);
                
            float2 currentCellCenter = coord + new float2(0.5f);
                
            int chunkIndex = GetChunkIndex(currentCellCenter);

            CostField[index] = WalkableChunk.Contains(chunkIndex) ? (byte)1 : byte.MaxValue;
        }
        private int GetChunkIndex(in float2 pointPos)
        {
            float2 percents = pointPos / (MapSize * ChunkSize);
            percents = clamp(percents, float2.zero, float2(1f));
            int2 xy =  clamp((int2)floor(MapSize * percents), 0, MapSize - 1);
            return mad(xy.y, MapSize.x/ChunkSize, xy.x);
        }
    }
    
    public struct JSmoothCostField : IJobFor
    {
        [ReadOnly] public int2 MapSize;
        [ReadOnly] public int ChunkSize;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<Road> RoadConfig;
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<int> WalkableChunk;


        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<byte> CostField;
        public void Execute(int index)
        {
            int2 cellCoord = index.GetXY2(ChunkSize);
            int chunkIndex = GetChunkIndex(cellCoord);
            int2 chunkCoord = chunkIndex.GetXY2(MapSize.x);

            int2 cellChunkCoord = cellCoord - (chunkCoord * ChunkSize);

            int walkChunk = WalkableChunk.IndexOf(chunkIndex);
            if (walkChunk != -1)
            {
                int value = CostField[index];
                int oddOffset = (ChunkSize & 1);
                Road config = RoadConfig[walkChunk];

                if (config == Road.None) return;

                value = config switch
                {
                    Road.Horizontal => (int)ceil(cellChunkCoord.y - (ChunkSize / 2f)),
                    Road.Vertical => (int)ceil(cellChunkCoord.x - (ChunkSize / 2f)),
                    _ => value
                };
/*
                if (config == Road.Horizontal)
                {
                    value = (int)ceil(cellChunkCoord.y - (ChunkSize / 2f)); //PROBLEM!!! NEED INDEX INSIDE CHUNK
                }
                else if (config == Road.Vertical)
                {
                    value = (int)ceil(cellChunkCoord.x - (ChunkSize / 2f));
                }
                */
                value += value >= 0 ? 1 : 0; //readjust this on the Half Top (see Excel Graph)
                value = value < 0 ? abs(value) + oddOffset : value; //negative Value are made positiv and if ChunkSize is Odd => + 1 (see Excel Graph)
            
                CostField[index] = (byte)value;
            }
        }

        private int GetChunkIndex(in float2 pointPos)
        {
            float2 percents = pointPos / (MapSize * ChunkSize);
            percents = clamp(percents, float2.zero, float2(1f));
            int2 xy =  clamp((int2)floor(MapSize * percents), 0, MapSize - 1);
            return mad(xy.y, MapSize.x/ChunkSize, xy.x);
        }
    }
    /*
    public struct JSmoothCostField : IJobFor
    {
        [ReadOnly] public int ChunkIndex;
        
        [ReadOnly] public int2 MapSize;
        [ReadOnly] public int ChunkSize;
        
        [ReadOnly] public Road RoadConfig;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<int> WalkableChunk;


        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<byte> CostField;
        public void Execute(int index)
        {
            int2 cellCoordInChunk = index.GetXY2(ChunkSize);
            
            //On a le Chunk
            int gridCellIndex = ChunkIndex.GetGridCellIndexFromChunkCellIndex(MapSize.x, ChunkSize, index);
            int value = CostField[gridCellIndex];
            //=> on a la config
            
            //Check IfOdd
            int oddOffset = (ChunkSize & 1); // add when number are negativ

            if (RoadConfig == Road.Horizontal)
            {
                value = (int)ceil(cellCoordInChunk.y - (ChunkSize / 2f));
                //int negativCheck = value <= 0 ? (int)floor(value) : (int)ceil(value); //NEED CHECK
            }
            else if (RoadConfig == Road.Vertical)
            {
                value = (int)ceil(cellCoordInChunk.x - (ChunkSize / 2f));
            }
            value += value >= 0 ? 1 : 0; //readjust this on the Half Top (see Excel Graph)
            value = value < 0 ? abs(value) + oddOffset : value; //negative Value are made positiv and if ChunkSize is Odd => + 1 (see Excel Graph)
            
            CostField[gridCellIndex] = (byte)value;
        }

        private int GetChunkIndex(in float2 pointPos)
        {
            float2 percents = pointPos / (MapSize * ChunkSize);
            percents = clamp(percents, float2.zero, float2(1f));
            int2 xy =  clamp((int2)floor(MapSize * percents), 0, MapSize - 1);
            return mad(xy.y, MapSize.x/ChunkSize, xy.x);
        }
    }
    */
    //[BurstCompile(CompileSynchronously = true)]
    public struct JIntegrationField : IJob
    {
        [ReadOnly] public int DestinationCellIndex;
        [ReadOnly] public int MapSizeX;
        
        public NativeArray<byte> CostField;
        public NativeArray<int> BestCostField;

        public void Execute()
        {
            NativeQueue<int> cellsToCheck = new NativeQueue<int>(Allocator.Temp);
            NativeList<int> currentNeighbors = new NativeList<int>(4,Allocator.Temp);
            
            //Set Destination cell cost at 0
            CostField[DestinationCellIndex] = 0;
            BestCostField[DestinationCellIndex] = 0;
            
            cellsToCheck.Enqueue(DestinationCellIndex);
            
            while(cellsToCheck.Count > 0)
            {
                int currentCellIndex = cellsToCheck.Dequeue();
                GetNeighborCells(currentCellIndex, ref currentNeighbors);

                for (int i = 0; i < currentNeighbors.Length; i++)
                {
                    int neighborIndex = currentNeighbors[i];
                    if (CostField[neighborIndex] >= byte.MaxValue) continue;
                    if (CostField[neighborIndex] + BestCostField[currentCellIndex] < BestCostField[neighborIndex])
                    {
                        BestCostField[neighborIndex] = CostField[neighborIndex] + BestCostField[currentCellIndex];
                        cellsToCheck.Enqueue(neighborIndex);
                    }
                }
                currentNeighbors.Clear();
            }

            currentNeighbors.Dispose();
            cellsToCheck.Dispose();
        }
        
        private void GetNeighborCells(int index, ref NativeList<int> curNeighbors)
        {
            int2 coord = index.GetXY2(MapSizeX);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, MapSizeX);
                if (neighborId == -1) continue;
                curNeighbors.Add(neighborId);
            }
        }
    }
    
    //[BurstCompile(CompileSynchronously = true)]
    public struct JBestDirection : IJobFor
    {
        [ReadOnly] public int MapSizeX;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<int> BestCostField;
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> CellBestDirection;
        
        public void Execute(int index)
        {
            int currentBestCost = BestCostField[index];

            if (currentBestCost >= ushort.MaxValue)
            {
                CellBestDirection[index] = float3.zero;
                return;
            }
            
            NativeList<int> neighbors = new NativeList<int>(8,Allocator.Temp);
            
            GetNeighborCells(index, ref neighbors);

            int2 currentCellCoord = index.GetXY2(MapSizeX);

            for (int i = 0; i < neighbors.Length; i++)
            {
                int currentNeighbor = neighbors[i];
                if(BestCostField[currentNeighbor] < currentBestCost)
                {
                    currentBestCost = BestCostField[currentNeighbor];
                    int2 neighborCoord = currentNeighbor.GetXY2(MapSizeX);
                    int2 bestDirection = neighborCoord - currentCellCoord;
                    CellBestDirection[index] = new float3(bestDirection.x, 0, bestDirection.y);
                }
            }
            neighbors.Dispose();
        }
        
        private void GetNeighborCells(int index, ref NativeList<int> neighbors)
        {
            int2 coord = index.GetXY2(MapSizeX);
            for (int i = 0; i < 8; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, MapSizeX);
                if (neighborId == -1) continue;
                neighbors.Add(neighborId);
            }
        }
    }
}
