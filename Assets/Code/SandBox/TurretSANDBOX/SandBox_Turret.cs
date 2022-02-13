using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;
using UnityEngine;
using static TowerDefense.TowerDefenseUtils;
using static KWUtils.InputSystemExtension;

namespace TowerDefense
{
    public class SandBox_Turret : MonoBehaviour
    {
        //PrefabTurret
        [SerializeField] private GameObject FovPrefab;
        [SerializeField] private SandBox_FieldOfView FieldOfView;

        private void Awake()
        {
            GameObject fov = Instantiate(FovPrefab, transform.position, transform.rotation);
            FieldOfView = fov.GetComponent<SandBox_FieldOfView>();
            fov.transform.position = fov.transform.position.SetAxis(Axis.Y, 1);
        }

        private void Start()
        {
            FieldOfView.GetFieldOfView();
        }
        
    }
}
