using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BulletManager : MonoBehaviour
    {
        private List<BulletComponent> bullets;

        private void Awake()
        {
            bullets = new List<BulletComponent>(2);
        }

        private void Update()
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                if (!bullets[i].Hit) continue;
                
                this.Notify(bullets[i].enemyHit);
                bullets[i].ResetValues();
            }
        }

        public void RegisterBullet(BulletComponent bullet) => bullets.Add(bullet);
        public void UnregisterBullet(BulletComponent bullet) => bullets.Remove(bullet);
    }
}
