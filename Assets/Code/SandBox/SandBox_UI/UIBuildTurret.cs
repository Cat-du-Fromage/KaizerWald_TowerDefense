using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TowerDefense
{
    public class UIBuildTurret : MonoBehaviour
    {
        [SerializeField] private SandBox_GridManager gridManager;
        [SerializeField] private GameObject uiCanvas;
        private GraphicRaycaster uiRaycaster;
        
        private PointerEventData clickData;
        private readonly List<RaycastResult> clickResults = new List<RaycastResult>();

        private void Awake()
        {
            uiCanvas = gameObject;
        }

        private void Start()
        {
            uiRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
            clickData = new PointerEventData(EventSystem.current);
        }
        
        private void Update()
        {
            // use isPressed if you wish to ray cast every frame:
            //if(Mouse.current.leftButton.isPressed)
        
            // use wasReleasedThisFrame if you wish to ray cast just once per click:
            if (!Mouse.current.leftButton.wasReleasedThisFrame) return;
            GetUiElementsClicked();
        }
 
        private void GetUiElementsClicked()
        {
            clickData.position = Mouse.current.position.ReadValue();
            clickResults.Clear();
 
            uiRaycaster.Raycast(clickData, clickResults);
 
            foreach(RaycastResult result in clickResults)
            {
                GameObject uiElement = result.gameObject;
 
                Debug.Log(uiElement.name);
            }
        }

        private void EnableBuild()
        {
            
        }
    }
}
