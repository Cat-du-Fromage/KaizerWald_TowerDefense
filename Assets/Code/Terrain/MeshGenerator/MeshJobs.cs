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
using UnityEngine.Rendering;

using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KWUtils.KWmath;
using static KWUtils.NativeCollectionExt;

namespace KaizerWaldCode.MapGeneration
{
    //=====================================================================
    // JOB SYSTEM
    //=====================================================================
    
    //VERTICES POSITION
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesPosJob : IJobFor
    {
        [ReadOnly] private int jMapSize;
        [ReadOnly] private int jPointPerAxis;
        [ReadOnly] private float jSpacing;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> jVertices;

        public VerticesPosJob(in MapSettings mapSettings, NativeArray<float3> vertices)
        {
            jMapSize = mapSettings.mapSize;
            jPointPerAxis = mapSettings.mapPointPerAxis;
            jSpacing = mapSettings.pointSpacing;
            jVertices = vertices;
        }
        
        public void Execute(int index)
        {
            int z = (int)floor(index / (float)jPointPerAxis);
            int x = index - (z * jPointPerAxis);
            
            float3 pointPosition = float3(x, 0, z) * float3(jSpacing) + float3(jMapSize*-0.5f,0,jMapSize*-0.5f);
            jVertices[index] = pointPosition;
        }
    }
    
    //UVS
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct UvsJob : IJobFor
    {
        [ReadOnly] private int JMapPointPerAxis;
        [NativeDisableParallelForRestriction] [WriteOnly] private NativeArray<float2> JUvs;

        public UvsJob(in MapSettings mapSettings, NativeArray<float2> uvs)
        {
            JMapPointPerAxis = mapSettings.mapPointPerAxis;
            JUvs = uvs;
        }
        
        public void Execute(int index)
        {
            float z = floor((float)index / JMapPointPerAxis);
            float x = index - (z * JMapPointPerAxis);
            JUvs[index] = float2(x / JMapPointPerAxis, z / JMapPointPerAxis);
        }
    }
    
    //TRIANGLES
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct TrianglesJob : IJobFor
    {
        [ReadOnly] private int JMapPointPerAxis;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> JTriangles;

        public TrianglesJob(in MapSettings mapSettings, NativeArray<int> triangles)
        {
            JMapPointPerAxis = mapSettings.mapPointPerAxis;
            JTriangles = triangles;
        }
        
        public void Execute(int index)
        {
            int mapPoints = JMapPointPerAxis - 1;
            
            int z = (int)floor((float)index / mapPoints);
            int x = index - (z * mapPoints);
            int baseTriIndex = index * 6;
            
            int vertexIndex = index + select(z,1 + z, x > mapPoints);
            int4 trianglesVertex = int4(vertexIndex, vertexIndex + JMapPointPerAxis + 1, vertexIndex + JMapPointPerAxis, vertexIndex + 1);

            JTriangles[baseTriIndex] = trianglesVertex.z;
            JTriangles[baseTriIndex + 1] = trianglesVertex.y;
            JTriangles[baseTriIndex + 2] = trianglesVertex.x;
            baseTriIndex += 3;
            JTriangles[baseTriIndex] = trianglesVertex.w;
            JTriangles[baseTriIndex + 1] = trianglesVertex.x;
            JTriangles[baseTriIndex + 2] = trianglesVertex.y;
        }
    }
}