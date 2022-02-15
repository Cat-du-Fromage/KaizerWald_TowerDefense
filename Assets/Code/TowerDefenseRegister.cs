using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public static class TowerDefenseRegister
    {
        private static List<GameObject> turrets;
        private static HashSet<EnemyComponent> enemies;

        public static List<GameObject> GetTurrets => turrets;
        public static HashSet<EnemyComponent> GetEnemies => enemies;

        //needed because static values are note reset in editor!
        public static void InitializeTurrets() => turrets = new List<GameObject>(2);
        public static void InitializeEnemies() => enemies = new HashSet<EnemyComponent>(2);

        public static int EnemiesCount => enemies.Count;

        public static void AddToRegister(this Component sender, Component entity)
        {
            if (sender is TurretManager)
            {
                turrets.Add(entity.gameObject);
                Debug.Log($"turret added : {entity.name}");
            }
            else if (sender is EnemyManager)
            {
                enemies.Add(entity as EnemyComponent);
                Debug.Log($"register enemy count : {enemies.Count}");
            }
            else
            {
                Debug.Log($"not eligible for register");
            }
        }
        
        //public static void AddToRegister(EnemyComponent enemy) => enemies.Add(enemy);
        
        public static void RemoveFromRegister(this Component sender, Component entity)
        {
            if (sender is TurretManager)
            {
                turrets.Remove(entity.gameObject);
            }
            else if (sender is EnemyManager)
            {
                enemies.Remove(entity as EnemyComponent);
            }
            else
            {
                Debug.Log($"not eligible for register");
            }
        }
        
        public static void RemoveFromRegister(this EnemyManager enemyManager, HashSet<EnemyComponent> entities)
        {
            enemies.ExceptWith(entities);
            Debug.Log($"register enemy count : {enemies.Count}");
        }
    }
}
