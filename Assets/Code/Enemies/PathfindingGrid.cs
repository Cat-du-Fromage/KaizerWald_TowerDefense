using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.VirtualTexturing;
#endif

namespace TowerDefense
{
    public class PathfindingGrid : MonoBehaviour
    {
        [SerializeField] private int SpawningChunkIndex;
        
        public bool EnableDebugger;
        
        public TerrainData terrainData;
        [SerializeField] private int ChunkSize = 16;
        
        private Dictionary<int, int[]> grid;
        
        private int2 gridSize;
        private int2 numChunkXY;
        
        private int totalChunk;

        //private byte[] costField;
        //private short[] integrationField;
        private Vector2[] directionField;

#if UNITY_EDITOR   
        //SPAWNING POINT
        [SerializeField] private GameObject SpawnArea;
        public bool ShowSpawnChunk;
        
        [HideInInspector]
        [SerializeField]private Vector3[] chunksPosition;
#endif

        private int GetChunkIndex(int cellIndex) => Mathf.FloorToInt((float)cellIndex / (float)ChunkSize);
        
        //1) Walkable Chunk
        
        
        //2) Weight/blur on Walkable Chunk cells (Cost Grid)
        
        
        
        private void OnValidate()
        {
            
            InitializeFields();
            if (chunksPosition.Length == 0 || chunksPosition.Length != totalChunk)
            {
                chunksPosition = new Vector3[totalChunk];
            }
            InitializeChunkGrid();
            SpawningChunkIndex = Mathf.Clamp(SpawningChunkIndex, 0, totalChunk - 1);
            MoveSpawnArea();
            ShowHideSpawnArea(ShowSpawnChunk);
        }
        
        private void Awake()
        {
            InitializeFields();
        }

        private void Start()
        {
            GetCostField();
        }

        private void InitializeFields()
        {
            gridSize = (int2)terrainData.size.XZ();
            ChunkSize = ceilpow2(ChunkSize);
            numChunkXY = (int2)(terrainData.size.XZ() / new int2(ChunkSize));
            totalChunk = numChunkXY.x * numChunkXY.y;
        }
        
        //Spawn Point
        //==============================================================================================================

        //Get Position Point depending on number of enemy to spawn (must be pow2)
        public Vector3[] GetSpawnPoints(int numToSpawn, int separation)
        {
            int spawns = max(ceilpow2(numToSpawn),4);
            Vector3[] spawnPoints = new Vector3[spawns];
            
            int2 xyPos = SpawningChunkIndex.GetXY2(numChunkXY.x);
            
            Vector3 centerSpawnCell = new Vector3((xyPos.x * ChunkSize) + ChunkSize / 2f, 0, (xyPos.y * ChunkSize) + ChunkSize / 2f);

            int spawnSquareHalfExtent = spawns / 4;

            Vector3 topLeftSpawnZ = new Vector3
            (
                centerSpawnCell.x - spawnSquareHalfExtent,
                0,
                centerSpawnCell.z + spawnSquareHalfExtent
            );

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
        
        //1) Go through all the cells!
        //2) Set Basic Cost(1/255) use Short(255 may be to small depending on the terrain size)
        //2) Make Integration
        
        public void GetCostField()
        {
            int totalNumCells = gridSize.x * gridSize.y;
            /*
            for (int i = 0; i < totalNumCells; i++)
            {
                int chunkIndex = GetChunkIndex(i);
            }

            JCostField job = new JCostField
            {
                MapSize = gridSize,
                ChunkSize = this.ChunkSize,
                WalkableChunk = default,
                CostField = default
            };
            JobHandle jobHandle = job.ScheduleParallel(totalNumCells, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            */
        }

        /// <summary>
        /// CAREFUL FOR BLENDING
        /// NEED TO KNOW which cell is directly near an unwalkable chunk
        /// So we can set a value of 2 for the costfield
        /// </summary>
        public struct JCostField : IJobFor
        {
            [ReadOnly] public int2 MapSize;
            [ReadOnly] public int ChunkSize;
            [ReadOnly] public NativeArray<int> WalkableChunk;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<byte> CostField;
            public void Execute(int index)
            {
                int2 coord = index.GetXY2(MapSize.x);
                
                float2 currentCellCenter = coord + new float2(0.5f);
                
                int chunkIndex = GetChunkIndex(currentCellCenter);
                
                //Check if is in walkableChunk : if not => CostField[index] = Byte.MaxValue (0-255 is more than enough for cost field)
                
                /*
                if (index < 4096)
                {
                    Debug.Log($"at cell {index}; coord : {coord}; Chunk = {chunkIndex}");
                }
                */
                //CHECK IF TRUE! OK checked!

            }

            private int GetChunkIndex(float2 pointPos)
            {
                float2 percents = pointPos / (MapSize * ChunkSize);
                percents = clamp(percents, float2.zero, float2(1f));
                int2 xy =  clamp((int2)floor(MapSize * percents), 0, MapSize - 1);
                return mad(xy.y, MapSize.x/ChunkSize, xy.x);
            }
        }
        
        
        
        
#if UNITY_EDITOR   
        
//======================================================================================================================
//DEBUG PURPOSES
//======================================================================================================================

        /// <summary>
        /// Move the spawning area (Green Square)
        /// </summary>
        private void MoveSpawnArea()
        {
            int2 xyPos = SpawningChunkIndex.GetXY2(numChunkXY.x);
            
            Vector3 position = new Vector3(
                (xyPos.x * ChunkSize) + ChunkSize / 2f,
                0.05f,
                (xyPos.y * ChunkSize) + ChunkSize / 2f);
            Debug.Log(numChunkXY.x);

            SpawnArea.transform.position = position;
        }

        private void ShowHideSpawnArea(bool state) => SpawnArea.GetComponent<MeshRenderer>().enabled = state;



        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            if (!EnableDebugger) return;
            if (chunksPosition.Length == 0)
            {
                chunksPosition = new Vector3[totalChunk];
                InitializeChunkGrid();
            }
            
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            
            Vector3 cubeBounds = new Vector3(ChunkSize, 0, ChunkSize);
            for (int i = 0; i < totalChunk; i++)
            {
                Handles.DrawWireCube(chunksPosition[i], cubeBounds);
                Handles.Label(chunksPosition[i] + new Vector3(-(ChunkSize/4f),0,ChunkSize), i.GetXY2(numChunkXY.x).ToString(), style);
            }
        }
        
        private void InitializeChunkGrid()
        {
            int halfChunk = ChunkSize/2;
            for (int i = 0; i < totalChunk; i++)
            {
                int2 chunkCoord = i.GetXY2(numChunkXY.x);
                
                float chunkX = (chunkCoord.x * ChunkSize) + halfChunk;
                float chunkY = (chunkCoord.y * ChunkSize) + halfChunk;

                chunksPosition[i] = new Vector3(chunkX, 0, chunkY);
            }
        }
        
#endif
        
    }
}
