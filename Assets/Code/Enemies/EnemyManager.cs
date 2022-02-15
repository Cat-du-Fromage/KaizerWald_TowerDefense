using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private Transform SpawnPoint;
        [SerializeField] private Transform EndPoint;
        [SerializeField] private GameObject EnemyPrefab;

        private Vector3 spawn;

        private HashSet<EnemyComponent> enemies = new HashSet<EnemyComponent>(16);

        private HashSet<EnemyComponent> enemiesGone = new HashSet<EnemyComponent>(16);

        private void Awake()
        {
            TowerDefenseRegister.InitializeEnemies();
        }

        private void Start()
        {
            spawn = new Vector3
            (
                SpawnPoint.position.x, 
                EnemyPrefab.transform.position.y, 
                SpawnPoint.position.z
            );
        }

        // Update is called once per frame
        private void Update()
        {
            if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;
            CreateEnemy();
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

        private void CreateEnemy()
        {
            GameObject go = Instantiate(EnemyPrefab, spawn, Quaternion.identity);
            EnemyComponent enemy = go.GetComponent<EnemyComponent>();
            
            enemy.SetDestination(EndPoint.position);
            enemies.Add(enemy);
            this.AddToRegister(enemy);
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
                Destroy(gone.gameObject);
            }
            
            enemies.ExceptWith(enemiesGone);
            this.RemoveFromRegister(enemiesGone);
            
            enemiesGone.Clear();
        }
    }
}
