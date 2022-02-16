using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{
    public struct SpCell
    {
        private int index;
        private int numEntityInside;
        private List<uint> entitiesId;

        public SpCell(int id)
        {
            index = id;
            numEntityInside = 0;
            entitiesId = new List<uint>(4);
        }
    }

    public class SandBox_SpatialPartition : MonoBehaviour
    {
        [SerializeField] private TerrainData terrainData;
        private SpCell[] cells;
        private int cellSize = 2;
        private int numCellX;
        private int numCellY;
        private int totalNumCell;

        private void Awake()
        {
            numCellX = cellSize * (int)terrainData.size.x;
            numCellY = cellSize * (int)terrainData.size.z;
            totalNumCell = numCellX * numCellY;
            cells = new SpCell[totalNumCell];
        }

        private void Start()
        {
            for (int i = 0; i < totalNumCell; i++)
            {
                cells[i] = new SpCell(i);
            }
        }

        public int GetCellIndexFromPosition(Vector3 pointPos, int2 numCellsOnAxis)
        {
            float percentX = pointPos.x / (numCellsOnAxis.x * cellSize); //CHECK may no need to mak numcell * cellSize
            float percentY = pointPos.z / (numCellsOnAxis.y * cellSize);

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);
            
            int x = Mathf.Clamp(Mathf.FloorToInt(numCellsOnAxis.x * percentX), 0, numCellsOnAxis.x-1);
            int y = Mathf.Clamp(Mathf.FloorToInt(numCellsOnAxis.y * percentY), 0, numCellsOnAxis.y-1);

            return (y * numCellsOnAxis.x) + x;
        }
    }
}
