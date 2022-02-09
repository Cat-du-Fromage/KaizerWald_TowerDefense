using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public class InteractionInputs : MonoBehaviour
    {
        private InputsEventCallBack inputsEventCallBack;
        
        private SelectionInputController Control;
        private SelectionInputController.MouseControlActions MouseCtrl;
        private InputAction SelectionEvents;
        private InputAction PlacementEvents;

        private bool uiEvent;

        //Selection Inputs Event
        public bool ShiftPressed;
        //public bool LeftClick;
        //public bool IsDragging;

        //public Vector2 StartMouseClick;
        //public Vector2 EndMouseClick;

        private void OnEnable() => Control.Enable();
        private void OnDisable() => Control.Disable();
        
        private void Awake()
        {
            inputsEventCallBack = new InputsEventCallBack(gameObject);
            Control ??= new SelectionInputController();
            MouseCtrl = Control.MouseControl;
            SelectionEvents = Control.MouseControl.SelectionMouseLeftClick;
            PlacementEvents = Control.MouseControl.PlacementRightClickMove;
        }

        private void Start()
        {
            Control.MouseControl.ShiftClick.ToggleStartCancelEvent(OnStartShift, OnCancelShift, true);
            SelectionEvents.ToggleAllEvents(OnStartMouseClick, OnPerformLeftClickMoveMouse, OnCancelMouseClick, true);
        }

        private void OnDestroy()
        {
            Control.MouseControl.ShiftClick.ToggleStartCancelEvent(OnStartShift, OnCancelShift, false);
            SelectionEvents.ToggleAllEvents(OnStartMouseClick, OnPerformLeftClickMoveMouse, OnCancelMouseClick, false);
        }

        private void OnStartShift(InputAction.CallbackContext ctx) => ShiftPressed = true;
        private void OnCancelShift(InputAction.CallbackContext ctx) => ShiftPressed = false;
        
        //LEFT CLICK + MOUSE MOVE
        //==============================================================================================================
        private void OnStartMouseClick(InputAction.CallbackContext ctx)
        {
            //StartMouseClick = ctx.ReadValue<Vector2>();
            //uiEvent = EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId);
            inputsEventCallBack.Dispatch(InputEventType.Press);
            //Debug.Log($"Is UI event? {uiEvent}");
        }
        
        private void OnPerformLeftClickMoveMouse(InputAction.CallbackContext ctx)
        {
            //if (uiEvent) return;
            //EndMouseClick = ctx.ReadValue<Vector2>();
            inputsEventCallBack.Dispatch(InputEventType.Move);
        }
        
        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            if (uiEvent)
            {
                Debug.Log($"Ui Input Event Left Click");
                return;
            }
            inputsEventCallBack.Dispatch(InputEventType.Release);
        }

    }
}
