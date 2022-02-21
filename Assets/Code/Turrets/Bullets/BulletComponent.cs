using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BulletComponent : MonoBehaviour
    {
        public bool Hit;
        
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

        public void Shoot(Vector3 direction)
        {
            trail.emitting = true;
            
            bulletRigidBody.velocity = direction * 3;
            bulletRigidBody.AddForce(bulletRigidBody.velocity * 1f, ForceMode.Impulse);
        }
        
        public void Fade()
        {
            trail.emitting = false;
            
            bulletRigidBody.velocity = Vector3.zero;
            transform.position = initialPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer is 10 or 8)
            {
                Hit = true;
                other.TryGetComponent(out enemyHit);
                Fade();
            }
        }

        
    }
}
