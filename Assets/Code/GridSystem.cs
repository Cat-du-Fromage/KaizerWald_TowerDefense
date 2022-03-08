using System;
using KWUtils;
using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

using static KWUtils.GameObjectExtension;

namespace TowerDefense
{
    public interface IGridHandler<T1, T2>
    where T1 : class, IGenericGrid<T2>
    where T2 : struct
    {
        public IGridSystem GridSystem { get; set; }
        
        public T1 Grid{ get;}

        public void SetGridSystem(IGridSystem gridSystem)
        {
            GridSystem = gridSystem;
        }
    }

    public interface IGridSystem
    {
        public void NotifyGridSystem<T>(Component handler, int index, T value);

        public T[] RequestArray<T>(GridType grid);
        
        public IGenericGrid<T> RequestGrid<T>(GridType grid) where T : struct;

        public void OnGridChange(GridType grid, int index);
    }

    public enum GridType
    {
        Turret,
        Enemy,
        FlowField
    }
    
    /// <summary>
    /// REGISTER ALL GRID!
    /// </summary>
    public class GridSystem : MonoBehaviour, IGridSystem
    {
        [SerializeField] private BuildManager TurretGrid;
        [SerializeField] private PathfindingGrid FlowFieldGrid;
        [SerializeField] private EnemyManager EnemyGrid;
        
        [SerializeField] private AStarPathfinding2 Astar;

        private void Awake()
        {
            TurretGrid ??= FindObjectOfType<BuildManager>();
            TurretGrid.GetInterfaceComponent<IGridHandler<SimpleGrid<bool>, bool>>().SetGridSystem(this);
            
            FlowFieldGrid ??= FindObjectOfType<PathfindingGrid>();
            FlowFieldGrid.GetInterfaceComponent<IGridHandler<ChunkedGrid<Vector3>, Vector3>>().SetGridSystem(this);
            
            Astar ??= FindObjectOfType<AStarPathfinding2>();
            Astar.GetInterfaceComponent<IGridHandler<SimpleGrid<Node>, Node>>().SetGridSystem(this);
        }
        

        public void NotifyGridSystem<T>(Component handler, int index ,T value)
        {
            if (handler is BuildManager)
            {
                //Notify turret was added OR removed
            }
        }

        public T[] RequestArray<T>(GridType grid)
        {
            if (grid == GridType.Turret)
            {
                return TurretGrid.Grid.GridArray as T[];
            }
            
            return null;
        }

        public IGenericGrid<T> RequestGrid<T>(GridType grid)
        where T : struct
        {
            if (grid == GridType.Turret)
            {
                return TurretGrid.Grid as IGenericGrid<T>;
            }
            else if (grid == GridType.FlowField)
            {
                return FlowFieldGrid.Grid as IGenericGrid<T>;
            }
            else
            {
                return null;
            }
        }
        
        public void OnGridChange(GridType grid, int index)
        {
            if (grid == GridType.Turret)
            {
                Astar.OnObstacleAdded(index);
            }
        }
    }

}
