using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;
/*
namespace TowerDefense
{
    public class InteractionInputs : MonoBehaviour
    {

        public SelectionInputController Control;
        public SelectionInputController.MouseControlActions MouseCtrl { get; private set; }
        public InputAction SelectionEvents { get; private set; }
        public InputAction PlacementEvents { get; private set; }


        //Selection Inputs Event
        public bool ShiftPressed{ get; private set; }
        public bool LeftClick{ get; private set; }
        public bool IsDragging{ get; private set; }

        public Vector2 StartMouseClick{ get; private set; }
        
        public readonly Vector2[] EndMouseClick = new Vector2[2];

        private void OnEnable() => Control.Enable();
        private void OnDisable() => Control.Disable();
        
        private void Awake()
        {
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
            StartMouseClick = ctx.ReadValue<Vector2>();
            LeftClick = true;
            //if start is on UI => define eventType.UI
        }
        
        private void OnPerformLeftClickMoveMouse(InputAction.CallbackContext ctx)
        {
            //if EventType == UI => return;
            if(EndMouseClick[0] != ctx.ReadValue<Vector2>()) //this way we can compare arr[0] and arr[1] in other systems
            {
                EndMouseClick[0] = ctx.ReadValue<Vector2>(); //swap : new current [0]
                (EndMouseClick[0], EndMouseClick[1]) = (EndMouseClick[1], EndMouseClick[0]); //swap : current become previous 
            }
            IsDragging = (EndMouseClick[1] - StartMouseClick).sqrMagnitude > 200;
            //Send => START + END! ONLY if dragging
        }
        
        private void OnCancelMouseClick(InputAction.CallbackContext ctx)
        {
            //if start == 
            //Send => End Position
            //If dragging = true => notify drag selection
            LeftClick = false;
            IsDragging = false;
        }

    }
}
*/