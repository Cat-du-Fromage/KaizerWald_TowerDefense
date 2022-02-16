using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TowerDefense
{
    public class UIBuilding : MonoBehaviour
    {
        [SerializeField] private GameObject[] TurretsBlueprint;

        [SerializeField] private Button[] buttons;
        
        [SerializeField] private BuildManager BuildManager;

        private void Awake()
        {
            buttons = GetComponentsInChildren<Button>();
            BuildManager = BuildManager == null ? FindObjectOfType<BuildManager>() : BuildManager;
        }

        private void Start()
        {
            InitializeBlueprints();
            EnableButtons();
        }

        private void OnDestroy()
        {
            DisableButtons();
        }

        //MAY WANT TO SWITCH FOR INPUT SYSTEM!
        private void Update()
        {
            if (!BuildManager.IsBuilding) return;
            if (Keyboard.current.escapeKey.wasReleasedThisFrame || Keyboard.current.qKey.wasReleasedThisFrame)
            {
                BuildManager.ToggleBuildMode();
                DisableAllBlueprint();
            }
        }

        private void OnBuildTurretButton(GameObject blueprint)
        {
            DisableAllBlueprint();
            BuildManager.ToggleBuildMode(blueprint);
            blueprint.SetActive(true);
        }

        private void EnableButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                GameObject blueprint = TurretsBlueprint[i];
                buttons[i].onClick.AddListener(new UnityAction(() => OnBuildTurretButton(blueprint)));
            }
        }

        private void DisableButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }

        private void InitializeBlueprints()
        {
            if (buttons.Length == 0)
            {
                buttons = GetComponentsInChildren<Button>();
            }
            
            TurretsBlueprint = new GameObject[buttons.Length];
            if (buttons.Length != 0)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    GameObject blueprint = Instantiate(buttons[i].GetComponent<ButtonTurretReference>().GetBlueprint());
                    TurretsBlueprint[i] = blueprint;
                    blueprint.SetActive(false);
                }
            }
        }

        private void DisableAllBlueprint()
        {
            for (int i = 0; i < TurretsBlueprint.Length; i++)
            {
                TurretsBlueprint[i].SetActive(false);
            }
        }
    }
}
