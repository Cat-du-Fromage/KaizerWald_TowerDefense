using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KWUtils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

//Disable warning for async demanding await where not needed..
//#pragma warning disable CS4014

namespace TowerDefense
{
    public class TurretComponent : MonoBehaviour
    {
        [SerializeField] private TurretType Turret;

        //Rotation
        private Quaternion BaseRotation;

        //Transform buffer
        private Transform turretTransform;
        public Transform currentTarget;

        //Target check
        private bool targetFound;
        private bool isReloading;
        private Collider[] colliders;
        
        //Shooting variables
        private float randomOffset;
        private Vector3 shootDirection;

        //Effect On Shoot
        private AudioSource shootSound;
        [SerializeField] private ParticleSystem MuzzleFlash;
        
        //Bullets
        private BulletComponent bullet;
        [SerializeField] private Transform BulletPositionInTurret;
        private Vector3 GetBulletPosition => BulletPositionInTurret.position;

        //Convenient helper method
        private bool IsCurrentTargetInRange => currentTarget.position.DistanceTo(TurretPosition) <= Turret.Range;
        private Quaternion TurretRotation => turretTransform.rotation;
        private Vector3 TurretPosition => turretTransform.position;
        private Vector3 TargetPosition => currentTarget.position;
        
        private Vector3 TargetShootPosition => currentTarget.position + Vector3.up * 0.75f;

        private CancellationTokenSource soundToken;

        //TESTING
        public bool IsPlayingSound;

        private void Awake()
        {
            if(MuzzleFlash != null) MuzzleFlash.Stop(true);
            
            shootSound = GetComponent<AudioSource>();
            
            turretTransform = transform;
            BaseRotation = turretTransform.rotation;
            
            colliders = new Collider[10];
            soundToken = new CancellationTokenSource();
        }

        private void Start()
        {
            Turret.SetForwardAxis();
        }

        private void OnDestroy()
        {
            soundToken.Cancel();
        }

        //Update For turrets with no Targets
        public bool NoTargetUpdate()
        {
            IsPlayingSound = shootSound.isPlaying;
            //Find target by collision
            if (OverlapSphereNonAlloc(turretTransform.position, Turret.Range, colliders, 1<<10) != 0)
            {
                if (CheckIfValidTarget())
                {
                    EnableAudioSource();
                    return true;
                };
            }
            
            //return turret to it's normal rotation
            if (turretTransform.rotation != BaseRotation)
            {
                ToBaseRotation();
            }
            return false;
        }

        public bool WithTargetUpdate()
        {
            if (currentTarget == null)
            {
                DisableAudioSource(soundToken.Token).Forget();
                return true;
            }
            
            if (!IsCurrentTargetInRange && currentTarget != null)
            {
                DisableAudioSource(soundToken.Token).Forget();
                currentTarget = null;
                return true;
            }
            RotateTowardsTarget();
            return false;
        }
        
        //==============================================================================================================
        //SHOOTING STEPS
        //==============================================================================================================

        public void GetAim()
        {
            if (currentTarget == null) return;
            shootDirection = (TargetShootPosition - GetBulletPosition).normalized;
            randomOffset = max(Random.value, 0.05f);
        }

        public void ShootAt(AudioClip clip)
        {
            if (currentTarget == null || isReloading) return;
            
            shootSound.PlayOneShot(clip, randomOffset);
            bullet.Shoot(shootDirection);
            
            if (MuzzleFlash != null)
            {
                MuzzleFlash.Play(true);
            }
            
            Turret.CurrentReload = randomOffset + Turret.ReloadTime;
            Reload().Forget();
        }
        
        private async UniTaskVoid Reload()
        {
            isReloading = true;
            await UniTask.Delay(TimeSpan.FromSeconds(Turret.CurrentReload));
            bullet.Fade();
            isReloading = false;
        }

        //==============================================================================================================
        //Audio Behaviour
        //==============================================================================================================

        private void EnableAudioSource() => shootSound.enabled = true;

        private async UniTaskVoid DisableAudioSource(CancellationToken token)
        {
            while (shootSound.isPlaying && !token.IsCancellationRequested)
            {
                await UniTask.Yield(token);
            }
        }
        
        public BulletComponent InitializeBullet(GameObject bulletPrefab)
        {
            bullet = Instantiate(bulletPrefab, GetBulletPosition, Quaternion.identity).GetComponent<BulletComponent>();
            return bullet;
        }
        
        //==============================================================================================================
        //SEARCH TARGET
        //==============================================================================================================
        
        private bool CheckIfValidTarget()
        {
            Vector3 forward = turretTransform.TransformDirection(Turret.ForwardAxis);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null) continue;
                
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
        private void ToBaseRotation() => turretTransform.rotation = Quaternion.Lerp(TurretRotation, BaseRotation, Turret.RotationSpeed * Time.deltaTime);
        
        private void RotateTowardsTarget()
        {
            Vector3 direction = (TargetPosition - TurretPosition).Flat();
            Quaternion rotation = Quaternion.LookRotation(direction) * Turret.OffsetRotation;
            turretTransform.rotation = Quaternion.Lerp(TurretRotation, rotation, Turret.RotationSpeed * Time.deltaTime);
        }
        
#if UNITY_EDITOR
        /*
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
        */
#endif
    }
}
