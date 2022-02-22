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
        [SerializeField] private Transform Destination;
        [SerializeField] private PathfindingGrid grid;
        [SerializeField] private GameObject AgentPrefab;
        [SerializeField] private EnemyManager EnemyManager; //Use to have agents
        
        //public FlockAgent Agent;
        private List<FlockAgent> agents = new List<FlockAgent>(2);
        public FlockBehaviour behaviour;

        public int AgentCount = 2;
        public float DriveFactor = 4f; // Multiply so agent may move faster
        
        public float MaxSpeed = 2f;
        public float NeighborRadius = 3f; //Range to check for neighbor => Will be change for spatial partition
        public float AvoidanceRadiusMultiplier = 0.5f;//Multiply to neighborRadius: get actual radius where agent want to stay away from others

        private float squareMaxSpeed;
        private float squareNeighborRadius;
        private float squareAvoidanceRadius;
        
        public float SquareAvoidanceRadius => squareAvoidanceRadius;
        
        private void Awake()
        {
            grid ??= FindObjectOfType<PathfindingGrid>();
            EnemyManager ??= FindObjectOfType<EnemyManager>();
            squareMaxSpeed = Sq(MaxSpeed);
            squareNeighborRadius = Sq(NeighborRadius);
            squareAvoidanceRadius = squareNeighborRadius * Sq(AvoidanceRadiusMultiplier);
        }

        private void Update()
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                agents = CreateFlockWave(16);
            }

            if (agents.Count == 0) return;
            
            for (int i = 0; i < agents.Count; i++)
            {
                List<Transform> nearbyObjects = GetNearbyObjects(agents[i]);
                Vector3 move = behaviour.CalculateMove(agents[i], nearbyObjects, this);
                move *= DriveFactor;
                
                if (move.sqrMagnitude < squareMaxSpeed)
                {
                    move = move.normalized * MaxSpeed;
                }

                Vector3 destinationDir = agents[i].transform.position.DirectionTo(Destination.position);
                agents[i].Move(destinationDir + move.Flat());
            }
        }
        
        public List<FlockAgent> CreateFlockWave(int numToSpawn) //temporary public
        {
            List<FlockAgent> agents = new List<FlockAgent>(numToSpawn);
            Vector3[] spawnPoints = grid.GetSpawnPoints(numToSpawn, 2);
            for (int i = 0; i < numToSpawn; i++)
            {
                GameObject go = Instantiate(AgentPrefab, spawnPoints[i] + Vector3.up, Quaternion.identity);
                go.name = $"Agent_{i}";
                agents.Add(go.GetComponent<FlockAgent>());
            }

            return agents;
        }

        private List<Transform> GetNearbyObjects(FlockAgent agent)
        {
            List<Transform> nearbyObjects = new List<Transform>(4);
            Collider[] nearbyColliders = Physics.OverlapSphere(agent.transform.position, NeighborRadius, 1<<10);
            for (int i = 0; i < nearbyColliders.Length; i++)
            {
                if (nearbyColliders[i] == agent.AgentCollider) continue;
                nearbyObjects.Add(nearbyColliders[i].transform);
            }
            return nearbyObjects;
        }
    }
}
