using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BlueprintColorChange : MonoBehaviour
    {
        //BEHIND COLOR
        [SerializeField] private Material DefaultColor;
        
        [SerializeField] private Material ValidColor;
        [SerializeField] private Material BusyColor;

        //FRONT COLOR
        [SerializeField] private Material DefaultFrontColor;
        
        [SerializeField] private Material ValidFrontColor;
        [SerializeField] private Material BusyFrontColor;
        
        //BEHIND COLOR
        private Color defaultColor;
        private Color validColor;
        private Color busyColor;
        
        //FRONT COLOR
        private Color defaultFrontColor;
        private Color validFrontColor;
        private Color busyFrontColor;

        private void Awake()
        {
            defaultColor = DefaultColor.color;
            validColor = ValidColor.color;
            busyColor = BusyColor.color;

            defaultFrontColor = DefaultFrontColor.color;
            validFrontColor = ValidFrontColor.color;
            busyFrontColor = BusyFrontColor.color;
        }


        public void OnBusyTile()
        {
            if (DefaultColor.color == busyColor) return;
            DefaultColor.color = busyColor;
            DefaultFrontColor.color = busyFrontColor;
        }
        
        public void OnFreeTile()
        {
            if (DefaultColor.color == validColor) return;
            DefaultColor.color = validColor;
            DefaultFrontColor.color = defaultFrontColor;
        }
    }
}
