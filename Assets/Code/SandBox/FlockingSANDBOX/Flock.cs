using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KWUtils;
using UnityEngine.InputSystem;
using static KWUtils.KWmath;

namespace TowerDefense
{
    public class Flock : MonoBehaviour
    {
        [SerializeField] private EnemyManager EnemyManager; //Use to have agents
        
        //public FlockAgent Agent;
        private List<FlockAgent> agents = new List<FlockAgent>(2);
        public FlockBehaviour behaviour;

        public int AgentCount = 16;
        public float DriveFactor = 10f; // Multiply so agent may move faster
        
        public float MaxSpeed = 5f;
        public float NeighborRadius = 1.5f; //Range to check for neighbor => Will be change for spatial partition
        public float AvoidanceRadiusMultiplier = 0.5f;//Multiply to neighborRadius: get actual radius where agent want to stay away from others

        private float squareMaxSpeed;
        private float squareNeighborRadius;
        private float squareAvoidanceRadius;
        
        public float SquareAvoidanceRadius => squareAvoidanceRadius;
        
        private void Awake()
        {
            EnemyManager ??= FindObjectOfType<EnemyManager>();
            squareMaxSpeed = Sq(MaxSpeed);
            squareNeighborRadius = Sq(NeighborRadius);
            squareAvoidanceRadius = squareNeighborRadius * Sq(AvoidanceRadiusMultiplier);
        }

        private void Update()
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                agents = EnemyManager.CreateFlockWave(AgentCount);
            }
        }
    }
}
