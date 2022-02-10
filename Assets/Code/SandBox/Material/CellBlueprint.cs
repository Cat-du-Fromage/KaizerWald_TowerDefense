using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KWUtils;
using Unity.Mathematics;

namespace TowerDefense
{
    public class CellBlueprint : MonoBehaviour
    {
        
        private Texture2D texture;
        public MeshRenderer meshRender;
        public Renderer Renderer;

        private Mesh newMesh;
        
        private void Awake()
        {
            meshRender = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            Debug.Log(meshRender.sharedMaterial);
            newMesh = CreateMesh();
            GetComponent<MeshFilter>().mesh = newMesh;
            //
        }

        private void Update()
        {
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                Debug.Log($"width {meshRender.sharedMaterial.mainTexture.width} height {meshRender.sharedMaterial.mainTexture.height}");
            }
            
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SetGreen();
            }
            
            
        }

        private Mesh CreateMesh()
        {
            newMesh = new Mesh();
            
            Vector3[] vertices = new Vector3[9];
            Vector2[] uvs = new Vector2[9];
            int[] triangles = new int[4*6];

            for (int i = 0; i < 9; i++)
            {
                (int x, int y) = i.GetXY(3);
                
                vertices[i] = new Vector3(x-1, 0, y-1);
                Debug.Log($"Pos at {i} = {vertices[i]}");
            }

            for (int i = 0; i < 2; i++)
            {
                (int x, int y) = i.GetXY(3);
                int baseTriIndex = Mathf.Max(0,i * 6 - 1);
                Debug.Log(baseTriIndex);
                int vertexIndex = i + math.select(y,1 + y, x > 3);
                int4 trianglesVertex = new int4(vertexIndex, vertexIndex + 3, vertexIndex + 3-1, vertexIndex + 1);
                triangles[baseTriIndex] = trianglesVertex.z;
                triangles[baseTriIndex + 1] = trianglesVertex.y;
                triangles[baseTriIndex + 2] = trianglesVertex.x;
                //Debug.Log($"tri at {baseTriIndex} = {triangles[baseTriIndex]}; {triangles[baseTriIndex + 1]}; {triangles[baseTriIndex + 2]}");
                baseTriIndex += 3;
                triangles[baseTriIndex] = trianglesVertex.w;
                triangles[baseTriIndex + 1] = trianglesVertex.x;
                triangles[baseTriIndex + 2] = trianglesVertex.y;
               // Debug.Log($"tri at {baseTriIndex} = {triangles[baseTriIndex]}; {triangles[baseTriIndex + 1]}; {triangles[baseTriIndex + 2]}");
            }
            newMesh.SetVertices(vertices);
            newMesh.SetTriangles(triangles,0);
            return newMesh;
        }

        public void SetGreen()
        {
            texture = new Texture2D(10, 10);
            texture.filterMode = FilterMode.Point; //just fill the triangles / trillinear/Billinear = blend with surrounding cube
            texture.wrapMode = TextureWrapMode.Clamp; // what happen when we go outside of the limits? (in this case stretch value continu as if it was part of the range)
            texture.SetPixels(GetQuadrantColor(2));
            texture.Apply();
            meshRender.sharedMaterial.mainTexture = texture;
        }

        public Color[] GetQuadrantColor(int quad)
        {
            Color[] colourMap = new Color[100];
            Color free = new Color(0,1,0,0.25f);
            Color occupied = new Color(1,0,0,0.25f);

            int minR = (5 * quad);
            int maxR = (5 * quad);
            Debug.Log($"minR = {minR} maxR = {maxR}");
            
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    int index = y * 10 + x;
                    if((y >= minR && y < maxR) && (x >= minR && x < maxR))
                        colourMap[index] = occupied;
                    else
                        colourMap[index] = free;
                }
            }
/*
            for (int i = 0; i < 100; i++)
            {
                (int x, int y) = i.GetXY(10);
                int index = y * 10 + x;
                if((y >= minR && y < maxR) && (x >= minR && x < maxR))
                    colourMap[index] = occupied;
                else
                    colourMap[index] = free;
            }
*/
            return colourMap;
        }
    }
}
