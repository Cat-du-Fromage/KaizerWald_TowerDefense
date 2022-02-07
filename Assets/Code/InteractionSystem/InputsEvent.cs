using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public enum InputEventType
    {
        Press,
        Move,
        Release
    }
    
    public enum InputUIEventType
    {
        Press,
        Release
    }
    
    public class InputsEventCallBack
    {
        private InteractionSystem interactionSystem;

        public InputsEventCallBack(GameObject gameObject)
        {
            interactionSystem = gameObject.GetComponent<InteractionSystem>();
            interactionSystem ??= GameObject.FindObjectOfType<InteractionSystem>();
        }
        
        public void Dispatch(InputEventType eventType)
        {
            interactionSystem.OnInputCallback(eventType);
        }
        
        public void Dispatch(InputUIEventType uiEvent)
        {
            //OnStart_LeftClick
            
            //OnCancel_LeftClick
        }
    }
}
