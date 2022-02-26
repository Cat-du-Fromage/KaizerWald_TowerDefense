using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.TerrainTools;

namespace TowerDefense
{
    public class TerrainComponent : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private TerrainLayer layer;
        
        private float[,,] element;
        
        // Start is called before the first frame update
        private void Awake()
        {
            //element = new float[1, 1, terrain.terrainData.alphamapLayers];
        }

        void Start()
        {
            EdiTTerrainManual();
        }

        private void EdiTTerrainManual()
        {
            Rect surface = new Rect(0, 0, 16, 16);
            PaintContext paintContext = PaintContext.CreateFromBounds(terrain, surface, 1,1);
            paintContext.CreateRenderTargets(RenderTextureFormat.R8);
            paintContext.GatherAlphamap(layer);
            paintContext.GatherNormals();
            paintContext.ScatterAlphamap("test");
            paintContext.Cleanup();
            PaintContext.ApplyDelayedActions();
            //paintContext.destinationRenderTexture.Create() = paintContext.sourceRenderTexture. ;

        }
    }
}
