using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public class CellBlueprint : MonoBehaviour
    {
        private Texture2D texture;
        private MeshRenderer meshRender;

        private void Awake()
        {
            meshRender = GetComponent<MeshRenderer>();
            //Debug.Log(meshRender.sharedMaterial.mainTexture.dimension);
            //Debug.Log($"width {meshRender.sharedMaterial.mainTexture.width} height {meshRender.sharedMaterial.mainTexture.height}");
        }

        private void Start()
        {
            //Debug.Log(meshRender.sharedMaterial.mainTexture.dimension);
            //Debug.Log($"width {meshRender.sharedMaterial.mainTexture.width} height {meshRender.sharedMaterial.mainTexture.height}");
        }

        private void Update()
        {
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;
            SetGreen();
            
        }

        public void SetGreen()
        {
            texture = new Texture2D(10, 10);
            texture.filterMode = FilterMode.Point; //just fill the triangles / trillinear/Billinear = blend with surrounding cube
            texture.wrapMode = TextureWrapMode.Clamp; // what happen when we go outside of the limits? (in this case stretch value continu as if it was part of the range)
            texture.SetPixels(GetQuadrantColor(0));
            texture.Apply();
            meshRender.sharedMaterial.mainTexture = texture;
        }

        public Color[] GetQuadrantColor(int quad)
        {
            Color[] colourMap = new Color[100];
            Color free = new Color(0, 1, 0, 0.25f);
            Color occupied = new Color(1, 0, 0, 0.25f);
            if (quad == 0) //botleft
            {
                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        if(y < 5 && x < 5)
                            colourMap[y * 10 + x] = occupied;
                        else
                            colourMap[y * 10 + x] = free;
                    }
                }
            }

            return colourMap;
            /*
            
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    colourMap[y * 10 + x] = ; 
                }
            }
            */
        }
    }
}
