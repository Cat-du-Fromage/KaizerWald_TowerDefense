using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;

namespace TowerDefense
{
    public class TurretComponent : MonoBehaviour
    {
        [SerializeField] private TurretType Turret;
        
        //May want to make scriptable object for this
        //or enum to differenciate range by turret type
        [SerializeField] private float range = 16; //scriptable object
        
        //Rotation
        private Quaternion BaseRotation;
        private Quaternion offsetRotation; //scriptable object
        
        //Transform buffer
        private Transform turretTransform;
        public Transform currentTarget;

        //Target check
        private Collider[] colliders;
        private bool targetFound;

        private float reloadTime = 2f; //scriptable object
        private bool isReloading;

        private AudioSource shootSound; //scriptable object
        [SerializeField] private ParticleSystem MuzzleFlash; //scriptable object
        
        //Bullets
        private BulletComponent bullet;
        [SerializeField] private Transform BulletPosition;
        public Vector3 GetBulletPosition => BulletPosition.position;

        //Convenient helper method
        private bool IsCurrentTargetInRange => currentTarget.position.DistanceTo(TurretPosition) <= range;
        private Quaternion TurretRotation => turretTransform.rotation;
        private Vector3 TurretPosition => turretTransform.position;
        private Vector3 TargetPosition => currentTarget.position;
        
        private Vector3 TargetShootPosition => currentTarget.position + Vector3.up;

        private Vector3 shootDirection;

        private void Awake()
        {
            if(MuzzleFlash != null) MuzzleFlash.Stop(true);
            shootSound = GetComponent<AudioSource>();
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

        public void GetAim()
        {
            if (currentTarget == null) return;
            shootDirection = (TargetShootPosition - GetBulletPosition).normalized;
        }

        public void ShootAt(AudioClip clip)
        {
            if (currentTarget == null || isReloading) return;
            
            shootSound.PlayOneShot(clip, UnityEngine.Random.value);
            bullet.Shoot(shootDirection);
            
            if (MuzzleFlash != null)
            {
                MuzzleFlash.Play(true);
            }
            
            StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            isReloading = true;
            float reload = UnityEngine.Random.value;
            yield return new WaitForSeconds(reloadTime+reload);
            bullet.Fade();
            isReloading = false;
        }
        
        
        
        //==============================================================================================================
        //SHOOT BEHAVIOUR
        //==============================================================================================================

        public void InitializeBullet(GameObject bulletPrefab)
        {
            bullet = Instantiate(bulletPrefab, GetBulletPosition, Quaternion.identity).GetComponent<BulletComponent>();
        }
        
        //==============================================================================================================
        //SEARCH TARGET
        //==============================================================================================================
        
        private bool CheckIfValidTarget()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null) continue;
                
                Vector3 forward = turretTransform.TransformDirection(Vector3.left);
                Vector3 toOther = (colliders[i].transform.position - TurretPosition);
                
                if (!(dot(forward, toOther) > 0)) continue;
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
        
#if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            return;
        }

        private void DebugForwardTurret()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(turretTransform.position, turretTransform.forward * 10);
        }

        private void DebugTargetShoot()
        {
            if (currentTarget == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetBulletPosition, 0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(TargetShootPosition, 0.2f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(GetBulletPosition, TargetShootPosition);
        }
#endif
    }
}
