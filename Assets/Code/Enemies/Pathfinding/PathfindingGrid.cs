using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;
using static KWUtils.KWmath;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.VirtualTexturing;
#endif

namespace TowerDefense
{
    public partial class PathfindingGrid : MonoBehaviour
    {
        [SerializeField] private int SpawningChunkIndex;
        [SerializeField] private TerrainData terrainData;
        [SerializeField] private int ChunkSize = 16;

        private Vector3[] directionsGrid;
        private Dictionary<int, Vector3[]> directionChunkGrid;

        private int2 gridSize;
        private int2 numChunkXY;

        //Path Data
        //=====================
        private int[] walkableChunk = new[] { 1, 9, 10, 11, 19, 27, 33, 34, 35, 41, 49, 50, 51, 52, 53, 54 };

        private Road[] walkableRoad = new[]
        {
            Road.Vertical, Road.BotRight, Road.Horizontal, Road.TopLeft,
            Road.Vertical, Road.Vertical, Road.BotLeft, Road.Horizontal,
            Road.TopRight, Road.Vertical, Road.BotRight, Road.Horizontal,
            Road.Horizontal, Road.Horizontal, Road.Horizontal, Road.Horizontal,
        };
        
        private int destinationChunk = 54;
        private int destinationGridCell = -1;
        
        //Accessors
        //=====================
        public int2 GridSize => gridSize;
        public Vector3[] DirectionsGrid => directionsGrid;
        
        private void Awake()
        {
            InitializeFields();
        }

        private void Start()
        {
            //DEBUG
            walkableChunkObj = new GameObject[walkableChunk.Length];
            ShowWalkableArea();

            GetFlowField();
        }

        private void InitializeFields()
        {
            gridSize = (int2)terrainData.size.XZ();
            ChunkSize = ceilpow2(ChunkSize);
            numChunkXY = (int2)(terrainData.size.XZ() / new int2(ChunkSize));
        }
        
        //Spawn Point
        //==============================================================================================================

        //Get Position Point depending on number of enemy to spawn (must be pow2)
        public Vector3[] GetSpawnPointsForEntities(int numToSpawn, int separation)
        {
            int spawns = max(ceilpow2(numToSpawn),4);
            Vector3[] spawnPoints = new Vector3[spawns];
            
            int2 xyPos = SpawningChunkIndex.GetXY2(numChunkXY.x);
            
            Vector3 centerSpawnCell = new Vector3((xyPos.x * ChunkSize) + ChunkSize / 2f, 0, (xyPos.y * ChunkSize) + ChunkSize / 2f);

            int spawnSquareHalfExtent = spawns / 4;

            Vector3 topLeftSpawnZ = new Vector3(centerSpawnCell.x - spawnSquareHalfExtent, 0, centerSpawnCell.z + spawnSquareHalfExtent);

            for (int i = 0; i < spawns; i++)
            {
                int2 xy = i.GetXY2(spawnSquareHalfExtent) * separation;
                Vector3 offset = (xy.x * Vector3.right) + (xy.y * Vector3.back); //each iteration we go right then down(back in 3D)
                spawnPoints[i] = topLeftSpawnZ + offset;
            }
            
            return spawnPoints;
        }
        
        //FlowField
        //==============================================================================================================
        private void GetFlowField()
        {
            GridData gridData = new GridData(ChunkSize, gridSize);
            destinationGridCell = destinationChunk.GetCellIndexFromChunkEnterPoint(ChunkEnterPoint.Right, gridData);
            
            FlowField flowField = new FlowField(gridSize, ChunkSize);
            directionsGrid = flowField.GetFlowField(destinationGridCell, walkableChunk, walkableRoad);
            directionChunkGrid = directionsGrid.GetGridValueOrderedByChunk(gridData);
            //grid = KWChunk.GetCellIndicesOrderedByChunk(flowField.BestCostField, gridData);
            //CostField = flowField.CostField;

            //costGrid = KWChunk.GetCellCostOrderedByChunk(CostField, gridData);
        }
    }
}
