using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense
{
    public sealed class TerrainDataProvider : Singleton<TerrainDataProvider>
    {
        [SerializeField] private Terrain terrain;

        private TerrainData data;
        private int2 size2D;

        protected sealed override void Awake()
        {
            terrain ??= GetComponent<Terrain>();
            data = terrain.terrainData;

            size2D = (int2)data.size.XZ();
            base.Awake();
        }

        public int2 TerrainWidthHeight => size2D;
    }
}
