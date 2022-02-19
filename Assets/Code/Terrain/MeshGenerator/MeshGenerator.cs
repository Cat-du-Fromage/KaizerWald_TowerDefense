using System;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration.Data;
using KWUtils;
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
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;


namespace KaizerWaldCode.MapGeneration
{
    public static class MeshGenerator
    {
        public static Mesh GetTerrainMesh(MapSettings mapSettings)
        {
            Mesh mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
                name = "MapTerrain"
            };
            mesh.SetVertices(GetVertices(mapSettings).ReinterpretArray<float3, Vector3>());
            mesh.SetTriangles(GetTriangles(mapSettings),0);
            mesh.SetUVs(0, GetUvs(mapSettings).ReinterpretArray<float2, Vector2>());
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        //=====================================================================
        // GET DATA
        //=====================================================================
        
        public static float3[] GetVertices(MapSettings mapSettings, JobHandle dependency = default)
        {
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(mapSettings.totalMapPoints);
            VerticesPosJob job = new VerticesPosJob(in mapSettings, verticesTemp);
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            return verticesTemp.ToArray();
        }
        
        private static float2[] GetUvs(MapSettings mapSettings, JobHandle dependency = default)
        {
            using NativeArray<float2> uvsTemp = AllocNtvAry<float2>(mapSettings.totalMapPoints);
            UvsJob job = new UvsJob(in mapSettings, uvsTemp);
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.totalMapPoints, JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            return uvsTemp.ToArray();
        }
        
        private static int[] GetTriangles(MapSettings mapSettings, JobHandle dependency = default)
        {
            int trianglesBufferSize = sq(mapSettings.mapPointPerAxis - 1) * 6;
            using NativeArray<int> trianglesTemp = AllocNtvAry<int>(trianglesBufferSize);
            TrianglesJob job = new TrianglesJob(in mapSettings, trianglesTemp);
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.mapPointPerAxis-1), JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            return trianglesTemp.ToArray();
        }
    }
}