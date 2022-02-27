using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

using static TowerDefense.TowerDefenseUtils;
using static KWUtils.KWGrid;
using static KWUtils.InputSystemExtension;
using static KWUtils.KWmath;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;

namespace TowerDefense
{
    public class BuildManager : MonoBehaviour
    {
        public bool IsBuilding;

        [SerializeField] private TurretManager TurretManager;
        [SerializeField] private Camera PlayerCamera;
        [SerializeField] private TerrainData terrainData;

        private GameObject currentTurret;
        private Transform currentBlueprint;
        
        //Need To Make it's own separate grid!
        private int2 terrainWidthHeight;
        //private Vector3[] snapPositions;
        
        private RaycastHit[] hits = new RaycastHit[1];
        private Ray ray;
        
        //Simple Generic Grid
        private const int CellSize = 4;
        private SimpleGrid<bool> simpleGrid; //use instanceID
        private int currentGridIndex = -1;
        private void Awake()
        {
            TurretManager ??= FindObjectOfType<TurretManager>();
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;
            terrainWidthHeight = new int2((int) terrainData.size.x, (int) terrainData.size.z);
        }
        
        private void Start()
        {
            //snapPositions = new Vector3[(terrainWidthHeight.x - 1) * (terrainWidthHeight.y - 1)];
            //GetSnapPosition();
            simpleGrid = new SimpleGrid<bool>(terrainWidthHeight, CellSize);
        }

        private void Update()
        {
            if (!IsBuilding) return;
            
            //SnapTowerToGrid();
            SnapBlueprintToGrid();
            if (currentGridIndex == -1) return;
            
            //Rotate Turret
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                OnBlueprintRotation();
            }
            
            //so we dont create a turret when clicking on a build icon
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            //Build Turret
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnCreateTurret();
            }
        }

        /// <summary>
        /// Will activate "construction mode"
        /// Must deactivate all blueprint except the one selected
        /// </summary>
        /// <param name="turretBlueprint"></param>
        public void ToggleBuildModeOn(GameObject turretBlueprint)
        {
            IsBuilding = true;
            currentBlueprint = turretBlueprint.transform;
            currentTurret = currentBlueprint.GetComponent<BlueprintComponent>().GetTurretPrefab;
        }
        public void ToggleBuildModeOff()
        {
            IsBuilding = false;
            currentGridIndex = -1;
        }

        private void OnBlueprintRotation() => currentBlueprint.rotation *= Quaternion.Euler(0, 90, 0);

        private void OnCreateTurret()
        {
            if (simpleGrid.GetValueAt(currentGridIndex) == false)
            {
                //Move this to Register Notification
                TurretManager.CreateTurret(currentTurret, currentBlueprint.position, currentBlueprint.rotation);
                simpleGrid.SetValue(currentGridIndex, true);
            }
        }
        
//======================================================================================================================
//TEST SIMPLE GRID
//======================================================================================================================

        private void SnapBlueprintToGrid()
        {
            ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            if (RaycastNonAlloc(ray.origin, ray.direction, hits,INFINITY, TerrainLayerMask) != 0)
            {
                currentGridIndex = hits[0].point.GetIndexFromPosition(terrainWidthHeight, CellSize);
                currentBlueprint.position = currentBlueprint.position.FlatMove(simpleGrid.GetCenterCellAt(currentGridIndex));
            }
        }


//======================================================================================================================
//TEST SIMPLE GRID
//======================================================================================================================
/*
        /// <summary>
        /// Get Snapping grid where turret can be placed
        /// </summary>
        private void GetSnapPosition()
        {
            Vector3 cellOffset = Vector3.one.Flat();
            
            for (int index = 0; index < snapPositions.Length; index++)
            {
                (int x, int z) = index.GetXY(terrainWidthHeight.x-1); //we offset by 1,0,1 so we just remove the last one
                
                Vector3 cellPositionOnMesh = new Vector3(x, 0, z);
                Vector3 pointPosition = cellPositionOnMesh + cellOffset; //* cellBound not needed this time since value = Vector3(1,1,1)
                snapPositions[index] = pointPosition;
            }
        }
        
        /// <summary>
        /// 1/2 BIG CELLS
        /// Get index from Inner Grid! => bigCell => divide in 4 cells
        /// imagine a square of length one with each intersection of smalls square as center
        /// </summary>
        private int SnapGridBounds(Vector3 point)
        {
            Vector3 clampPoint = new Vector3(point.x - 0.5f, 0, point.z - 0.5f);
            int indexPos = clampPoint.GetIndexFromPosition(terrainWidthHeight - new int2(1), 1);
            return indexPos;
        }
        
        private void SnapTowerToGrid()
        {
            ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            
            //for some reason basic raycast can't go through turrets
            int numHits = RaycastNonAlloc(ray.origin, ray.direction, hits,Mathf.Infinity, TerrainLayerMask);
            if(numHits != 0)
            {
                int indexPos = SnapGridBounds(hits[0].point);
                currentBlueprint.position = currentBlueprint.position.FlatMove(snapPositions[indexPos]);
            }
        }
*/
        private void OnDrawGizmos()
        {
            if (!IsBuilding) return;
            
            Gizmos.color = Color.green;
            int numCellIteration = KWmath.cmul(terrainWidthHeight / CellSize);
            Vector3 cellBounds = new Vector3(CellSize, 0.05f, CellSize);
            for (int i = 0; i < numCellIteration; i++)
            {
                Gizmos.DrawWireCube(simpleGrid.GetCenterCellAt(i), cellBounds);
            }
        }
        
    }
}
