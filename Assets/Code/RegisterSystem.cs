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

        public static void Notify(this BulletManager bulletManager, EnemyComponent enemy)
        {
            register.BulletNotification(enemy);
        }

    }
    
    public class RegisterSystem : MonoBehaviour
    {
        [SerializeField] private TurretManager Turret;
        [SerializeField] private BulletManager Bullet;
        [SerializeField] private EnemyManager Enemy;

        private void Awake()
        {
            Turret ??= FindObjectOfType<TurretManager>();
            Bullet ??= FindObjectOfType<BulletManager>();
            Enemy ??= FindObjectOfType<EnemyManager>();
            this.InitializeRegister();
        }

        //Bullet Notify => Enemy : remove(EnemyComponent)
        public void BulletNotification(EnemyComponent enemy)
        {
            Enemy.EnemyKilled(enemy);
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