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

        private void Start()
        {
            InitializeCellBlueprint();
        }

        private void InitializeCellBlueprint()
        {
            Transform currentTransform = transform;
            
            for (int i = 0; i < NumCellAroundBlueprint; i++)
            {
                (int x, int y) = i.GetXY(NumCellPerRow);
                GameObject go = Instantiate(GridBlueprint, currentTransform);
                go.transform.localPosition = new Vector3(x - 0.5f, 0 - currentTransform.position.y * 0.9f, y - 0.5f); //HARD CODED!
                go.GetComponent<Renderer>().material = Free;
            }
            
            Transform[] children = GetTurretPlacementTokens(currentTransform);
            DetachPlacementToken(children);
            //currentTransform.DetachChildren();
            currentTransform.localScale = new Vector3(2f, 1f, 2f); //HARD CODED!
            ReScalePlacementTokens(children, currentTransform);
        }

        private void ReScalePlacementTokens(Transform[] children, Transform currentTransform)
        {
            for (int i = 1; i < NumCellAroundBlueprint+1; i++)
            {
                children[i].localScale = new Vector3(0.1f, 0.1f, 0.1f);
                children[i].SetParent(currentTransform);
            }
        }

        private void DetachPlacementToken(Transform[] tokens)
        {
            for (int i = 1; i < tokens.Length; i++)
            {
                tokens[i].parent = null;
            }
        }

        private Transform[] GetTurretPlacementTokens(Transform currentTransform)
        {
            Transform[] children = new Transform[currentTransform.childCount];
            for (int i = 1; i < currentTransform.childCount; i++)
            {
                children[i] = currentTransform.GetChild(i);
            }
            return children;
        }
    }
}
