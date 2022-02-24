using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace TowerDefense
{
    public class EnemyManager : MonoBehaviour
    {
        //References to Grids
        [SerializeField] private SandBox_SpatialPartition spatialPartition;
        [SerializeField] private PathfindingGrid PathfindingGrid;

        //Prefabs
        [SerializeField] private Transform EndPoint;
        [SerializeField] private GameObject EnemyPrefab;

        private Vector3 spawn;
        
        private HashSet<EnemyComponent> enemies;
        private HashSet<EnemyComponent> enemiesGone;

        private Dictionary<int, EnemyComponent> enemiesData;
        private Dictionary<int, Transform> enemiesTransforms;
        private List<int> enemiesToRemove;

        //multithreading Move
        //private List<Transform> enemyTransforms;
        private TransformAccessArray TransformAccesses;

        private void Awake()
        {
            PathfindingGrid ??= FindObjectOfType<PathfindingGrid>();
            
            enemies = new HashSet<EnemyComponent>(16);
            enemiesGone = new HashSet<EnemyComponent>(16);

            enemiesData = new Dictionary<int, EnemyComponent>(16);
            enemiesTransforms = new Dictionary<int, Transform>(16);
            enemiesToRemove = new List<int>(16);
            
            TransformAccesses = new TransformAccessArray(16);
            //enemyTransforms = new List<Transform>(enemies.Count);
        }
        

        // Update is called once per frame
        private void Update()
        {
            if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;
            CreateWave(16);
        }

        private void FixedUpdate()
        {
            if (enemies.Count == 0) return;
            //MoveToDestination();
        }

        private void LateUpdate()
        {
            ClearEnemiesGone();
            //if (enemiesGone.Count == 0) return;
            //CheckEnemyArrived();
        }

        public void EnemyKilled(EnemyComponent enemy)
        {
            enemiesGone.Add(enemy);
            enemiesToRemove.Add(enemy.GetInstanceID());
        }

        private void MoveAllEnemies()
        {
            JMoveEnemies job = new JMoveEnemies()
            {

            };
            job.Schedule(TransformAccesses,default);
            TransformAccesses.Dispose();
        }
        

        public void CreateWave(int numToSpawn) //temporary public
        {
            Vector3[] spawnPoints = PathfindingGrid.GetSpawnPointsForEntities(numToSpawn, 2);
            for (int i = 0; i < numToSpawn; i++)
            {
                GameObject go = Instantiate(EnemyPrefab, spawnPoints[i], Quaternion.identity);
                go.name = $"Agent_{i}";
                EnemyComponent enemy = go.GetComponent<EnemyComponent>();
                
                enemy.SetDestination(EndPoint.position);
                enemies.Add(enemy);
                
                enemiesData.Add(enemy.GetInstanceID(), enemy);
                enemiesTransforms.Add(enemy.GetInstanceID(), enemy.transform);
                TransformAccesses.Add(enemy.transform);
            }
        }

        private void ClearEnemiesGone()
        {
            if (enemiesToRemove.Count == 0) return;
            for (int i = 0; i < enemiesToRemove.Count; i++)
            {
                enemiesData.Remove(enemiesToRemove[i]);
                enemiesTransforms.Remove(enemiesToRemove[i]);
            }
            TransformAccesses = new TransformAccessArray(enemiesTransforms.GetValuesArray());
            enemiesToRemove.Clear();
        }
/*
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
        */
    }
    
    //[BurstCompile(CompileSynchronously = true)]
    public struct JMoveEnemies : IJobParallelForTransform
    {
        
        public void Execute(int index, TransformAccess transform)
        {
            

            //transform.position = unitPos;
            //transform.rotation = quaternion.LookRotation(-crossDirection, up());
        }
    }
}

/*
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
*/