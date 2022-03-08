using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public static class TowerDefenseUtils
    {
        private static Camera camera;

        public static Camera PlayerCamera
        {
            get
            {
                if(camera == null) camera = Camera.main;
                return camera;
            }
        }
        
        //LAYERS
        public static readonly LayerMask TerrainLayerMask = 1 << 8;
        public static readonly LayerMask UnitLayer = 1 << 9;
        public static readonly LayerMask EnemyLayer = 1 << 10;
        public static readonly LayerMask BulletLayer = 1 << 10;
    }
}
