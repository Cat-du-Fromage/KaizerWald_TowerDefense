using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    /// <summary>
    /// Leader :
    /// Get Units attached to him
    /// When all units die, he dies too
    /// </summary>
    public class LeaderPath : MonoBehaviour
    {
        [SerializeField] private AStarPathfinding2 pathGrid;
        private int currentPathNodeIndex;
        private int[] pathIndices;
        private Vector3 currentGoal;
        private Vector3[] PathPositions;
        
        public Vector3 currentDirection;
        public Vector3 currentRotation;
        private void Awake()
        {
            pathGrid ??= FindObjectOfType<AStarPathfinding2>();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                OnFirstRequest();
            }
            if (PathPositions.IsNullOrEmpty()) return;
            //Update? => Leader GetCurrentDirection He Must Follow
            GetCellCurrentlyIn();
            MoveLeader();
            //during this : Rotate towards current cell -> next cell (if in Last cell follow same direction)

            //Update : Is he in the Same cell than the one he is pursuing?

            // if yes : Calculate New Direction


        }

        private void OnFirstRequest()
        {
            (PathPositions, pathIndices) = pathGrid.RequestPath(transform.position);
            currentDirection = (PathPositions[1] - PathPositions[0]).normalized.Flat();
            currentPathNodeIndex = 0;
            currentGoal = PathPositions[1];
            currentRotation = (PathPositions[currentPathNodeIndex+1] - PathPositions[currentPathNodeIndex]).normalized.Flat();
        }

        private void GetCellCurrentlyIn()
        {
            //int currentCell = pathGrid.Grid.GetIndexFromPosition(transform.position);
            
            //If we are in the last or 1 before the Last we keep the same direction!
            if (currentPathNodeIndex == pathIndices[^2] || currentPathNodeIndex == pathIndices[^1]) return;
            //Debug.Log($"current distance! {(currentGoal - transform.position).magnitude}");
            if ((currentGoal - transform.position).magnitude < 1.8f)
            {
                currentRotation = (PathPositions[currentPathNodeIndex+1] - PathPositions[currentPathNodeIndex]).normalized.Flat();
            }
        
            if ((currentGoal - transform.position).magnitude < 0.05f)
            {
                currentPathNodeIndex++;
                currentGoal = PathPositions[currentPathNodeIndex];
            }
        }

        private void MoveLeader()
        {
            Quaternion lookRotation = Quaternion.LookRotation(currentRotation, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 1f);
            Vector3 newPosition = Vector3.MoveTowards(transform.position, currentGoal, Time.deltaTime * 1f);
            transform.SetPositionAndRotation(newPosition, newRotation);
        }
    }
}
