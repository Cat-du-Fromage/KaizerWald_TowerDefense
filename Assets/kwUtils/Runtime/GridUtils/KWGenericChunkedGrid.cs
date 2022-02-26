using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWChunk;

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

        public ChunkedGrid(int mapWidth, int mapHeight, int chunkSize, int cellSize)
        {
            this.chunkSize = chunkSize;
            this.cellSize = cellSize;
            mapWidthHeight = new int2(mapWidth, mapHeight);
            chunkWidthHeight = mapWidthHeight / chunkSize;

            gridArray = new T[mapWidth * mapHeight];
            chunkDictionary = gridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, mapWidthHeight));
        }
        
        
        public ChunkedGrid(int2 mapSize, int chunkSize, int cellSize)
        {
            this.chunkSize = chunkSize;
            this.cellSize = cellSize;
            mapWidthHeight = mapSize;
            chunkWidthHeight = mapWidthHeight / chunkSize;

            gridArray = new T[mapSize.x * mapSize.y];
            chunkDictionary = gridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, mapWidthHeight));
        }
        
        // Cell Grid : Get Value
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
        }

        public void SetValueFromPosition(Vector3 position, T value)
        {
            gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)] = value;
        }
    }
}
