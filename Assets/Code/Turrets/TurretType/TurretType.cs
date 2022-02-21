using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    [CreateAssetMenu(fileName = "New Turret", menuName = "Turret")]
    public class TurretType : ScriptableObject
    {
        public ParticleSystem MuzzleFlash;
        public Quaternion OffsetRotation;
        public float ReloadTime = 1f;

        public Vector3 Forward;
        private void OnEnable()
        {
            
        }
    }
}
