using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

using UnityEngine.InputSystem;
using static TowerDefense.TowerDefenseUtils;
using static KWUtils.KWGrid;
using static KWUtils.InputSystemExtension;

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

        public Transform token;
        [SerializeField] private Camera PlayerCamera;
        [SerializeField] private TerrainData terrainData;
        //bounds = half extends
        //size = width/height and ???? 600 from what? apparently something about vertices (number decrease/increase)

        private const int CellSize = 2;

        public int2 widthHeight;
        public int2 numCellWidthHeight;

        public int TotalNumCells;
        
        private Vector3[] cellsCenter;

        private void Awake()
        {
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;
            widthHeight = new int2((int) terrainData.size.x, (int) terrainData.size.z);
            numCellWidthHeight = new int2(widthHeight >> 1); // == /2
            TotalNumCells = numCellWidthHeight.x * numCellWidthHeight.y;
            
            Debug.Log($"width {numCellWidthHeight.x}; height {numCellWidthHeight.y}");
        }

        private void Start()
        {
            cellsCenter = new Vector3[TotalNumCells];
            GetCellPosition();
        }

        private void Update()
        {
            if (!Mouse.current.leftButton.isPressed) return;
            SnapTowerToGrid();
        }

        public void SnapTowerToGrid()
        {
            Ray ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            bool hitTerrain = Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, TerrainLayerMask);
            if (!hitTerrain) return;
            
            int indexPos = hit.point.GetIndexFromPosition(numCellWidthHeight, CellSize);
            token.position = token.position.GridHMove(cellsCenter[indexPos]);
        }

        /// <summary>
        /// GRID : Construction
        /// </summary>
        public void GetCellPosition()
        {
            Vector3 cellBounds = new Vector3(CellSize,0,CellSize);
            Vector3 centerCellOffset = new Vector3(CellSize / 2f, 0, CellSize / 2f);
            
            for (int index = 0; index < TotalNumCells; index++)
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
                Vector3 centerCellOffset = new Vector3(CellSize, CellSize/5f, CellSize);
                Gizmos.color = Color.green;
                
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                int numIteration = numCellWidthHeight.x * 2;
                
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(cellsCenter[i], centerCellOffset);
                    Handles.Label(cellsCenter[i], i.ToString(), style);
                    //Gizmos.DrawWireSphere(cellsCenter[i], 0.5f);
                }
            }
        }
    }
}
