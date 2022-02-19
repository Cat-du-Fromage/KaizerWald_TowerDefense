using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.VirtualTexturing;
#endif

namespace TowerDefense
{
    public class PathfindingGrid : MonoBehaviour
    {
        public bool EnableDebugger;
        
        public TerrainData terrainData;
        [SerializeField] private int ChunkSize = 16;
        
        private Dictionary<int, int[]> grid;
        
        private int2 gridSize;
        private int2 numChunkXY;
        
        private int totalChunk;
        
#if UNITY_EDITOR
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
        }
        
        private void Awake()
        {
            InitializeFields();
            Debug.Log($"num chunk = {numChunkXY}");
        }

        private void InitializeFields()
        {
            gridSize = (int2)terrainData.size.XZ();
            ChunkSize = math.ceilpow2(ChunkSize);
            numChunkXY = (int2)(terrainData.size.XZ() / new int2(ChunkSize));
            totalChunk = numChunkXY.x * numChunkXY.y;
        }

        public void GetCostField()
        {
            int totalNumCells = gridSize.x * gridSize.y;
            for (int i = 0; i < totalNumCells; i++)
            {
                int chunkIndex = GetChunkIndex(i);
            }
        }
        
#if UNITY_EDITOR
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
                //int chunkIndex = Mathf.FloorToInt((float)i / chunkSize);
                
                int2 chunkCoord = i.GetXY2(numChunkXY.x);
                
                float chunkX = (chunkCoord.x * ChunkSize) + halfChunk;
                float chunkY = (chunkCoord.y * ChunkSize) + halfChunk;

                chunksPosition[i] = new Vector3(chunkX, 0, chunkY);
            }
        }
        
#endif
        
    }
}
