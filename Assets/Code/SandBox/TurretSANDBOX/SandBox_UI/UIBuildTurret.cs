using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using KWUtils;

namespace TowerDefense
{
    public class UIBuildTurret : MonoBehaviour
    {
        [SerializeField] private GameObject[] TurretsBlueprint;
        
        [SerializeField] private Button BasicTurretBuild;
        
        [SerializeField] private SandBox_GridManager gridManager;

        private void Awake()
        {
            //gridManager
            gridManager = gridManager == null ? FindObjectOfType<SandBox_GridManager>() : gridManager;
            BasicTurretBuild = BasicTurretBuild == null ? gameObject.GetComponentInChildrenFrom<TagBasicTurret, Button>() : BasicTurretBuild;
        }

        private void Start()
        {
            BasicTurretBuild.onClick.AddListener(OnBasicTurretClick);
        }

        private void OnDestroy()
        {
            BasicTurretBuild.onClick.RemoveListener(OnBasicTurretClick);
        }

        private void OnBasicTurretClick() => gridManager.ToggleBlueprint();
    }
}