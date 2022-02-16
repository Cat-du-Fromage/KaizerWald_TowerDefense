using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KWUtils;

namespace TowerDefense
{
    public class EnemyComponent : MonoBehaviour, ISpatialEntity
    {
        //SPATIAL PARTITION
        public uint EntityIndex { get; set; }
        public uint CurrentCellIndex { get; set; }
        public uint PreviousCellIndex { get; set; }
        public Vector3 Position { get; set; }
        
        //ANIMATION
        //=====================================
        // animation IDs
        private int animIDSpeed;
        private int animIDMotionSpeed;

        private Animator animator;
        
        //Acceleration and deceleration
        private float speedChangeRate = 10.0f;
        private float animationBlend;
        //=====================================
        
        private Transform enemyTransform;
        private Vector3 destination = Vector3.zero;
        [SerializeField] private float speed = 10.0f;

        private Rigidbody rigidBody;
        private void Awake()
        {
            enemyTransform = transform;
            rigidBody = GetComponent<Rigidbody>();
            TryGetComponent(out animator);
        }

        private void Start()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        public void SetMove()
        {
            Vector3 test = Vector3.MoveTowards(enemyTransform.position, destination, speed * Time.deltaTime);
            rigidBody.MovePosition(test);
            AnimationSpeed();
        }

        public bool CheckDestinationEnd()
        {
            return (destination - enemyTransform.position).sqrMagnitude <= 2;
        }

        public void SetDestination(Vector3 point)
        {
            destination = point;
        }

        /// <summary>
        /// VERY rough implementation of an animation
        /// </summary>
        private void AnimationSpeed()
        {
            animationBlend = Mathf.Lerp(animationBlend, speed, Time.deltaTime * speedChangeRate);
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, 1);
        }

        
    }
}
