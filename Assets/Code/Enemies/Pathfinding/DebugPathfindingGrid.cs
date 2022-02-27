using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace TowerDefense
{
#if UNITY_EDITOR
    public partial class PathfindingGrid : MonoBehaviour
    {
        public bool EnableDebugger;
        int totalChunk => numChunkXY.x * numChunkXY.y;
        
        private Dictionary<int, int[]> grid;//DEBUG ONLY
        private Dictionary<int, byte[]> costGrid; //DEBUG ONLY
        
        private GridData debugGridData;
        
        private byte[] CostField; 
        
        

        //SPAWNING POINT
        [SerializeField] private GameObject SpawnArea;
        public bool ShowSpawnChunk;
        
        [HideInInspector]
        [SerializeField]private Vector3[] chunksPosition;

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
            if (directionChunkGrid == null) return;
            for (int i = 0; i < 4; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < directionChunkGrid[chunkIndex].Length; j++)
                {
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    KWUtils.Debug.DrawArrow.ForGizmo(cellPos, directionChunkGrid[chunkIndex][j]/2f);
                }
            }
        }
    }
#endif
}
