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

        [SerializeField] private SelectionSubSystem selectionSubSystem;
        
        [SerializeField] private Transform characterSelection;

        private SelectionTag characterSelectTag;
        private List<SelectionTag> turretSelectionTags = new List<SelectionTag>();
        
        //We need to get All Friendly Transforms from Major System!

        private void Awake()
        {
            PlayerCamera = PlayerCamera == null ? Camera.main : PlayerCamera;

            characterSelection = characterSelection == null ? GameObject.Find("PlayerCharacter").transform : characterSelection;

            selectionSubSystem ??= GetComponent<SelectionSubSystem>();
        }

        private void Start()
        {
            characterSelectTag = characterSelection.GetComponent<SelectionTag>();
        }

        public void OnInputCallback(InputEventType eventType)
        {
            if (eventType == InputEventType.Move)
            {
                selectionSubSystem.OnMoveMouse();
            }
            else if (eventType == InputEventType.Release)
            {
                selectionSubSystem.OnLeftClickRelease(); //Pass as parameter all friendly entities!
            }
            else
            {
                selectionSubSystem.OnStartLeftClick();
            }
        }

        public void SelectionNotification(Transform selection = null)
        {
            characterSelectTag.ToggleVisible(selection!=null);
            //Notify Major System => character selected (true/false)
        }
        
        public void SelectionNotification(HashSet<Transform> selections)
        {
            if (selections.Contains(characterSelection))
            {
                
            }
            //characterSelectTag.ToggleVisible(selection!=null);
            //Notify Major System => character selected (true/false)
        }
    }
}
