using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class BlueprintColorChange : MonoBehaviour
    {
        [SerializeField] private Material DefaultColor;
        [SerializeField] private Material ValidColor;
        [SerializeField] private Material BusyColor;

        private string colorName;
        
        private Color defaultColor;
        private Color validColor;
        private Color busyColor;

        private void Awake()
        {

            defaultColor = DefaultColor.color;
            validColor = ValidColor.color;
            busyColor = BusyColor.color;
        }


        public void OnBusyTile()
        {
            if (DefaultColor.color == busyColor) return;
            DefaultColor.color = busyColor;
        }
        
        public void OnFreeTile()
        {
            if (DefaultColor.color == validColor) return;
            DefaultColor.color = validColor;
        }
    }
}
