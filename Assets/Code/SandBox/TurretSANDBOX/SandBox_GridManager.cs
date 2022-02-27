using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.EventSystems;
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
        public bool IsBuilding;
        public Transform TurretBlueprint;
        [SerializeField] private GameObject TurretPrefab;
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
        
        private RaycastHit[] Hits = new RaycastHit[1];

        private Keyboard keyboard;

        private void Awake()
        {
            TurretBlueprint.gameObject.SetActive(false);
            
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;
            widthHeight = new int2((int) terrainData.size.x, (int) terrainData.size.z);
            numCellWidthHeight = new int2(widthHeight >> 1); // == /2
            TotalNumCells = numCellWidthHeight.x * numCellWidthHeight.y;
        }

        private void Start()
        {
            keyboard = Keyboard.current;
            
            cellsCenter = new Vector3[TotalNumCells];
            snapPositions = new Vector3[(widthHeight.x - 1) * (widthHeight.y - 1)];
            visualBuildPositions = new Vector3[cellsCenter.Length * 4];
            //GetCellPosition();
            GetSnapPosition();
            GetVisualBuildPosition();
        }

        private void Update()
        {
            if (!IsBuilding) return;
            SnapTowerToGrid();

            //Rotate Turret
            if (keyboard.rKey.wasPressedThisFrame)
            {
                TurretBlueprint.rotation *= Quaternion.Euler(0, 90, 0);
            }
            
            //Exit Build Mode
            if (keyboard.escapeKey.wasReleasedThisFrame || keyboard.qKey.wasReleasedThisFrame)
            {
                ToggleBlueprint();
            }

            //so we dont create a turret when clicking on a build icon
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            //Build Turret
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Instantiate(TurretPrefab, TurretBlueprint.position.SetAxis(Axis.Y, -1), TurretBlueprint.rotation);
            }
        }

        //FROM USE CASE
        //Create Turret Blueprint => true
        //Destroy Turret Blueprint => false
        public void ToggleBlueprint()
        {
            IsBuilding = !IsBuilding;
            TurretBlueprint.gameObject.SetActive(IsBuilding);
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
            int indexPos = clampPoint.GetIndexFromPosition(widthHeight-(new int2(1)), 1);
            return indexPos;
        }

        public void SnapTowerToGrid()
        {
            Ray ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            
            //bool hitTerrain = Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, TerrainLayerMask);
            //Debug.Log(hitTerrain);
            //for some reason basic raycast can't go through turrets
            int numHits = Physics.RaycastNonAlloc(ray.origin, ray.direction, Hits,Mathf.Infinity, TerrainLayerMask);
            if(numHits != 0)
            {
                int indexPos = SnapGridBounds(Hits[0].point);
                TurretBlueprint.position = TurretBlueprint.position.FlatMove(snapPositions[indexPos]);
            }
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
            Vector3 cellOffset = Vector3.one.Flat();
            
            for (int index = 0; index < snapPositions.Length; index++)
            {
                (int x, int z) = index.GetXY(widthHeight.x-1); //we offset by 1,0,1 so we just remove the last one
                
                Vector3 cellPositionOnMesh = new Vector3(x, 0, z);
                //Vector3 pointPosition = Vector3.Scale(cellPositionOnMesh,Vector3.one) + flatVectorOne;
                Vector3 pointPosition = cellPositionOnMesh + cellOffset; //* cellBound not needed this time since value = Vector3(1,1,1)
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
                    //Gizmos.DrawWireCube(cellsCenter[i], centerCellOffset);
                    Handles.Label(cellsCenter[i], i.ToString(), style);
                    Gizmos.DrawWireSphere(snapPositions[i], 0.25f);
                }
                /*
                Gizmos.color = Color.red;
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(snapPositions[i], centerSnapOffset);
                }
                */
                Gizmos.color = Color.cyan;
                for (int i = 0; i < numIteration; i++)
                {
                    Gizmos.DrawWireCube(visualBuildPositions[i], centerSnapOffset);
                }
            }
        }
    }
}
