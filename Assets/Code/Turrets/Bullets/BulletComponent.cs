using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BulletComponent : MonoBehaviour
    {
        [SerializeField] private float muzzleVelocity = 10f;
        private float velocity = 5f;
        
        private bool isShoot;
        public bool Hit;
        
        //Stored Values
        private Vector3 initialPosition;
        private Rigidbody bulletRigidBody;
        private TrailRenderer trail;

        public EnemyComponent enemyHit;

        private void Awake()
        {
            initialPosition = transform.position;
            bulletRigidBody = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();
        }

        public void CheckFadeDistance()
        {
            if ((transform.position - initialPosition).sqrMagnitude > 1024f)
            {
                Fade();
            }
        }

        public void Fade()
        {
            bulletRigidBody.useGravity = false;
            trail.emitting = false;
            
            bulletRigidBody.velocity = Vector3.zero;
            transform.position = initialPosition;
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
        
        public void Shoot(Vector3 direction)
        {
            isShoot = true;
            
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
