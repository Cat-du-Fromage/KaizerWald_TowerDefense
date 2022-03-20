using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Collections;
using Unity.Mathematics;
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
        private const int CellSize = 4;

        [SerializeField] private BlueprintColorChange BlueprintColor;
        
        [SerializeField] private Camera PlayerCamera;
        [SerializeField] private StaticEntitiesGrid GridHandler;
        [SerializeField] private TurretManager TurretManager;
        [SerializeField] private AStarGrid AStarGrid;
        private UIBuilding uiBuilding;
        
        private int2 MapXY;
        
        //Cache Active Blueprint we are working in
        private GameObject currentTurret;
        private Transform currentBlueprint;
        
        //Use to get turret Position by raycast
        private Ray ray;
        private RaycastHit[] hits = new RaycastHit[1];
        
        //Use to avoid unnecessary calculation
        private int currentGridIndex = -1;
        private int previousGridIndex = -1;
        
        //Use to Toggle(On/Off) Build Mode
        public bool IsBuilding { get; private set; }

        private void Awake()
        {
            BlueprintColor = FindObjectOfType<BlueprintColorChange>();
            
            uiBuilding = FindObjectOfType<UIBuilding>();
            GridHandler = FindObjectOfType<StaticEntitiesGrid>();
            TurretManager ??= FindObjectOfType<TurretManager>();
            AStarGrid = FindObjectOfType<AStarGrid>();
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;
        }

        public void Start()
        {
            MapXY = GridHandler.Grid.GridData.MapSize;
            uiBuilding.OnBuildCommand += ToggleBuildModeOn;
        }

        private void OnDestroy()
        {
            if(uiBuilding != null)
                uiBuilding.OnBuildCommand -= ToggleBuildModeOn;
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

        /// <summary>
        /// CALLBACK TO OTHER SYSTEMS
        /// </summary>
        private void OnCreateTurret()
        {
            if (GridHandler.Grid[currentGridIndex] == false)
            {
                BlueprintColor.OnBusyTile();
                //Move this to Register Notification
                TurretManager.CreateTurret(currentTurret, currentBlueprint.position, currentBlueprint.rotation);
                GridHandler.Grid[currentGridIndex] = true;
            }
        }

        private void UpdateBluePrintColor()
        {
            if (GridHandler.Grid[currentGridIndex] == false)
            {
                BlueprintColor.OnFreeTile();
                return;
            }
            BlueprintColor.OnBusyTile();
        }
        
        private void SnapBlueprintToGrid()
        {
            previousGridIndex = currentGridIndex;//Avoid unnecessary computation
            ray = PlayerCamera.ScreenPointToRay(GetMousePosition);
            if (RaycastNonAlloc(ray.origin, ray.direction, hits,INFINITY, TerrainLayerMask) != 0)
            {
                currentGridIndex = hits[0].point.GetIndexFromPosition(MapXY, CellSize);
                if (currentGridIndex == previousGridIndex) return;//though it comes late in the process...
                SetBlueprintPosition();
                UpdateBluePrintColor();
                
                //Check if cell don't block
                if (AStarGrid.IsPathAffected(currentGridIndex, GridHandler.Grid.GridData))
                {
                    BlueprintColor.OnBusyTile();
                }
                
                //Notify Astar (index in the Grid + in GridData)
                //1) Need to get Array corresponding to "fakeChunk"
                //AStarGrid.OnBuildCursorMove(currentGridIndex, GridHandler.Grid.GridData);
            }
            
            void SetBlueprintPosition()
            {
                Vector3 blueprintPosition = currentBlueprint.position;
                Vector3 positionInGrid = GridHandler.Grid.GetCellCenter(currentGridIndex);
                currentBlueprint.position = blueprintPosition.FlatMove(positionInGrid);
            }
        }

        
        private void OnDrawGizmos()
        {
            /*
            if (!IsBuilding) return;
            
            Gizmos.color = Color.green;
            int numCellIteration = cmul(MapXY / CellSize);
            Vector3 cellBounds = new Vector3(CellSize, 0.05f, CellSize);
            for (int i = 0; i < numCellIteration; i++)
            {
                Gizmos.DrawWireCube(i.GetCellCenterFromIndex(MapXY, CellSize), cellBounds);
            }
            */
        }
    }
}
