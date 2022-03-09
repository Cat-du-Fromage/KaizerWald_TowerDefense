using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Collections;
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
    public class BuildManager : MonoBehaviour, IGridHandler<bool, SimpleGrid<bool>>
    {
        public IGridSystem GridSystem { get; set; }
        public SimpleGrid<bool> Grid { get; private set; }
        public void InitGrid(int2 mapSize, int chunkSize, int cellSize = 1, Func<int2, SimpleGrid<bool>> providerFunction = null)
        {
            throw new NotImplementedException();
        }

        public bool IsBuilding;

        [SerializeField] private TurretManager TurretManager;
        [SerializeField] private Camera PlayerCamera;
        [SerializeField] private TerrainData terrainData;

        private GameObject currentTurret;
        private Transform currentBlueprint;
        
        //Need To Make it's own separate grid!
        private int2 terrainWidthHeight;
        
        private RaycastHit[] hits = new RaycastHit[1];
        private Ray ray;
        
        //Simple Generic Grid
        private const int CellSize = 4;

        private int currentGridIndex = -1;
        private int previousGridIndex = -1;

        private void Awake()
        {
            TurretManager ??= FindObjectOfType<TurretManager>();
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;
            terrainWidthHeight = new int2((int) terrainData.size.x, (int) terrainData.size.z);
        }
        
        private void Start()
        {
            Grid = new SimpleGrid<bool>(terrainWidthHeight, CellSize);
        }

        private void Update()
        {
            if (!IsBuilding) return;
            
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
            if (Grid[currentGridIndex] == false)
            {
                //Move this to Register Notification
                TurretManager.CreateTurret(currentTurret, currentBlueprint.position, currentBlueprint.rotation);
                Grid[currentGridIndex] = true; //NOT THE RIGHT PLACE
                GridSystem.OnGridChange(GridType.Turret, currentGridIndex);
            }
        }
        
        private void SnapBlueprintToGrid()
        {
            previousGridIndex = currentGridIndex;//Avoid unnecessary computation
            ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            if (RaycastNonAlloc(ray.origin, ray.direction, hits,INFINITY, TerrainLayerMask) != 0)
            {
                currentGridIndex = hits[0].point.GetIndexFromPosition(terrainWidthHeight, CellSize);
                if (currentGridIndex == previousGridIndex) return;//though it comes late in the process...
                currentBlueprint.position = currentBlueprint.position.FlatMove(Grid.GetCenterCellAt(currentGridIndex));
            }
        }

        
        private void OnDrawGizmos()
        {
            if (!IsBuilding) return;
            
            Gizmos.color = Color.green;
            int numCellIteration = KWmath.cmul(terrainWidthHeight / CellSize);
            Vector3 cellBounds = new Vector3(CellSize, 0.05f, CellSize);
            for (int i = 0; i < numCellIteration; i++)
            {
                Gizmos.DrawWireCube(Grid.GetCenterCellAt(i), cellBounds);
            }
        }
/*
        private void DebugChunkBitField()
        {
            if (chunkedBitfieldGrid == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            Vector3 position;
            bool value;
            for (int i = 0; i < chunkedBitfieldGrid.ChunkLength; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    position = chunkedBitfieldGrid.GetCellCenterFromChunkIndexAt(i, j);
                    
                    value = chunkedBitfieldGrid.GetChunkCellValueAt(i,j);
                    Handles.Label(position, value.ToString(), style);
                }
            }
        }
        */
    }
}
