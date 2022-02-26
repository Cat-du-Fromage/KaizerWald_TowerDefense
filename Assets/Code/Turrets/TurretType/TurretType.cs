using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense
{
    [CreateAssetMenu(fileName = "New Turret", menuName = "Turret")]
    public class TurretType : ScriptableObject
    {
        public GameObject BulletPrefab;
        public float Range = 4;
        public float RotationSpeed = 2f;
        public float ReloadTime = 1f;
        public float CurrentReload = 1f;
        public float OffsetYRotation;
        public Vector3 ForwardAxis;
        public Quaternion OffsetRotation;

        private void Awake()
        {
            Debug.Log("AWAKE SO");
        }

        public void SetForwardAxis()
        {
            ForwardAxis = OffsetYRotation switch
            {
                90f => Vector3.left,
                180f => Vector3.back,
                270f => Vector3.right,
                _ => Vector3.forward
            };
        }
    }
}
