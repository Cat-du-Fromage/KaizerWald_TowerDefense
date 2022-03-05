using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;

namespace KWUtils.KWGenericGrid
{

    /// <summary>
    /// CAREFUL ALL SIZE must be POW 2!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleGrid<T>
    {
        //Need this in order to get value from world position

        private readonly int cellSize;
        private readonly int gridWidth;
        private readonly int gridHeight;
        
        private readonly int2 mapWidthHeight;
        private readonly int2 gridBounds;
        
        private readonly T[] gridArray;

        public SimpleGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
            this.cellSize = cellSize;

            mapWidthHeight = mapSize;
            
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            
            gridArray = new T[gridWidth * gridHeight];

            UnityEngine.Debug.Log($"PATH {gridArray.Length}");
            //Init Grid
            for (int i = 0; i < gridArray.Length; i++)
            {
                gridArray[i] = createGridObject(i.GetXY2(gridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize, Func<SimpleGrid<T>, int2 , T> createGridObject)
        {
            this.cellSize = cellSize;

            mapWidthHeight = new int2(mapWidth, mapHeight);
            
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);

            gridArray = new T[gridWidth * gridHeight];

            
            //Init Grid
            for (int i = 0; i < gridArray.Length; i++)
            {
                
                gridArray[i] = createGridObject(this, i.GetXY2(gridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize)
        {
            this.cellSize = cellSize;

            mapWidthHeight = new int2(mapWidth, mapHeight);
            
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);

            gridArray = new T[gridWidth * gridHeight];
        }
        
        public SimpleGrid(in int2 mapSize, int cellSize)
        {
            this.cellSize = cellSize;

            mapWidthHeight = mapSize;
            
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            
            gridArray = new T[gridWidth * gridHeight];
            UnityEngine.Debug.Log($"BUILD {gridArray.Length}");
        }

        public T[] GetGridArray => gridArray;
        public int GridLength => gridArray.Length;

        public int GetGridWidth => gridWidth;
        
        //Get Grid's Cell World Position
        public Vector3 GetCenterCellAt(int index)
        {
            (int x, int z) = index.GetXY(gridWidth);
            //UnityEngine.Debug.Log($"X : {x}; Z : {z}");
            Vector3 pointPosition = (new Vector3(x, 0, z) * cellSize) + (Vector3.one * (cellSize / 2f));
            return pointPosition.Flat();
        }

        //Get Value
        //==============================================================================================================
        public T GetValueAt(int index)
        {
            return gridArray[index];
        }
        public T GetValueAt(int x, int y)
        {
            return gridArray[y * gridWidth + x];
        }

        public T GetValueAt(in int2 coord)
        {
            return gridArray[coord.y * gridWidth + coord.x];
        }

        public T GetValueFromWorldPosition(in Vector3 position)
        {
            return gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)];
        }
        
        //GetIndex from Position
        //==============================================================================================================
        public int GetIndexFromPosition(in Vector3 position)
        {
            return position.XZ().GetIndexFromPosition(mapWidthHeight, cellSize);
        }
        
        //Set Value
        public void SetValue(int index, T value)
        {
            gridArray[index] = value;
        }
        
        public void SetValue(int x, int y, T value)
        {
            gridArray[y * gridWidth + x] = value;
        }
        
        public void SetValue(in int2 coord, T value)
        {
            gridArray[coord.y * gridWidth + coord.x] = value;
        }

        public void SetValueFromPosition(in Vector3 position, T value)
        {
            gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)] = value;
        }
    }
}
