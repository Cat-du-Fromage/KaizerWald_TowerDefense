using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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
    public class ChunkedGrid<T> : IGenericGrid<T>
    where T : struct
    {
        
        public int CellSize        { get; private set; }
        public int GridWidth       { get; private set; }
        public int GridHeight      { get; private set; }
        public int2 MapWidthHeight { get; private set; }
        public int2 GridBounds     { get; private set; }
        public T[] GridArray       { get; private set; }

        //Need this in order to get value from world position

        private int ChunkSize;
        private int2 ChunkWidthHeight;
        
        private int2 CellWidthHeight;
        
        private Dictionary<int, T[]> chunkDictionary;
        //private T[] arrayGrid; //cell Independant From the Chunk
        
        //TODO : Try Make a Composition, instead of arrayGrid => SimpleGrid<T>

        private float2 cachedCellCenter = float2.zero; //Use for SetValue

        private void BaseConstructor(int mapWidth, int mapHeight, int chunksSize, int cellsSize)
        {
            ChunkSize = chunksSize;
            CellSize = cellsSize;
            MapWidthHeight = new int2(mapWidth, mapHeight);
            ChunkWidthHeight = MapWidthHeight / ChunkSize;
            CellWidthHeight = CellSize is 1 ? MapWidthHeight : MapWidthHeight / CellSize ;
            GridArray = new T[cmul(CellWidthHeight)];
        }
        
        public ChunkedGrid(int mapWidth, int mapHeight, int chunkSize, int cellSize = 1)
        {
            BaseConstructor(mapWidth, mapHeight, chunkSize, cellSize);

            chunkDictionary = GridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, CellWidthHeight));
        }
        
        
        public ChunkedGrid(int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null)
        {
            BaseConstructor(mapSize.x, mapSize.y, chunkSize, cellSize);

            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray);
            chunkDictionary = GridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, CellWidthHeight));
        }


        /// <summary>
        /// Get values contained in the chunk
        /// </summary>
        public T[] this[int index] => chunkDictionary[index];
        public T[] this[int x, int y] => chunkDictionary[y*ChunkWidthHeight.x+x];
        public T[] this[int2 coord] => chunkDictionary[coord.y * ChunkWidthHeight.x + coord.x];
        
        
        /// <summary>
        /// Get World position of the chunk
        /// </summary>
        /// <param name="index"></param>
        /// <returns>chunk's center's world position</returns>
        public Vector3 ChunkCenterAt(int index)
        {
            (int x, int z) = index.GetXY(ChunkWidthHeight.x); //we offset by 1,0,1 so we just remove the last one
            Vector3 pointPosition = (new Vector3(x, 0, z) * ChunkSize) + (Vector3.one * (ChunkSize / 2f)); //* cellBound not needed this time since value = Vector3(1,1,1)
            return pointPosition.Flat();
        }


        // Array Grid : Get Value
        public Vector3 CellCenterAt(int index)
        {
            (int x, int z) = index.GetXY(CellWidthHeight.x);
            Vector3 pointPosition = (new Vector3(x, 0, z) * CellSize) + (Vector3.one * (CellSize / 2f));
            return pointPosition.Flat();
        }
        //==============================================================================================================
        //Get Values given coords
        //==============================================================================================================
        public T ArrayValueAt(int x, int y) => GridArray[y * ChunkWidthHeight.x + x];
        public T ArrayValueAt(in int2 coord) => GridArray[coord.y * ChunkWidthHeight.x + coord.x];
        public T ArrayValueFromWorldPosition(in Vector3 position) => GridArray[position.GetIndexFromPosition(MapWidthHeight, CellSize)];

        
        // Cell Grid : Set Value
        public void SetValue(int x, int y, T value)
        {
            GridArray[y * ChunkWidthHeight.x + x] = value;
            
            cachedCellCenter = new float2(x + 0.5f, y + 0.5f);
            int chunkIndex = cachedCellCenter.GetIndexFromPosition(MapWidthHeight, ChunkSize);
            int2 chunkCoord = chunkIndex.GetXY2(MapWidthHeight.x);
            int chunkCellIndex = cachedCellCenter.GetIndexFromPositionOffseted(ChunkSize, CellSize, chunkCoord * ChunkSize);
        }

        public void SetValueFromPosition(Vector3 position, T value)
        {
            GridArray[position.GetIndexFromPosition(MapWidthHeight, CellSize)] = value;
        }
    }
}
