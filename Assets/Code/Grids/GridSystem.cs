using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{

    public sealed class GridSystem : MonoBehaviour, IGridSystem<GridType>
    {
        [SerializeField] private EndGateComponent DestinationPath;
        [SerializeField] private StartSpawnComponent StartSpawnPath;
        
        [SerializeField] private AStarGrid AStarGrid;
        [SerializeField] private FlowFieldGrid FlowFieldGrid;
        [SerializeField] private StaticEntitiesGrid StaticEntitiesGrid;

        public TerrainData MapData { get; set; }
        public int2 MapBounds { get; set; }

        private void Awake()
        {
            DestinationPath    = DestinationPath.FindCheckNullComponent();
            StartSpawnPath     = StartSpawnPath.FindCheckNullComponent();

            StaticEntitiesGrid = this.GetCheckNullComponent(StaticEntitiesGrid);
            FlowFieldGrid      = this.GetCheckNullComponent(FlowFieldGrid);
            AStarGrid          = this.GetCheckNullComponent(AStarGrid);
            
            this.AsInterface<IGridSystem<GridType>>().Initialize();
        }

        public void SubscribeToGrid(GridType gridType, Action action)
        {
            switch (gridType)
            {
                case GridType.FlowField:
                    FlowFieldGrid.Grid.OnGridChange += action;
                    return;
                case GridType.Obstacles:
                    StaticEntitiesGrid.Grid.OnGridChange += action;
                    return;
            }
        }

        //===============================================================================================================================
        /// <summary>
        /// IMPORTANT : Need a explicit implementation or AOT will not be generated on build!
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void UsedOnlyForAOTCodeGeneration()
        {
            RequestGrid<bool, GenericGrid<bool>>(GridType.Obstacles);
            RequestGrid<Vector3, GenericChunkedGrid<Vector3>>(GridType.FlowField);
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
        //===============================================================================================================================
        
        public T2 RequestGrid<T1, T2>(GridType gridType) 
        where T1 : struct
        where T2 : GenericGrid<T1>
        {
            return gridType switch
            {
                GridType.Obstacles => StaticEntitiesGrid.Grid as T2,
                GridType.FlowField => FlowFieldGrid.Grid as T2,
                _ => throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null)
            };
        }
        

        public T1[] RequestGridArray<T1>(GridType gridType) 
        where T1 : struct
        {
            return gridType switch
            {
                GridType.Obstacles => StaticEntitiesGrid.Grid.GridArray as T1[],
                GridType.FlowField => FlowFieldGrid.Grid.GridArray as T1[],
                _ => throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null)
            };
        }
    }
}
