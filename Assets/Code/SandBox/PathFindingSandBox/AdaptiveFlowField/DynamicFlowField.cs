using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace TowerDefense
{
    public class DynamicFlowField : MonoBehaviour, IGridHandler<Vector3, ChunkedGrid<Vector3>>
    {
        //Interface
        public IGridSystem GridSystem { get; set; }
        public ChunkedGrid<Vector3> Grid { get; private set; }

        [SerializeField] private int ChunkSize = 16;
        private int2 numChunkXY;
        
        private void Awake()
        {
            int2 terrainBounds = TerrainDataProvider.Instance.TerrainWidthHeight;
            ChunkSize = ceilpow2(ChunkSize);
            numChunkXY = (terrainBounds / new int2(ChunkSize));
            
        }
        
        public void InitGrid(int2 mapSize, int chunkSize, int cellSize = 1, Func<int2, ChunkedGrid<Vector3>> providerFunction = null)
        {
            Grid = new ChunkedGrid<Vector3>(mapSize, ChunkSize);
        }
    }
}
