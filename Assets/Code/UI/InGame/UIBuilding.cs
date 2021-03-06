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

        public event Action<GameObject> OnBuildCommand;
        
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
            //=======================================
            //TODO : REPLACE THIS WITH INPUT SYSTEM
            //=======================================
            if (!BuildManager.IsBuilding) return;
            if (Keyboard.current.escapeKey.wasReleasedThisFrame || Keyboard.current.qKey.wasReleasedThisFrame)
            {
                BuildManager.ToggleBuildModeOff();
                DisableAllBlueprint();
            }
        }
        
        /// <summary>
        /// Assign all buttons to a turret's Blueprint (GameObject)
        /// </summary>
        private void EnableButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                GameObject blueprint = TurretsBlueprint[i];
                buttons[i].onClick.AddListener(new UnityAction(() => OnBuildTurretButton(blueprint)));
            }
        }
        
        /// <summary>
        /// Remove All Listeners on Buttons
        /// </summary>
        private void DisableButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// Assign all buttons to a turret's Blueprint (GameObject)
        /// </summary>
        private void OnBuildTurretButton(GameObject blueprint)
        {
            DisableAllBlueprint();
            OnBuildCommand?.Invoke(blueprint);
            //BuildManager.ToggleBuildModeOn(blueprint);
            blueprint.SetActive(true);
        }
        
        /// <summary>
        /// Assign all buttons to a turret's Blueprint (GameObject)
        /// </summary>
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

        /// <summary>
        /// Disable all Blueprint
        /// </summary>
        private void DisableAllBlueprint()
        {
            for (int i = 0; i < TurretsBlueprint.Length; i++)
            {
                TurretsBlueprint[i].SetActive(false);
            }
        }
    }
}
