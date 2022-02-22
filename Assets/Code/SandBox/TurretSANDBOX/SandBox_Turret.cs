using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;
using static TowerDefense.TowerDefenseUtils;
using static KWUtils.InputSystemExtension;

namespace TowerDefense
{
    public class SandBox_Turret : MonoBehaviour
    {
        //DOUBLE REFERENCE WITH TURRET!!!!!!
        //[SerializeField] private EnemySpawnManager EnemiesManager;
        
        private List<Transform> enemies;

        //PrefabTurret
        [SerializeField] private GameObject FovPrefab;
        [SerializeField] private SandBox_FieldOfView FieldOfView;

        private void Awake()
        {
            GameObject fov = Instantiate(FovPrefab, transform.position, transform.rotation);
            FieldOfView = fov.GetComponent<SandBox_FieldOfView>();
            fov.transform.position = fov.transform.position.SetAxis(Axis.Y, 1);

            enemies = new List<Transform>(10);
        }

        private void Start()
        {
            FieldOfView.GetFieldOfView();
        }

        public void OnEnemySpawn(Transform enemy) => enemies.Add(enemy);
        public void OnEnemyDestroy(Transform enemy) => enemies.Remove(enemy);

        public void GetEnemiesInRange()
        {
            
        }
    }
}
