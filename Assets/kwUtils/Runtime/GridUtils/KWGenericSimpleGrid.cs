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
    public class SimpleGrid<T> : IGenericGrid<T>
    where T : struct
    {
        //Need this in order to get value from world position
        
        public int CellSize        { get; private set; }
        public int GridWidth       { get; private set; }
        public int GridHeight      { get; private set; }
        public int2 MapWidthHeight { get; private set; }
        public int2 GridBounds     { get; private set; }
        public T[] GridArray       { get; private set; }
        

        public SimpleGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
            this.CellSize = cellSize;

            MapWidthHeight = mapSize;
            
            GridWidth = mapSize.x / cellSize;
            GridHeight = mapSize.y / cellSize;
            
            GridBounds = new int2(GridWidth, GridHeight);
            
            GridArray = new T[GridWidth * GridHeight];
            
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i.GetXY2(GridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize, Func<SimpleGrid<T>, int2 , T> createGridObject)
        {
            this.CellSize = cellSize;
            MapWidthHeight = new int2(mapWidth, mapHeight);
            
            GridWidth = mapWidth / cellSize;
            GridHeight = mapHeight / cellSize;
            GridBounds = new int2(GridWidth, GridHeight);

            GridArray = new T[GridWidth * GridHeight];
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(this, i.GetXY2(GridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize)
        {
            this.CellSize = cellSize;

            MapWidthHeight = new int2(mapWidth, mapHeight);
            
            GridWidth = mapWidth / cellSize;
            GridHeight = mapHeight / cellSize;
            
            GridBounds = new int2(GridWidth, GridHeight);
            GridArray = new T[GridWidth * GridHeight];
        }
        
        public SimpleGrid(in int2 mapSize, int cellSize)
        {
            this.CellSize = cellSize;

            MapWidthHeight = mapSize;
            
            GridWidth = mapSize.x / cellSize;
            GridHeight = mapSize.y / cellSize;
            
            GridBounds = new int2(GridWidth, GridHeight);
            
            GridArray = new T[GridWidth * GridHeight];
        }

        public T[] GetGridArray => GridArray;
        public int GridLength => GridArray.Length;

        public int GetGridWidth => GridWidth;
        
        //Get Grid's Cell World Position
        public Vector3 GetCenterCellAt(int index)
        {
            (int x, int z) = index.GetXY(GridWidth);
            Vector3 pointPosition = (new Vector3(x, 0, z) * CellSize) + (Vector3.one * (CellSize / 2f));
            return pointPosition.Flat();
        }

        //Get Value
        //==============================================================================================================
        public T this[int index]
        {
            get => GridArray[index];
            set => GridArray[index] = value;
        }

        public T this[int x, int y]
        {
            get => GridArray[y * GridWidth + x];
            set => GridArray[y * GridWidth + x] = value;
        }

        public T this[in int2 coord]
        {
            get => GridArray[coord.y * GridWidth + coord.x];
            set => GridArray[coord.y * GridWidth + coord.x] = value;
        }

        
        
        //Operation from World Position
        //==============================================================================================================
        public int IndexFromPosition(in Vector3 position)
        {
            return position.XZ().GetIndexFromPosition(MapWidthHeight, CellSize);
        }
        
        public T GetValueFromWorldPosition(in Vector3 position)
        {
            return GridArray[position.GetIndexFromPosition(MapWidthHeight, CellSize)];
        }

        public void SetValueFromPosition(in Vector3 position, T value)
        {
            GridArray[position.GetIndexFromPosition(MapWidthHeight, CellSize)] = value;
        }

        
    }
}
