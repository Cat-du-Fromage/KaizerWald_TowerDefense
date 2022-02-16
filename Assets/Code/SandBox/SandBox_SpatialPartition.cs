using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public interface ISpatialEntity
    {
        public uint EntityIndex { get; set; }
        public uint CurrentCellIndex { get; set; }
        public uint PreviousCellIndex { get; set; }
        public Vector3 Position { get; set; }
    }
    
    public struct SpCell
    {
        private uint index;
        public List<uint> entitiesId;

        public SpCell(uint id)
        {
            index = id;
            entitiesId = new List<uint>(4);
        }
    }

    public class SandBox_SpatialPartition : MonoBehaviour
    {
        [SerializeField] private bool DrawDebug = false;
        [SerializeField] private TerrainData terrainData;
        
        private int cellSize = 2;

        private int2 mapWidthHeight;
        
        private int numCellX;
        private int numCellY;
        
        private SpCell[] cells;

        private Vector3[] DebugCellPosition;
        
        private int TotalNumCell => numCellX * numCellY;

        private void Awake()
        {
            mapWidthHeight = new int2((int)terrainData.size.x, (int)terrainData.size.z);
            numCellX = mapWidthHeight.x / cellSize;
            numCellY = mapWidthHeight.y / cellSize;
            InitializeCells();
        }

        private void InitializeCells()
        {
            cells = new SpCell[TotalNumCell];
            for (uint i = 0; i < cells.Length; i++)
            {
                cells[i] = new SpCell(i);
            }
        }

        public int GetCellIndexFromPosition(Vector3 pointPos)
        {
            float percentX = pointPos.x / (mapWidthHeight.x * cellSize); //CHECK may no need to mak numcell * cellSize
            float percentY = pointPos.z / (mapWidthHeight.y * cellSize);

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);
            
            int x = Mathf.Clamp(Mathf.FloorToInt(mapWidthHeight.x * percentX), 0, mapWidthHeight.x-1);
            int y = Mathf.Clamp(Mathf.FloorToInt(mapWidthHeight.y * percentY), 0, mapWidthHeight.y-1);

            return (y * numCellX) + x;
        }
        
        //======================================
        //NOTIFICATION
        //======================================

        //Enemy
        //=====================
        
        //Very first spawn we don't have a "previous index"
        public void OnNotifyFirstSpawn(ISpatialEntity entity)
        {
            //Spawn Enemy
            cells[entity.CurrentCellIndex].entitiesId.Add(entity.EntityIndex);
        }
        
        public void OnNotifyUpdate(ISpatialEntity entity)
        {
            //Spawn Enemy
            cells[entity.PreviousCellIndex].entitiesId.Remove(entity.EntityIndex);
            cells[entity.CurrentCellIndex].entitiesId.Add(entity.EntityIndex);
            
            int index = GetCellIndexFromPosition(entity.Position);
            //cells[index] = 
        }
        
//======================================================================================================================
//DEBUG
//======================================================================================================================
        private void GetMouseIndexInGrid()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = new RaycastHit[1];
            //for some reason basic raycast can't go through turrets
            int numHits = Physics.RaycastNonAlloc(ray.origin, ray.direction, hits,Mathf.Infinity, 1<<8);
            if(numHits != 0)
            {
                int index = GetCellIndexFromPosition(hits[0].point);
                Debug.Log(index);
            }
        }
        
        private void GetCellCenter()
        {
            DebugCellPosition = new Vector3[TotalNumCell];
            Vector3 cellBounds = new Vector3(cellSize,0,cellSize);
            Vector3 centerCellOffset = new Vector3(cellSize / 2f, 0, cellSize / 2f);
            
            for (int index = 0; index < DebugCellPosition.Length; index++)
            {
                (int x, int z) = index.GetXY(numCellX);
                
                Vector3 cellPositionOnMesh = new Vector3(x, 0, z);
                Vector3 pointPosition = Vector3.Scale(cellPositionOnMesh,cellBounds) + centerCellOffset;
                
                DebugCellPosition[index] = pointPosition;
            }
        }

        private void OnDrawGizmos()
        {
            if (!DrawDebug) return;
            Gizmos.color = Color.red;
            Vector3 cellBounds = new Vector3(cellSize,cellSize,cellSize);
            
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            int iteration = DebugCellPosition.Length / 256;
            
            for (int i = 0; i < iteration; i++)
            {
                Gizmos.DrawWireCube(DebugCellPosition[i], cellBounds);
                Handles.Label(DebugCellPosition[i], i.ToString(), style);
            }
        }
    }
}
