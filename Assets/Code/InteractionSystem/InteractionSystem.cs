using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.RTTCamera;
using UnityEngine;
using KWUtils;

namespace TowerDefense
{
    public class InteractionSystem : MonoBehaviour, IInteractionSystem
    {
        public Camera MainCamera;
        private List<ISubSystem> test;
        [SerializeField] private SelectionSubSystem selectionSubSystem;
        
        //[SerializeField] private MoveOrderSubSystem selectionSubSystem;
        private void Awake()
        {
            MainCamera ??= Camera.main;
            selectionSubSystem ??= GetComponent<SelectionSubSystem>();
            
            selectionSubSystem.GetInterfaceComponent<ISubSystem>().AttachSubSystemTo(this);
        }

        public void Notify(ISubSystem subSystem)
        {
            if (subSystem is IInteractionSubSystem<SelectionEvent> selection)
            {
                SelectionNotification(selection.EventType);
            }
        }

        public void SelectionNotification(SelectionEvent eventType)
        {
            if (eventType == SelectionEvent.Selection)
            {
                //Add Selection
            }
            //Clear
        }
    }
}
