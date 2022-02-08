using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.InputSystem;
using static KWUtils.KWGrid;

namespace TowerDefense
{
    public class Cell
    {
        private int size;
        private Vector3 position;
        private Vector2Int positionInGrid;
    }
    
    public class SandBox_GridManager : MonoBehaviour
    {
        public bool GUIDebug;

        public GameObject token;
        
        [SerializeField] private TerrainData terrainData;
        //bounds = half extends
        //size = width/height and ???? 600 from what? apparently something about vertices (number decrease/increase)

        private const int cellSize = 2;

        public int2 widthHeight;

        public int2 numCellWidthHeight;

        public int TotalNumCells;
        
        private int[] grid;
        private Vector3[] cellsCenter;

        private void Awake()
        {
            widthHeight = new int2((int) terrainData.size.x, (int) terrainData.size.z);
            numCellWidthHeight = new int2(widthHeight >> 1); // == /2

            TotalNumCells = numCellWidthHeight.x * numCellWidthHeight.y;
            
            grid = new int[TotalNumCells];
            cellsCenter = new Vector3[TotalNumCells];
            Debug.Log($"width {numCellWidthHeight.x}; height {numCellWidthHeight.y}");
        }

        private void Start()
        {
            GetCellPosition();
        }

        private void Update()
        {
            if (!Mouse.current.rightButton.wasPressedThisFrame) return;

            //int index = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).GetIndexFromPosition(widthHeight, cellSize);
            int indexPos = token.transform.position.GetIndexFromPosition(widthHeight, cellSize);
            Debug.Log($"current {indexPos}");
        }

        /// <summary>
        /// GRID : Construction
        /// </summary>
        public void GetCellPosition()
        {
            Vector3 cellBounds = new Vector3(cellSize,0,cellSize);
            Vector3 centerCellOffset = new Vector3(cellSize >> 1, 0, cellSize >> 1);
            
            for (int index = 0; index < grid.Length; index++)
            {
                (int x, int z) = index.GetXY(numCellWidthHeight.x);
                
                Vector3 cellPositionOnMesh = new Vector3(x, 0, z);
                Vector3 pointPosition = Vector3.Scale(cellPositionOnMesh,cellBounds) + centerCellOffset;
                
                cellsCenter[index] = pointPosition;
            }
        }

        private void OnDrawGizmos()
        {
            if (GUIDebug)
            {
                Vector3 centerCellOffset = new Vector3(cellSize, cellSize, cellSize);
                Gizmos.color = Color.green;
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;

                int numIteration = numCellWidthHeight.x * 2;
                
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(cellsCenter[i], centerCellOffset);
                    Handles.Label(cellsCenter[i], i.ToString(), style);
                }
            }
        }
    }
}
