using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public class EnemyManager : MonoBehaviour
    {
        //References to Grids
        [SerializeField] private SandBox_SpatialPartition spatialPartition;
        [SerializeField] private PathfindingGrid PathfindingGrid;
        
        //Reference to FlockManager
        
        //Prefabs
        [SerializeField] private Transform EndPoint;
        [SerializeField] private GameObject EnemyPrefab;

        private Vector3 spawn;

        private HashSet<EnemyComponent> enemies;
        private HashSet<EnemyComponent> enemiesGone;

        private void Awake()
        {
            PathfindingGrid ??= FindObjectOfType<PathfindingGrid>();
            
            enemies = new HashSet<EnemyComponent>(16);
            enemiesGone = new HashSet<EnemyComponent>(16);
        }
        

        // Update is called once per frame
        private void Update()
        {
            if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;
            CreateFlockWave(16);
        }

        private void FixedUpdate()
        {
            if (enemies.Count == 0) return;
            MoveToDestination();
        }

        private void LateUpdate()
        {
            if (enemiesGone.Count == 0) return;
            CheckEnemyArrived();
        }

        public void EnemyKilled(EnemyComponent enemy)
        {
            enemiesGone.Add(enemy);
        }
        
/*
        public void CreateWave(int numToSpawn) //temporary public
        {
            Vector3[] spawnPoints = PathfindingGrid.GetSpawnPoints(numToSpawn, 2);
            for (int i = 0; i < numToSpawn; i++)
            {
                GameObject go = Instantiate(EnemyPrefab, spawnPoints[i], Quaternion.identity);
                go.name = $"Agent_{i}";
                EnemyComponent enemy = go.GetComponent<EnemyComponent>();
                
                enemy.SetDestination(EndPoint.position);
                enemies.Add(enemy);
            }
        }
        */
        public List<FlockAgent> CreateFlockWave(int numToSpawn) //temporary public
        {
            List<FlockAgent> agents = new List<FlockAgent>(numToSpawn);
            Vector3[] spawnPoints = PathfindingGrid.GetSpawnPoints(numToSpawn, 2);
            for (int i = 0; i < numToSpawn; i++)
            {
                GameObject go = Instantiate(EnemyPrefab, spawnPoints[i], Quaternion.identity);
                go.name = $"Agent_{i}";
                EnemyComponent enemy = go.GetComponent<EnemyComponent>();
                
                enemy.SetDestination(EndPoint.position);
                enemies.Add(enemy);
                agents.Add(go.GetComponent<FlockAgent>());
            }

            return agents;
        }

        private void MoveToDestination()
        {
            foreach (EnemyComponent enemy in enemies)
            {
                enemy.SetMove();
                if (enemy.CheckDestinationEnd())
                {
                    enemiesGone.Add(enemy);
                }
            }
        }

        private void CheckEnemyArrived()
        {
            foreach (EnemyComponent gone in enemiesGone)
            {
                if (gone == null) continue;
                Destroy(gone.gameObject);
            }
            enemies.ExceptWith(enemiesGone);
            enemiesGone.Clear();
        }
    }
}
