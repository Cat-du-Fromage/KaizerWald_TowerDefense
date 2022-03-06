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

        public SimpleGrid<bool> RequestTurretGrid();
        
        public SimpleGrid<T> RequestGrid<T>(GridType grid);

        public void OnGridChange(GridType grid, int index);
    }

    public enum GridType
    {
        Turret,
    }
    
    /// <summary>
    /// REGISTER ALL GRID!
    /// </summary>
    public class GridSystem : MonoBehaviour, IGridSystem
    {
        [SerializeField] private BuildManager buildManager;
        [SerializeField] private AStarPathfinding2 Astar;

        private void Awake()
        {
            buildManager ??= FindObjectOfType<BuildManager>();
            buildManager.GetInterfaceComponent<IGridHandler<bool>>().SetGridSystem(this);
            
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

        public SimpleGrid<bool> RequestTurretGrid() => buildManager.Grid;
        
        public SimpleGrid<T> RequestGrid<T>(GridType grid)
        {
            if (grid == GridType.Turret)
            {
                return buildManager.Grid as SimpleGrid<T>;
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
