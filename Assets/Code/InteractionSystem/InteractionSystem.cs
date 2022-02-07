using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTCamera;
using UnityEngine;
using KWUtils;

namespace TowerDefense
{
    public class InteractionSystem : MonoBehaviour
    {
        public Camera PlayerCamera;
        public InteractionInputs InputsControl;

        [SerializeField] private SelectionSubSystem selectionSubSystem;

        [SerializeField] private Transform characterSelection;
        private SelectionTag characterSelectTag;
        
        private void Awake()
        {
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;

            characterSelection = characterSelection == null ? GameObject.Find("PlayerCharacter").transform : characterSelection;
            
            InputsControl ??= GetComponent<InteractionInputs>();
            
            selectionSubSystem ??= GetComponent<SelectionSubSystem>();
        }

        private void Start()
        {
            characterSelectTag = characterSelection.GetComponent<SelectionTag>();
        }

        public void SelectionNotification(Transform selection = null)
        {
            characterSelectTag.ToggleVisible(selection!=null);
        }
    }
}
