using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;

namespace TowerDefense
{
    /// <summary>
    /// HINT we want to make them only visible WHEN pressing "Space"
    /// </summary>
    public class SandBox_FieldOfView : MonoBehaviour
    {
        [SerializeField] private float FieldOfView = 90.0f; //90°
        [SerializeField] private int RayCount = 50; // or number of triangle

        private Vector3 entityOrigin;
        private float startAngle;

        private void Awake()
        {
            Debug.Log($"FieldOfView / 2.0f + 180 = {FieldOfView / 2.0f + 90} ");
            entityOrigin = Vector3.zero;
            startAngle = FieldOfView / 2.0f + 90; //why 90?... who knows?!
            // ALL IS COUNTER CLOCK! final = take final Node
        }

        public void GetFieldOfView()
        {
            Mesh viewMesh = new Mesh();
            GetComponent<MeshFilter>().mesh = viewMesh;
            
            float viewAngle = startAngle;
            float viewAngleIncrease = FieldOfView / RayCount;
            float viewDistance = 10.0f;

            //fire ray at 0°; 45°; 90°
            Vector3[] vertices = new Vector3[RayCount + 1 + 1];// +1 origin; +1 ray[0] = 0°
            //Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[RayCount * 3];

            vertices[0] = entityOrigin;
            int vertexIndex = 1; // 0 is origin
            int triangleIndex = 0;
            for (int i = 0; i <= RayCount; i++)
            {
                Vector3 vertex = entityOrigin + GetVectorFromAngle(viewAngle) * viewDistance;
                vertices[vertexIndex] = vertex;

                if (i != 0)
                {
                    triangles[triangleIndex + 0] = 0;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = vertexIndex;
                    triangleIndex += 3;
                }
                
                vertexIndex++;
                viewAngle -= viewAngleIncrease; // - so it goes clockWise; + = counter clockWise
            }
            
            viewMesh.SetVertices(vertices);
            viewMesh.SetTriangles(triangles,0);
            
            viewMesh.RecalculateNormals();
            viewMesh.RecalculateBounds();
            viewMesh.RecalculateTangents();
            //viewMesh.bounds = new Bounds(entityOrigin, Vector3.one * 1000f);

            transform.position = transform.position.SetAxis(Axis.Y, 0.1f);
        }
        

        private Vector3 GetVectorFromAngle(float angle)
        {
            float angleRad = angle * (Mathf.PI / 180.0f);
            return new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad));
        }

        private float GetAngleFromVector(Vector3 direction)
        {
            direction = direction.normalized;
            //float angle = Vector3.Angle(Vector3.zero, direction);
            
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            //Debug.Log($"BEFORE angle = {angle}");
            if (angle < 0) angle += 180;
            //Debug.Log($"AFTER angle = {angle}");
            return angle;
        }
    }
}
