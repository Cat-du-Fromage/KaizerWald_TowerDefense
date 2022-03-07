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
    public interface IGridHandler<T>
    {
        public IGridSystem GridSystem { get; set; }
        
        public SimpleGrid<T> Grid { get;}

        public void SetGridSystem(IGridSystem gridSystem)
        {
            GridSystem = gridSystem;
        }
    }

    public interface IGridSystem
    {
        public void NotifyGridSystem<T>(Component handler, int index, T value);

        public T[] RequestArray<T>(GridType grid);
        
        public SimpleGrid<T> RequestGrid<T>(GridType grid);

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
        [SerializeField] private EnemyManager EnemyGrid;
        
        [SerializeField] private AStarPathfinding2 Astar;

        private void Awake()
        {
            TurretGrid ??= FindObjectOfType<BuildManager>();
            TurretGrid.GetInterfaceComponent<IGridHandler<bool>>().SetGridSystem(this);
            
            Astar ??= FindObjectOfType<AStarPathfinding2>();
            Astar.GetInterfaceComponent<IGridHandler<Node>>().SetGridSystem(this);
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

        public SimpleGrid<T> RequestGrid<T>(GridType grid)
        {
            if (grid == GridType.Turret)
            {
                return TurretGrid.Grid as SimpleGrid<T>;
            }

            return null;
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
