using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BulletManager : MonoBehaviour
    {
        private List<BulletComponent> bullets;
        private List<BulletComponent> activeBullets;

        private void Awake()
        {
            bullets = new List<BulletComponent>(2);
        }

        private void Update()
        {
            if (bullets.Count == 0) return;
            
            for (int i = 0; i < bullets.Count; i++)
            {
                if (!bullets[i].Hit) continue;
                
                if (bullets[i].enemyHit != null)
                {
                    OnBulletHitEnemy(bullets[i]);
                    continue;
                }

                OnBulletHitGround(bullets[i]);
            }
        }

        private void OnBulletHitEnemy(BulletComponent bullet)
        {
            this.Notify(bullet.enemyHit);
            bullet.ResetHitValues();
            bullet.Fade();
        }

        private void OnBulletHitGround(BulletComponent bullet)
        {
            bullet.UpdateVelocity();
            bullet.CheckFadeDistance();
            bullet.ResetHitValues();
        }

        public void RegisterBullet(BulletComponent bullet) => bullets.Add(bullet);
        public void UnregisterBullet(BulletComponent bullet) => bullets.Remove(bullet);
    }
}
