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
        public Camera MainCamera;
        [SerializeField] private SelectionSubSystem selectionSubSystem;
        
        //[SerializeField] private MoveOrderSubSystem selectionSubSystem;
        private void Awake()
        {
            MainCamera ??= Camera.main;
            selectionSubSystem ??= GetComponent<SelectionSubSystem>();
        }

        public void SelectionNotification()
        {
            
        }
    }
}
