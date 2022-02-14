using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;
using static TowerDefense.TowerDefenseUtils;
using static KWUtils.KWGrid;
using static KWUtils.InputSystemExtension;

namespace TowerDefense
{
    public class BuildManager : MonoBehaviour
    {
        public bool IsBuilding;
        
        //Blueprint has the reference of their turret prefab?
        [SerializeField] private GameObject[] TurretsBlueprint;
        
        //[SerializeField] private GameObject[] TurretsPrefab;
        
        [SerializeField] private Camera PlayerCamera;
        [SerializeField] private TerrainData terrainData;

        private Transform currentBlueprint;
        
        private int2 terrainWidthHeight;
        private Vector3[] snapPositions;
        
        private void Awake()
        {
            for (int i = 0; i < TurretsBlueprint.Length; i++) TurretsBlueprint[i].SetActive(false);
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;
            terrainWidthHeight = new int2((int) terrainData.size.x, (int) terrainData.size.z);
        }
        
        private void Start()
        {
            snapPositions = new Vector3[(terrainWidthHeight.x - 1) * (terrainWidthHeight.y - 1)];
            GetSnapPosition();
        }
        
        /// <summary>
        /// Will activate "construction mode"
        /// Must deactivate all blueprint except the one selected
        /// </summary>
        /// <param name="turretBlueprint"></param>
        public void ToggleBlueprint(GameObject turretBlueprint)
        {
            IsBuilding = !IsBuilding;
            for (int i = 0; i < TurretsBlueprint.Length; i++)
            {
                if (TurretsBlueprint[i] != turretBlueprint)
                {
                    TurretsBlueprint[i].SetActive(false);
                    continue;
                }
                TurretsBlueprint[i].SetActive(true);
                currentBlueprint = TurretsBlueprint[i].transform;
            }
        }

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
        /// <param name="point"></param>
        /// <returns></returns>
        private int SnapGridBounds(Vector3 point)
        {
            Vector3 clampPoint = new Vector3(point.x - 0.5f, 0, point.z - 0.5f);
            int indexPos = clampPoint.GetIndexFromPosition(terrainWidthHeight - new int2(1), 1);
            return indexPos;
        }
    }
}
