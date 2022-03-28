using System;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{
    public class StaticEntitiesGrid: MonoBehaviour, IGridHandler<GridType, bool, GenericGrid<bool>>
    {
        private const int CellSize = 4;
        
        //==============================================================================================================
        /// Grid Interface
        public IGridSystem<GridType> GridSystem { get; set; }
        public GenericGrid<bool> Grid { get; private set; }
        public void InitializeGrid(int2 terrainBounds) => Grid = new GenericGrid<bool>(terrainBounds, CellSize);
        //==============================================================================================================
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(Grid == null) return;
            Vector3 cubeBounds = (Vector3.one * CellSize).SetY(0.5f);
            Gizmos.color = Color.red;
            for (int i = 0; i < Grid.GridArray.Length; i++)
            {
                if (Grid.GetValue(i) == false) continue;
                Gizmos.DrawCube(Grid.GetCellCenter(i), cubeBounds);
            }
        }
#endif
    }
}