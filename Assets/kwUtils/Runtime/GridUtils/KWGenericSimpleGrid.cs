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
        
        public readonly T[] GridArray;

        public SimpleGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
            this.cellSize = cellSize;

            mapWidthHeight = mapSize;
            
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            
            GridArray = new T[gridWidth * gridHeight];
            
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i.GetXY2(gridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize, Func<SimpleGrid<T>, int2 , T> createGridObject)
        {
            this.cellSize = cellSize;

            mapWidthHeight = new int2(mapWidth, mapHeight);
            
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);

            GridArray = new T[gridWidth * gridHeight];

            
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                
                GridArray[i] = createGridObject(this, i.GetXY2(gridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize)
        {
            this.cellSize = cellSize;

            mapWidthHeight = new int2(mapWidth, mapHeight);
            
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);

            GridArray = new T[gridWidth * gridHeight];
        }
        
        public SimpleGrid(in int2 mapSize, int cellSize)
        {
            this.cellSize = cellSize;

            mapWidthHeight = mapSize;
            
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            
            GridArray = new T[gridWidth * gridHeight];
            UnityEngine.Debug.Log($"BUILD {GridArray.Length}");
        }

        public T[] GetGridArray => GridArray;
        public int GridLength => GridArray.Length;

        public int GetGridWidth => gridWidth;
        
        //Get Grid's Cell World Position
        public Vector3 GetCenterCellAt(int index)
        {
            (int x, int z) = index.GetXY(gridWidth);
            Vector3 pointPosition = (new Vector3(x, 0, z) * cellSize) + (Vector3.one * (cellSize / 2f));
            return pointPosition.Flat();
        }

        //Get Value
        //==============================================================================================================
        public T this[int index] => GridArray[index];
        public T this[int x, int y] => GridArray[y * gridWidth + x];
        public T this[in int2 coord] => GridArray[coord.y * gridWidth + coord.x];
        /*
        public T GetValueAt(int index)
        {
            return GridArray[index];
        }
        public T GetValueAt(int x, int y)
        {
            return GridArray[y * gridWidth + x];
        }

        public T GetValueAt(in int2 coord)
        {
            return GridArray[coord.y * gridWidth + coord.x];
        }
*/
        public T GetValueFromWorldPosition(in Vector3 position)
        {
            return GridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)];
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
            GridArray[index] = value;
        }
        
        public void SetValue(int x, int y, T value)
        {
            GridArray[y * gridWidth + x] = value;
        }
        
        public void SetValue(in int2 coord, T value)
        {
            GridArray[coord.y * gridWidth + coord.x] = value;
        }

        public void SetValueFromPosition(in Vector3 position, T value)
        {
            GridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)] = value;
        }
    }
}
