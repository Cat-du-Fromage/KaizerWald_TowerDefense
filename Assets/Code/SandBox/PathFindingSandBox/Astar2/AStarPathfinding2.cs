using System;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{
    public class AStarPathfinding2 : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        private int2 mapBounds;

        private const int CellSize = 1;
        private const float HalfCell = 0.5f;
        
        private SimpleGrid<Node2> nodeGrid;
        
        private void Awake()
        {
            terrain ??= Terrain.activeTerrain;
            mapBounds = (int2)terrain.terrainData.size.XZ();
            
            
        }

        private void Start()
        {
            nodeGrid = new SimpleGrid<Node2>(mapBounds, CellSize, (int2 coord) => new Node2(coord));
        }
    }
    
    [BurstCompile]
    public struct JAStar2 : IJob
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
}