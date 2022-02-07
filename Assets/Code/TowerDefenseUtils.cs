using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public static class TowerDefenseUtils
    {
        public static readonly LayerMask TerrainLayerMask = 1 << 8;
        public static readonly LayerMask UnitLayer = 1 << 9;
        public static readonly LayerMask ObstacleLayer = 1 << 10;
    }
}
