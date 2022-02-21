using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
namespace TowerDefense
{
    public static class TowerDefenseRegister
    {
        private static List<GameObject> turrets;
        private static HashSet<EnemyComponent> enemies;

        private static Dictionary<EnemyComponent, List<TurretComponent>> turretsTarget;

        public static List<GameObject> GetTurrets => turrets;
        public static HashSet<EnemyComponent> GetEnemies => enemies;

        //needed because static values are note reset in editor!
        public static void InitializeTurretsTarget() => turretsTarget = new Dictionary<EnemyComponent, List<TurretComponent>>(2);
        public static void InitializeTurrets() => turrets = new List<GameObject>(2);
        public static void InitializeEnemies() => enemies = new HashSet<EnemyComponent>(2);

        public static int EnemiesCount => enemies.Count;

        public static void AddTurretsTarget(EnemyComponent enemy, TurretComponent turret)
        {
            turretsTarget[enemy].Add(turret);
        }
        
        public static void RemoveTurretsTarget(EnemyComponent enemy)
        {
            turretsTarget.Remove(enemy);
        }

        public static void AddToRegister(this Component sender, Component entity)
        {
            if (sender is TurretManager)
            {
                turrets.Add(entity.gameObject);
            }
            else if (sender is EnemyManager)
            {
                enemies.Add(entity as EnemyComponent);
                turretsTarget.TryAdd(entity as EnemyComponent, new List<TurretComponent>(2));
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
        }
    }
}
*/