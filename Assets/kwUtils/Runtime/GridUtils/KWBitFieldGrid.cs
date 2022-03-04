using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using static KWUtils.KWChunk;
using static KWUtils.KWmath;

namespace KWUtils
{
    public class ChunkedBitFieldGrid
    {
        //Need this in order to get value from world position
        private int2 mapWidthHeight;

        private int chunkSize;
        private int2 chunkWidthHeight;

        private int cellSize;
        private int2 cellWidthHeight;

        private Dictionary<int, BitField32> chunkBitfield;
        private readonly bool[] gridArray; //cell Independant From the Chunk

        //private Vector3[] chunkCenters;
        
        public ChunkedBitFieldGrid(int mapWidth, int mapHeight, int chunkSize, int cellSize = 1)
        {
            this.chunkSize = chunkSize;
            this.cellSize = cellSize;
            mapWidthHeight = new int2(mapWidth, mapHeight);
            chunkWidthHeight = mapWidthHeight / chunkSize;

            cellWidthHeight = cellSize is 1 ? mapWidthHeight : mapWidthHeight / cellSize ;

            gridArray = new bool[cmul(cellWidthHeight)];
            
            int capacity = cmul(chunkWidthHeight);
            chunkBitfield = new Dictionary<int, BitField32>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                chunkBitfield.Add(i, new BitField32());
            }
        }
        
        
        public ChunkedBitFieldGrid(int2 mapSize, int chunkSize, int cellSize = 1)
        {
            this.chunkSize = chunkSize;
            this.cellSize = cellSize;
            mapWidthHeight = mapSize;
            chunkWidthHeight = mapWidthHeight / chunkSize;
            
            cellWidthHeight = cellSize is 1 ? mapWidthHeight : new int2(mapWidthHeight / cellSize);

            gridArray = new bool[cmul(cellWidthHeight)];

            int capacity = cmul(chunkWidthHeight);
            chunkBitfield = new Dictionary<int, BitField32>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                chunkBitfield.Add(i, new BitField32());
            }
        }
        
        
        //Get Chunk Value
        public int ChunkIndexAtCoord(int x, int y)
        {
            float2 centerCell = new float2(x + cellSize/2f, y + cellSize/2f);
            return centerCell.GetIndexFromPosition(mapWidthHeight, chunkSize);
        }
        
        public int ChunkIndexAtPosition(Vector3 position)
        {
            float2 centerCell = new float2(position.x, position.z);
            return centerCell.GetIndexFromPosition(mapWidthHeight, chunkSize);
        }

        public Vector3 GetChunkCenterAt(int index)
        {
            (int x, int z) = index.GetXY(chunkWidthHeight.x); //we offset by 1,0,1 so we just remove the last one
            Vector3 pointPosition = (new Vector3(x, 0, z) * chunkSize) + (Vector3.one * (chunkSize / 2f)); //* cellBound not needed this time since value = Vector3(1,1,1)
            return pointPosition.Flat();
        }
        

        // Cell Grid : Get Value
        public Vector3 GetCellCenterAt(int index)
        {
            (int x, int z) = index.GetXY(cellWidthHeight.x);
            Vector3 pointPosition = (new Vector3(x, 0, z) * cellSize) + (Vector3.one * (cellSize / 2f));
            return pointPosition.Flat();
        }
        
        public bool GetValueAt(int x, int y)
        {
            return gridArray[y * cellWidthHeight.x + x];
        }

        public bool GetValueAt(in int2 coord)
        {
            return gridArray[coord.y * cellWidthHeight.x + coord.x];
        }

        public bool GetValueFromWorldPosition(Vector3 position)
        {
            return gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)];
        }
        
        //Retrieve bot chunk Index and the cell Index from a coord
        private (int, int) GetChunkCellIndexFromCellArray(int x, int y, float offset = 0)
        {
            float2 centerCell = new float2(x + offset, y + offset); // OK
            
            int chunkIndex = centerCell.GetIndexFromPosition(mapWidthHeight, chunkSize);
            UnityEngine.Debug.Log(chunkIndex);
            int2 chunkCoord = chunkIndex.GetXY2(cellWidthHeight.x);
            int chunkCellIndex = centerCell.GetIndexFromPosition(mapWidthHeight, cellSize, chunkCoord * chunkSize);
//UnityEngine.Debug.Log(chunkIndex);
            return (chunkIndex, chunkCellIndex);
        }
        
        // Cell Grid : Set Value
        public void SetValue(int x, int y, bool value)
        {
            gridArray[y * cellWidthHeight.x + x] = value;
            //CHECK THIS
            (int chunkIndex, int chunkCellIndex) = GetChunkCellIndexFromCellArray(x, y, cellSize/2f);
            chunkBitfield[chunkIndex].SetBits(chunkCellIndex, value);
        }

        public void SetValueFromPosition(Vector3 position, bool value)
        {
            gridArray[position.GetIndexFromPosition(cellWidthHeight, cellSize)] = value;
            //CHECK THIS
            (int chunkIndex, int chunkCellIndex) = GetChunkCellIndexFromCellArray((int)position.x, (int)position.y);
            chunkBitfield[chunkIndex].SetBits(chunkCellIndex, value);
        }
    }
}