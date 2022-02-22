using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using KWUtils;
using Unity.Collections;

namespace TowerDefense
{
    public class TurretManager : MonoBehaviour
    {
        [SerializeField]private AudioClip shootClip;
        
        //Temporary
        [SerializeField] private GameObject BulletPrefab;
        
        //private List<TurretComponent> turrets = new List<TurretComponent>(2);

        //TEST
        public List<TurretComponent> noTargetTurrets;
        public List<TurretComponent> withTargetTurrets;
        
        private void Awake()
        {
            noTargetTurrets = new List<TurretComponent>(2);
            withTargetTurrets = new List<TurretComponent>(2);
        }

        // Update is called once per frame
        private void Update()
        {
            UpdateInactiveTurret();
            UpdateActiveTurret();
        }

        private void LateUpdate()
        {
            if (withTargetTurrets.Count == 0) return;
            for (int i = 0; i < withTargetTurrets.Count; i++)
            {
                withTargetTurrets[i].GetAim();
            }
        }
//CAREFULE! MUST INTRODUCE ANTICIPATION
//When enemies are faster, the physics is too slow to shoot in time!
        private void FixedUpdate()
        {
            if (withTargetTurrets.Count == 0) return;
            for (int i = 0; i < withTargetTurrets.Count; i++)
            {
                withTargetTurrets[i].ShootAt(shootClip);
            }
        }

        private void UpdateActiveTurret()
        {
            if (withTargetTurrets.Count == 0) return;
            for (int i = 0; i < withTargetTurrets.Count; i++)
            {
                if(!withTargetTurrets[i].WithTargetUpdate()) continue;
                
                noTargetTurrets.Add(withTargetTurrets[i]);
                withTargetTurrets.Remove(withTargetTurrets[i]);
            }
        }

        private void UpdateInactiveTurret()
        {
            if (noTargetTurrets.Count == 0) return;
            for (int i = 0; i < noTargetTurrets.Count; i++)
            {
                if(!noTargetTurrets[i].NoTargetUpdate()) continue;
                
                withTargetTurrets.Add(noTargetTurrets[i]);
                noTargetTurrets.Remove(noTargetTurrets[i]);
            }
        }
        
        

        public void CreateTurret(GameObject turretPrefab, Vector3 position, Quaternion rotation)
        {
            //Objects/Components
            GameObject turretObject = Instantiate(turretPrefab, position, rotation);
            TurretComponent turretComponent = turretObject.GetComponent<TurretComponent>();

            //Initializations triggered
            BulletComponent newBullet = turretComponent.InitializeBullet(BulletPrefab);
            this.Notify(newBullet, EventType.Register);
            
            //Registration
            noTargetTurrets.Add(turretComponent);
        }
    }
}
