using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KWUtils;

namespace TowerDefense
{

    
    public class InteractionSystem : MonoBehaviour, IInteractionSystem<int>
    {
        [SerializeField] private SelectionSubSystem selectionSubSystem;
        
        private void Awake()
        {
            selectionSubSystem ??= GetComponent<SelectionSubSystem>();
            
            gameObject.GetInterfaceComponent<ISubSystem>().AttachSubSystemTo(this);
        }
        
        //
    }
}
