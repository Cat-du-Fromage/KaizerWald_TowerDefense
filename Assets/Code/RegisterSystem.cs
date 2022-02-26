using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public enum EventType
    {
        Register,
        Unregister,
    }
    
    public static class RegisterEvent
    {
        private static RegisterSystem register;
        public static void InitializeRegister(this RegisterSystem registerSystem) => register = registerSystem;

        public static void Notify(this TurretManager turretManager, BulletComponent bullet, EventType eventType)
        {
            register.TurretNotification(bullet, eventType);
        }

        //ENEMY KILLED / DISAPPEAR
        
        /// <summary>
        /// Notify An enemy despawn by being killed by a turret
        /// Event : Player gain resources
        /// </summary>
        public static void Notify(this BulletManager bulletManager, EnemyComponent enemy)
        {
            register.OnEnemyDespawn(bulletManager, enemy);
        }

        /// <summary>
        /// Notify An enemy despawn by passing through the EndGate
        /// Event : Player take Damage
        /// </summary>
        public static void Notify(this EndGateComponent endGate, EnemyComponent enemy)
        {
            register.OnEnemyDespawn(endGate, enemy);
        }
    }
    
    public class RegisterSystem : MonoBehaviour
    {
        [SerializeField] private UIGameInformation UI;
        
        [SerializeField] private TurretManager Turret;
        [SerializeField] private BulletManager Bullet;
        [SerializeField] private EnemyManager Enemy;

        private void Awake()
        {
            UI     ??= FindObjectOfType<UIGameInformation>();
            Turret ??= FindObjectOfType<TurretManager>();
            Bullet ??= FindObjectOfType<BulletManager>();
            Enemy  ??= FindObjectOfType<EnemyManager>();
            this.InitializeRegister();
        }

        public void OnEnemyDespawn(Component sender, EnemyComponent enemy)
        {
            if (enemy == null) return;
            Enemy.EnemyKilled(enemy.UniqueID);

            if (sender is EndGateComponent)
            {
                UI.TakeDamage();
            }
            else if (sender is BulletManager)
            {
                UI.AddResources(1);
            }
        }
        
        //Turret Notify => Bullet : Add/Remove
        public void TurretNotification(BulletComponent bullet, EventType eventType)
        {
            if (eventType == EventType.Register)
            {
                Bullet.RegisterBullet(bullet);
            }
            else if (eventType == EventType.Unregister)
            {
                Bullet.UnregisterBullet(bullet);
            }
        }
    }
}