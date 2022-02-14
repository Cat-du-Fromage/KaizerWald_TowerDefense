using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KWUtils;

namespace TowerDefense
{
    public class SandBox_TurretManager : MonoBehaviour
    {
        private List<SandBox_Turret> turrets;
        
        [SerializeField] private GameObject FovPrefab;
        [SerializeField] private SandBox_FieldOfView FieldOfView;

        private void Awake()
        {
            turrets = new List<SandBox_Turret>(2);
        }

        public void CreateTurret(GameObject turretPrefab, Vector3 position, Quaternion rotation)
        {
            GameObject turret = Instantiate(turretPrefab, position.SetAxis(Axis.Y, -1), rotation);
            turrets.Add(turret.GetComponent<SandBox_Turret>());
            
            //GameObject fov = Instantiate(FovPrefab, position, transform.rotation);
        }
    }
}
