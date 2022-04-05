using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static KWUtils.KWmath;

namespace TowerDefense
{
    public struct BulletData
    {
        //Initialization when out of the Pool
        public bool IsLoaded;
        public float MaxRange;
        public Vector3 StartPosition;
    }
    public class BulletComponent : MonoBehaviour
    {
        public BulletData data;
        
        [SerializeField] private float muzzleVelocity = 10f;
        private float velocity = 5f;

        public bool Hit;
        
        //Stored Values
        public Vector3 StartPosition;
        private Rigidbody bulletRigidBody;
        
        private TrailRenderer trail;

        public EnemyComponent enemyHit;

        private void Awake()
        {
            data = new BulletData();
            
            StartPosition = transform.position;
            bulletRigidBody = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();
        }

        public void CheckFadeDistance()
        {
            if ((transform.position - StartPosition).sqrMagnitude > 1024f)
            {
                Fade();
            }
        }

        public void Fade()
        {
            bulletRigidBody.useGravity = false;
            trail.emitting = false;
            
            bulletRigidBody.velocity = Vector3.zero;
            transform.position = StartPosition;
            ResetHitValues();
        }

        public void ResetHitValues()
        {
            Hit = false;
            enemyHit = null;
        }

        public void UpdateVelocity() => bulletRigidBody.velocity /= 2f;



        //==============================================================================================================
        //EXTERNAL CALL
        //==============================================================================================================

        public void LoadBullet(in Vector3 startPosition, float turretRange)
        {
            data.MaxRange = Sq(turretRange * 1.5f);
            data.StartPosition = startPosition;
            data.IsLoaded = true;
        }
        
        public void Shoot(Vector3 direction)
        {
            trail.emitting = true;

            bulletRigidBody.velocity = direction * velocity;

            bulletRigidBody.useGravity = true;
            
            bulletRigidBody.AddForce(bulletRigidBody.velocity * muzzleVelocity, ForceMode.Impulse);
        }
        

        private void OnCollisionEnter(Collision other)
        {
            Hit = other.gameObject.layer is 10 or 8;
            other.transform.TryGetComponent(out enemyHit);
        }
    }
}
