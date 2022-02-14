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
        
        private Button[] buttons;
        
        [SerializeField] private SandBox_GridManager gridManager;

        private void Awake()
        {
            //gridManager
            gridManager = gridManager == null ? FindObjectOfType<SandBox_GridManager>() : gridManager;

            GetButtonsReference();
            
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

        private void GetButtonsReference()
        {
            ButtonTurretReference[] buttonsRef = GetComponentsInChildren<ButtonTurretReference>(true);
            
            TurretsBlueprint = new GameObject[buttonsRef.Length];
            buttons =new Button[buttonsRef.Length];
            
            for (int i = 0; i < buttonsRef.Length; i++)
            {
                TurretsBlueprint[i] = buttonsRef[i].GetBlueprint();
            }
        }
    }
}