using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using static KWUtils.NativeCollectionExt;
using static KWUtils.KWGrid;
#if UNITY_EDITOR
namespace TowerDefense
{
    //SOA
    public interface IEntity
    {
        public int EntityIndex { get; set; }
        public int CurrentCellIndex { get; set; }
        public int PreviousCellIndex { get; set; }
        public Vector3 Position { get; set; }
    }
    
    //AOT Prefered this one if possible
    public interface IEntityManager
    {
        public int[] Indices { get; set; }
        public int[] CurrentCellIndices { get; set; }
        public int[] PreviousCellIndices { get; set; }
        public float3[] Positions { get; set; }
    }
    
    public struct SpCell
    {
        private int index;
        public bool IsTrigger;
        public List<int> EntitiesId;
        public List<int> TriggeredEntitiesId;

        public SpCell(int id)
        {
            index = id;
            IsTrigger = false;
            EntitiesId = new List<int>(4);
            TriggeredEntitiesId = new List<int>(4);
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

        public Vector3[] DebugCellPosition;
        
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
            for (int i = 0; i < cells.Length; i++)
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
        public void OnNotifyFirstSpawn(IEntity entity)
        {
            //Spawn Enemy
            cells[entity.CurrentCellIndex].EntitiesId.Add(entity.EntityIndex);
        }
        
        public void OnNotifyUpdate(IEntity entity)
        {
            //Spawn Enemy
            cells[entity.PreviousCellIndex].EntitiesId.Remove(entity.EntityIndex);
            cells[entity.CurrentCellIndex].EntitiesId.Add(entity.EntityIndex);
            
            int index = GetCellIndexFromPosition(entity.Position);
            //cells[index] = 
        }
        
        /// <summary>
        /// Check
        /// Index in Grid(position) == current Cell index ??
        /// if not update shits
        /// </summary>
        public void CheckEntitiesCell(ref IEntityManager entityManager, int[] movedEntities)
        {
            //Get Changed Entities (uint: id?)
            using NativeArray<SpCell> jobCells     = cells.ToNativeArray();
            using NativeArray<int> registeredCell  = entityManager.PreviousCellIndices.ToNativeArray();
            using NativeArray<float3> positions    = entityManager.Positions.ToNativeArray();

            using NativeArray<int> entityToCheck   = movedEntities.ToNativeArray();
            
            //Update triggered
            
            using NativeList<int> triggeredCells   = new NativeList<int>(entityToCheck.Length, Allocator.TempJob);

            JCheckEntitiesCell job = new JCheckEntitiesCell
            {
                CellSize = cellSize,
                MapSize = mapWidthHeight,
                Cells = jobCells,
                Positions = positions,
                EntityToCheck = entityToCheck,
                RegisteredCell = registeredCell,
                TriggeredCell = triggeredCells.AsParallelWriter()
            };
            JobHandle jobHandle = job.ScheduleParallel(movedEntities.Length, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            
            registeredCell.CopyTo(entityManager.CurrentCellIndices);
        }
        
        private struct JCheckEntitiesCell : IJobFor
        {
            [ReadOnly] public int CellSize;
            [ReadOnly] public int2 MapSize;
            
            [NativeDisableParallelForRestriction]
            [ReadOnly] public NativeArray<SpCell> Cells;
            
            [NativeDisableParallelForRestriction]
            [ReadOnly] public NativeArray<float3> Positions;

            [NativeDisableParallelForRestriction]
            [ReadOnly] public NativeArray<int> EntityToCheck;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<int> RegisteredCell;
            
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeList<int>.ParallelWriter TriggeredCell;
            
            public void Execute(int index)
            {
                int entityIndex = EntityToCheck[index];
                int updatedCellIndex = Positions[entityIndex].GetIndexFromPosition(MapSize, CellSize);
                if (RegisteredCell[entityIndex] != updatedCellIndex)
                {
                    RegisteredCell[entityIndex] = updatedCellIndex;
                    TriggeredCell.AddNoResizeIf(Cells[updatedCellIndex].IsTrigger, updatedCellIndex);
                }
            }
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
            if (DebugCellPosition.Length == 0)
            {
                GetCellCenter();
            }
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
#endif