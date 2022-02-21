using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BulletComponent : MonoBehaviour
    {
        private Vector3 initialPosition;
        private Rigidbody bulletRigidBody;

        private void Awake()
        {
            initialPosition = transform.position;
            bulletRigidBody = GetComponent<Rigidbody>();
        }

        public void Shoot(Vector3 direction)
        {
            bulletRigidBody.velocity = direction * 3;
            bulletRigidBody.AddForce(bulletRigidBody.velocity * 1f, ForceMode.Impulse);
        }
        
        public void Fade()
        {
            bulletRigidBody.velocity = Vector3.zero;
            transform.position = initialPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 10)
            {
                Fade();
            }
        }

        
    }
}
