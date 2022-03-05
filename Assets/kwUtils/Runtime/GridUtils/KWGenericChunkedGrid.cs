using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWChunk;
using static KWUtils.KWmath;
using float2 = Unity.Mathematics.float2;

namespace KWUtils.KWGenericGrid
{
    /// <summary>
    /// CAREFUL ALL SIZE must be POW 2!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChunkedGrid<T>
    where T : struct
    {
        //Need this in order to get value from world position
        private int2 mapWidthHeight;

        private int chunkSize;
        private int2 chunkWidthHeight;

        private int cellSize;
        private int2 cellWidthHeight;

        private Dictionary<int, T[]> chunkDictionary;
        private readonly T[] gridArray; //cell Independant From the Chunk

        //private Vector3[] chunkCenters;

        private float2 cachedCellCenter = float2.zero; //Use for SetValue
        
        public ChunkedGrid(int mapWidth, int mapHeight, int chunkSize, int cellSize = 1)
        {
            this.chunkSize = chunkSize;
            this.cellSize = cellSize;
            mapWidthHeight = new int2(mapWidth, mapHeight);
            chunkWidthHeight = mapWidthHeight / chunkSize;

            cellWidthHeight = cellSize is 1 ? mapWidthHeight : mapWidthHeight / cellSize ;

            gridArray = new T[cmul(cellWidthHeight)];
            chunkDictionary = gridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, cellWidthHeight));
        }
        
        
        public ChunkedGrid(int2 mapSize, int chunkSize, int cellSize = 1)
        {
            this.chunkSize = chunkSize;
            this.cellSize = cellSize;
            mapWidthHeight = mapSize;
            chunkWidthHeight = mapWidthHeight / chunkSize;
            
            cellWidthHeight = cellSize is 1 ? mapWidthHeight : new int2(mapWidthHeight / cellSize);

            gridArray = new T[cmul(cellWidthHeight)];
            chunkDictionary = gridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, cellWidthHeight));
        }
        
        
        //Get Chunk Value

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
        
        public T GetValueAt(int x, int y)
        {
            return gridArray[y * chunkWidthHeight.x + x];
        }

        public T GetValueAt(in int2 coord)
        {
            return gridArray[coord.y * chunkWidthHeight.x + coord.x];
        }

        public T GetValueFromWorldPosition(Vector3 position)
        {
            return gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)];
        }
        
        // Cell Grid : Set Value
        public void SetValue(int x, int y, T value)
        {
            gridArray[y * chunkWidthHeight.x + x] = value;
            
            cachedCellCenter = new float2(x + 0.5f, y + 0.5f);
            int chunkIndex = cachedCellCenter.GetIndexFromPosition(mapWidthHeight, chunkSize);
            int2 chunkCoord = chunkIndex.GetXY2(mapWidthHeight.x);
            int chunkCellIndex = cachedCellCenter.GetIndexFromPositionOffseted(chunkSize, cellSize, chunkCoord * chunkSize);
        }

        public void SetValueFromPosition(Vector3 position, T value)
        {
            gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)] = value;
        }
    }
}
