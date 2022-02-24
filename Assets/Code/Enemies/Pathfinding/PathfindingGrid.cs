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
    public class PathfindingGrid : MonoBehaviour
    {
        public bool EnableDebugger;
        [SerializeField] private int SpawningChunkIndex;
        [SerializeField] private TerrainData terrainData;
        [SerializeField] private int ChunkSize = 16;

        private Dictionary<int, Vector3[]> directionGrid;

        private int2 gridSize;
        private int2 numChunkXY;
        
        private int totalChunk;

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
        
//======================================================================================================================
//DEBUG PURPOSES
//======================================================================================================================
        private Dictionary<int, int[]> grid;//DEBUG ONLY
        private Dictionary<int, byte[]> costGrid; //DEBUG ONLY
        
        private GridData debugGridData;
        
        public byte[] CostField; 
        [SerializeField] private GameObject walkableChunkPrefab;
        private GameObject[] walkableChunkObj;
#if UNITY_EDITOR   
        //SPAWNING POINT
        [SerializeField] private GameObject SpawnArea;
        public bool ShowSpawnChunk;
        
        [HideInInspector]
        [SerializeField]private Vector3[] chunksPosition;
#endif
//======================================================================================================================
//DEBUG PURPOSES
//======================================================================================================================


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
            //gridData = new GridData(ChunkSize, gridSize);
            
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
            totalChunk = numChunkXY.x * numChunkXY.y;
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
            directionGrid = KWChunk.GetCellDirectionOrderedByChunk(flowField.GetFlowField(destinationGridCell, walkableChunk, walkableRoad), gridData);
            //grid = KWChunk.GetCellIndicesOrderedByChunk(flowField.BestCostField, gridData);
            //CostField = flowField.CostField;

            //costGrid = KWChunk.GetCellCostOrderedByChunk(CostField, gridData);
        }



//======================================================================================================================
//DEBUG PURPOSES
//======================================================================================================================
#if UNITY_EDITOR
        private Vector3 GetPosition(int2 xyPos) => new Vector3((xyPos.x * ChunkSize) + ChunkSize / 2f, 0.05f, (xyPos.y * ChunkSize) + ChunkSize / 2f);
        private void ShowWalkableArea()
        {
            for (int i = 0; i < walkableChunk.Length; i++)
            {
                int2 xyPos = walkableChunk[i].GetXY2(numChunkXY.x);
                Vector3 position = GetPosition(xyPos);
                walkableChunkObj[i] = Instantiate(walkableChunkPrefab, position, Quaternion.identity);
                walkableChunkObj[i].name = $"Chunk_Id_{walkableChunk[i]}_Coord_({xyPos.x},{xyPos.y})";
            }
            
        }

        /// <summary>
        /// Move the spawning area (Green Square)
        /// </summary>
        private void MoveSpawnArea() => SpawnArea.transform.position = GetPosition(SpawningChunkIndex.GetXY2(numChunkXY.x));
        private void ShowHideSpawnArea(bool state) => SpawnArea.GetComponent<MeshRenderer>().enabled = state;



        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            if (!EnableDebugger) return;
            debugGridData = new GridData(ChunkSize, gridSize);
            if (chunksPosition.Length == 0)
            {
                chunksPosition = new Vector3[totalChunk];
                InitializeChunkGrid();
            }
            
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            //DisplayChunkGrid(style);

            //CostDebug(style, true);
            //BestCostDebug(style);

            FlowFieldDebug(true);

            //DisplayDestination();
        }

        private void DisplayChunkGrid(GUIStyle style)
        {
            Vector3 cubeBounds = new Vector3(ChunkSize, 0, ChunkSize);
            for (int i = 0; i < totalChunk; i++)
            {
                Handles.DrawWireCube(chunksPosition[i], cubeBounds);
                Handles.Label(chunksPosition[i] + new Vector3(-(ChunkSize/4f),0,ChunkSize), i.GetXY2(numChunkXY.x).ToString(), style);
            }
        }

        private void DisplayDestination()
        {
            if (destinationGridCell == -1) return;
            int2 xy = destinationGridCell.GetXY2(gridSize.x);
            Vector3 destPos = new Vector3(xy.x + 0.5f, 0, xy.y + 0.5f);
            Handles.DrawWireCube(destPos, Vector3.one);
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

        private void BestCostDebug(GUIStyle style, bool walkableOnly)
        {
            if (grid == null) return;
            for (int i = 0; i < 2; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < grid[chunkIndex].Length; j++)
                {
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    //Gizmos.DrawWireCube(cellPos, Vector3.one);
                    Handles.Label(cellPos, grid[chunkIndex][j].ToString(), style);
                }
            }
        }
        
        private void CostDebug(GUIStyle style, bool walkableOnly)
        {
            if (costGrid == null) return;
            for (int i = 4; i < 9; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < costGrid[chunkIndex].Length; j++)
                {
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    Handles.Label(cellPos, costGrid[chunkIndex][j].ToString(), style);
                }
            }
        }

        private void FlowFieldDebug(bool walkableOnly)
        {
            if (directionGrid == null) return;
            for (int i = 0; i < 4; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < directionGrid[chunkIndex].Length; j++)
                {
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    KWUtils.Debug.DrawArrow.ForGizmo(cellPos, directionGrid[chunkIndex][j]/2f);
                }
            }

        }
        
#endif
        
    }
}
