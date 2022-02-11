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
        private Vector3[] snapPositions;
        
        private Vector3[] visualBuildPositions;

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
            snapPositions = new Vector3[(widthHeight.x - 2) * (widthHeight.y - 2)];
            visualBuildPositions = new Vector3[cellsCenter.Length * 4];
            GetCellPosition();
            GetSnapPosition();
            GetVisualBuildPosition();
        }

        private void Update()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            SnapTowerToGrid();
        }

        public void ToggleBlueprint(bool state)
        {
            token.gameObject.SetActive(state);
            if (state)
            {
                
            }
        }

        
        /// <summary>
        /// 1/2 BIG CELLS
        /// Get intdex from Inner Grid! => bigCell => divid in 4 cells
        /// imagine a square of length one with each intersection of smalls square as center
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private int SnapGridBounds(Vector3 point)
        {
            Vector3 clampPoint = new Vector3(point.x - 0.5f, 0, point.z - 0.5f);
            
            //float clampAxisX = Mathf.Clamp(clampPoint.x, 0.5f, (float)widthHeight.x - 0.5f);
            //float clampAxisZ = Mathf.Clamp(clampPoint.z, 0.5f, (float)widthHeight.y - 0.5f);
            //clampPoint.Set(clampAxisX,0,clampAxisZ);
            
            int indexPos = clampPoint.GetIndexFromPosition(widthHeight-(new int2(1)), 1);
            //Debug.Log($"indexPos = {indexPos}");
            return indexPos;
        }

        public void SnapTowerToGrid()
        {
            Ray ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            bool hitTerrain = Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, TerrainLayerMask);
            if (!hitTerrain) return;
            //Vector3 pointOffset = hit.point + Vector3.one.Flat();
            
            //int indexPos = hit.point.GetIndexFromPosition(widthHeight, 1);
            int indexPos = SnapGridBounds(hit.point);
            token.position = token.position.GridHMove(snapPositions[indexPos]);
        }

        /// <summary>
        /// BIG CELLS
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

        /// <summary>
        /// Get Snapping grid where turret can be placed
        /// </summary>
        public void GetSnapPosition()
        {
            Vector3 flatVectorOne = Vector3.one.Flat();
            
            for (int index = 0; index < snapPositions.Length; index++)
            {
                (int x, int z) = index.GetXY(widthHeight.x-1);
                
                Vector3 cellPositionOnMesh = new Vector3(x, 0, z);
                //Vector3 pointPosition = Vector3.Scale(cellPositionOnMesh,Vector3.one) + flatVectorOne;
                Vector3 pointPosition = cellPositionOnMesh + flatVectorOne; //* cellBound not needed this time since value = Vector3(1,1,1)
                snapPositions[index] = pointPosition;
            }
        }

        /// <summary>
        /// 1/4 BIG CELLS
        /// </summary>
        public void GetVisualBuildPosition()
        {
            Vector3 flatOffset = Vector3.one.Flat() * 0.5f;
            for (int i = 0; i < visualBuildPositions.Length; i++)
            {
                (int x, int z) = i.GetXY(widthHeight.x);
                Vector3 cellPositionOnMesh = new Vector3(x, 0, z);
                Vector3 pointPosition = cellPositionOnMesh + flatOffset;
                visualBuildPositions[i] = pointPosition;
            }
        }

        private void OnDrawGizmos()
        {
            if (GUIDebug)
            {
                Vector3 centerCellOffset = new Vector3(CellSize, CellSize/5f, CellSize);
                
                Vector3 centerSnapOffset = new Vector3(1, 1/4f, 1);
                
                Gizmos.color = Color.green;
                
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                int numIteration = numCellWidthHeight.x * 4;
                
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(cellsCenter[i], centerCellOffset);
                    Handles.Label(cellsCenter[i], i.ToString(), style);
                    Gizmos.DrawWireSphere(snapPositions[i], 0.25f);
                }
                
                Gizmos.color = Color.red;
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(snapPositions[i], centerSnapOffset);
                }
                
                Gizmos.color = Color.cyan;
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(visualBuildPositions[i], centerSnapOffset);
                }
            }
        }
    }
}
