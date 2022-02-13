using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform SpawnPoint;
        [SerializeField] private Transform EndPoint;
        [SerializeField] private GameObject EnemyPrefab;

        private Vector3 spawn;

        private List<EnemyComponent> enemies = new List<EnemyComponent>(10);

        private Queue<EnemyComponent> enemiesGone = new Queue<EnemyComponent>(10);
        
        private EnemyComponent ToRemove = null;
        // Start is called before the first frame update
        void Start()
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
            
            GameObject go = Instantiate(EnemyPrefab, spawn, Quaternion.identity);
            enemies.Add(go.GetComponent<EnemyComponent>());
            go.GetComponent<EnemyComponent>().SetDestination(EndPoint.position);
        }

        private void FixedUpdate()
        {
            if (enemies.Count == 0) return;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].SetMove();
                
                if (enemies[i].CheckDestinationEnd())
                {
                    enemiesGone.Enqueue(enemies[i]);
                }
            }
        }

        private void LateUpdate()
        {
            if (enemiesGone.Count == 0) return;
            
            for (int i = 0; i < enemiesGone.Count; i++)
            {
                ToRemove = enemiesGone.Dequeue();
                enemies.Remove(ToRemove);
                Destroy(ToRemove.gameObject);
                ToRemove = null;
            }
        }
    }
}
