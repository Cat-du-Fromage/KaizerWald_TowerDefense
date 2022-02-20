using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using KWUtils;
using Unity.Collections;

namespace TowerDefense
{
    public class TurretManager : MonoBehaviour
    {
        [SerializeField]private AudioClip shootClip;
        
        //Temporary
        [SerializeField] private GameObject BulletPrefab;
        
        private List<TurretComponent> turrets = new List<TurretComponent>(2);
        private Rigidbody[] bullets;
        
        private void Awake()
        {
            TowerDefenseRegister.InitializeTurrets();
        }

        // Update is called once per frame
        private void Update()
        {
            if (turrets.Count == 0) return;
            for (int i = 0; i < turrets.Count; i++)
            {
                turrets[i].TurretUpdate();
            }
        }

        private void LateUpdate()
        {
            if (turrets.Count == 0) return;
            for (int i = 0; i < turrets.Count; i++)
            {
                turrets[i].GetAim();
            }
        }

        private void FixedUpdate()
        {
            if (turrets.Count == 0) return;
            for (int i = 0; i < turrets.Count; i++)
            {
                turrets[i].ShootAt(shootClip);
            }
        }

        public void AddTurret(TurretComponent turret) => turrets.Add(turret);
        
        public void CreateTurret(GameObject turretPrefab, Vector3 position, Quaternion rotation)
        {
            //Objects/Components
            GameObject turretObject = Instantiate(turretPrefab, position, rotation);
            TurretComponent turretComponent = turretObject.GetComponent<TurretComponent>();

            //Initializations triggered
            turretComponent.InitializeBullet(BulletPrefab);
            
            //Registration
            AddTurret(turretComponent);
            this.AddToRegister(turretObject.transform);
        }
    }
}

/*
         private void GetTargetsInRange()
        {
            //return array of transform
        }

        private void GetTargets()
        {
            EnemyComponent[] enemies = new EnemyComponent[TowerDefenseRegister.EnemiesCount];
            TowerDefenseRegister.GetEnemies.CopyTo(enemies);
            
            for (int i = 0; i < turrets.Count; i++)
            {
                FindNearestEnemy(turrets[i], enemies);
            }
        }

        private int FindNearestEnemy(GameObject turret, EnemyComponent[] enemies)
        {
            NativeArray<float> distances = new NativeArray<float>(enemies.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            Vector3 turretPosition = turret.transform.position;
            
            float minDistances = Single.MaxValue;
            int indexClosestEnemy = 0;
            
            for (int i = 0; i < enemies.Length; i++)
            {
                distances[i] = (enemies[i].transform.position - turretPosition).sqrMagnitude;
                
                if (distances[i] > turret.GetComponent<TurretComponent>().GetRange) continue;
                
                if (distances[i] < minDistances)
                {
                    minDistances = distances[i];
                    indexClosestEnemy = i;
                }
            }

            distances.Dispose();
            return indexClosestEnemy;
        }
 */
