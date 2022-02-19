using System;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.Rendering;


using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KWUtils.KWmath;
using static KWUtils.NativeCollectionExt;

namespace KaizerWaldCode.MapGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider colliderMesh;
        
        [SerializeField] private MapSettings mapSettings;
        
        public MapSettings GetMapSettings() => mapSettings;

        public void NewGameSettings(TerrainType[] regions)
        {
            mapSettings.NewGame();
            SetPositionToZero();
            SetMesh();
        }

        public void SetPositionToZero() => gameObject.transform.position = Vector3.zero - new Vector3(mapSettings.mapSize/2f,2,mapSettings.mapSize/2f);

        private void SetMesh()
        {
            meshFilter.sharedMesh = colliderMesh.sharedMesh = MeshGenerator.GetTerrainMesh(mapSettings);
        }

    }
}