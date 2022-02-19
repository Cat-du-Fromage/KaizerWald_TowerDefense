using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Physics;

namespace TowerDefense
{
    public class TurretComponent : MonoBehaviour
    {
        //May want to make scriptable object for this
        //or enum to differenciate range by turret type
        [SerializeField] private float range = 16;
        
        private Quaternion BaseRotation;
        private Quaternion offsetRotation;
        
        private Transform turretTransform;
        public Transform currentTarget;

        private Collider[] colliders;
        private bool targetFound;

        //Convenient helper method
        private bool IsCurrentTargetInRange => currentTarget.position.DistanceTo(TurretPosition) <= range;
        private Quaternion TurretRotation => turretTransform.rotation;
        private Vector3 TurretPosition => turretTransform.position;

        private void Awake()
        {
            BaseRotation = transform.rotation;
            offsetRotation = Quaternion.Euler(0, 90, 0);
            colliders = new Collider[10];
            turretTransform = transform;
        }

        public void TurretUpdate()
        {
            //Behaviour when turret has a target
            if (currentTarget != null)
            {
                if (!IsCurrentTargetInRange)
                {
                    currentTarget = null;
                }
                else
                {
                    RotateTowardsTarget();
                    return;
                }
            }
            
            //Find target by collision
            targetFound = false;
            if (OverlapSphereNonAlloc(turretTransform.position, range, colliders, 1<<10) != 0)
            {
                targetFound = CheckIfValidTarget();
            }

            //return turret to it's normal rotation
            if (targetFound || turretTransform.rotation == BaseRotation) return;
            ToBaseRotation();
        }
        
        private bool CheckIfValidTarget()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null) continue;
                
                Vector3 forward = turretTransform.TransformDirection(Vector3.left);
                Vector3 toOther = (colliders[i].transform.position - TurretPosition);
                
                if (!(math.dot(forward, toOther) > 0)) continue;
                currentTarget = colliders[i].transform;
                return true;
            }
            return false;
        }
        
        
        //==============================================================================================================
        //ROTATION BEHAVIOUR
        //==============================================================================================================
        private void ToBaseRotation() => turretTransform.rotation = Quaternion.Lerp(TurretRotation, BaseRotation, 2 * Time.deltaTime);
        
        private void RotateTowardsTarget()
        {
            Vector3 direction = currentTarget.position - TurretPosition;
            Quaternion rotation = Quaternion.LookRotation(direction.Flat()) * offsetRotation;
            turretTransform.rotation = Quaternion.Lerp(TurretRotation, rotation, 2 * Time.deltaTime);
        }
    }
}
