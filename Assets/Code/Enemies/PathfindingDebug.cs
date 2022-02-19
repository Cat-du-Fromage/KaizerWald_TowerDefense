using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
/*
namespace TowerDefense
{

#if UNITY_EDITOR
    [CustomEditor(typeof(PathfindingGrid))]
    public class PathfindingDebug : Editor
    {
        public bool EnableDebugger;
        [SerializeField] private TerrainData terrainData;
        [SerializeField] private PathfindingGrid PathGrid;

        private Vector3[] chunksPosition;

        private int2 gridSize;
        private int chunkSize;
        private int2 numChunkXY;

        private int totalChunk;
        private Vector3 cubeBounds;
        
        private void OnSceneGUI()
        {
            if (!EnableDebugger) return;
            
            PathfindingGrid linkedObject = target as PathfindingGrid;
            if (linkedObject == null || terrainData == null) return;

            if (chunksPosition.Length == 0)
            {
                gridSize   = (int2)terrainData.size.XZ();
                chunkSize  = math.ceilpow2(chunkSize);
                numChunkXY = (int2)(terrainData.size.XZ() / new int2(chunkSize));
                totalChunk = gridSize.x * gridSize.y;
                chunksPosition = new Vector3[gridSize.x * gridSize.y];
                InitializeChunkGrid();
                cubeBounds = new Vector3(chunkSize / 2, 1, chunkSize / 2);
            }
            for (int i = 0; i < totalChunk; i++)
            {
                Handles.DrawWireCube(chunksPosition[i], cubeBounds);
            }
        }

        private void OnValidate()
        {
            PathfindingGrid linkedObject = target as PathfindingGrid;
            if (linkedObject == null) return;
        }

        private void InitializeChunkGrid()
        {
            int halfChunk = chunkSize/2;
            for (int i = 0; i < totalChunk; i++)
            {
                //int chunkIndex = Mathf.FloorToInt((float)i / chunkSize);
                
                int2 chunkCoord = i.GetXY2(numChunkXY.x);
                
                float chunkX = (chunkCoord.x * chunkSize) + halfChunk;
                float chunkY = (chunkCoord.y * chunkSize) + halfChunk;

                chunksPosition[i] = new Vector3(chunkX, 0, chunkY);
            }
        }
        
        public override void OnInspectorGUI()
        {
            PathfindingGrid linkedObject = target as PathfindingGrid;
            DrawDefaultInspector();
            if(GUILayout.Button("Show Grid"))
            {
                terrainData ??= linkedObject.terrainData;
                gridSize   = (int2)terrainData.size.XZ();
                chunkSize  = math.ceilpow2(chunkSize);
                numChunkXY = (int2)(terrainData.size.XZ() / new int2(chunkSize));
                totalChunk = gridSize.x * gridSize.y;
                chunksPosition = new Vector3[gridSize.x * gridSize.y];
                InitializeChunkGrid();
                cubeBounds = new Vector3(chunkSize / 2, 1, chunkSize / 2);
                EnableDebugger = true;
            }
            
            if(GUILayout.Button("Hide Grid"))
            {
                EnableDebugger = false;
            }
        }
        
    }
#endif
    
}
*/