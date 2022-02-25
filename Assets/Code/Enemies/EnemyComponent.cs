using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KWUtils;

namespace TowerDefense
{
    public class EnemyComponent : MonoBehaviour
    {
        [SerializeField] private float speed = 10.0f;
        
        private int uniqueId;
        
        //ANIMATION
        //=====================================
        
        // animation IDs
        private int animIDSpeed;
        private int animIDMotionSpeed;

        private Animator animator;
        
        //Acceleration and deceleration
        private float animationBlend;
        
        //=====================================
        public int UniqueID => uniqueId;

        private void Awake()
        {
            uniqueId = gameObject.GetInstanceID();
            TryGetComponent(out animator);
        }

        private void Start()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            
            AnimationSpeed();
        }
        

        /// <summary>
        /// VERY rough implementation of an animation
        /// </summary>
        private void AnimationSpeed()
        {
            animator.SetFloat(animIDSpeed, speed);
            animator.SetFloat(animIDMotionSpeed, 1);
        }
    }
}
