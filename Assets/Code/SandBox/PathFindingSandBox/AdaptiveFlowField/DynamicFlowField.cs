using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using UnityEngine;

namespace TowerDefense
{
    public class DynamicFlowField : MonoBehaviour, IGridHandler<Vector3>
    {
        public IGridSystem GridSystem { get; set; }
        public SimpleGrid<Vector3> Grid { get; private set; }
        
        
    }
}
