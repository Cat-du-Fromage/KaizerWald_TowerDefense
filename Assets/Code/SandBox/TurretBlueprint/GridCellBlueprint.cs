using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KWUtils;

namespace TowerDefense
{
    public class GridCellBlueprint : MonoBehaviour
    {
        private const int NumCellAroundBlueprint = 4;
        private const int NumCellPerRow = 2;
        
        [SerializeField] private Material Free;
        [SerializeField] private Material Busy;
        [SerializeField] private GameObject GridBlueprint;
        
        void Start()
        {
            InitializeCellBlueprint();
        }

        private void InitializeCellBlueprint()
        {
            for (int i = 0; i < NumCellAroundBlueprint; i++)
            {
                (int x, int y) = i.GetXY(NumCellPerRow);
                GameObject go = Instantiate(GridBlueprint, transform);
                go.transform.localPosition = new Vector3(x - 0.5f, 0 - transform.position.y * 0.9f, y - 0.5f);
                go.GetComponent<Renderer>().material = Free;
            }
        }
    }
}
