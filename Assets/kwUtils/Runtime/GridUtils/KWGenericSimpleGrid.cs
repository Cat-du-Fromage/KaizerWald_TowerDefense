using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{

    /// <summary>
    /// CAREFUL ALL SIZE must be POW 2!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleGrid<T>
    {
        //Need this in order to get value from world position
        private int mapWidth;
        private int mapHeight;
        
        private int gridWidth;
        private int gridHeight;
        private int cellSize;
        
        private readonly T[] gridArray;

        public SimpleGrid(int mapWidth, int mapHeight, int cellSize)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            this.cellSize = cellSize;

            gridArray = new T[gridWidth * gridHeight];
        }
        
        public SimpleGrid(int2 mapSize, int cellSize, Vector3 originPosition)
        {
            mapWidth = mapSize.x;
            mapHeight = mapSize.y;
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            this.cellSize = cellSize;
            
            gridArray = new T[gridWidth * gridHeight];
        }
        
        //Get Value
        public T GetValueAt(int x, int y)
        {
            return gridArray[y * gridWidth + x];
        }

        public T GetValueAt(int2 coord)
        {
            return gridArray[coord.y * gridWidth + coord.x];
        }

        public T GetValueFromWorldPosition(Vector3 position)
        {
            return gridArray[position.GetIndexFromPosition(new int2(mapWidth, mapHeight), cellSize)];
        }
        
        //Set Value
        public void SetValue(int x, int y, T value)
        {
            gridArray[y * gridWidth + x] = value;
        }

        public void SetValueFromPosition(Vector3 position, T value)
        {
            gridArray[position.GetIndexFromPosition(new int2(mapWidth, mapHeight), cellSize)] = value;
        }
    }
}
