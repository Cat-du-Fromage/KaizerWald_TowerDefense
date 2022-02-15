using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class SelectionTag : MonoBehaviour
    {
        [SerializeField] private MeshRenderer SelectRenderer;

        private void Awake()
        {
            SelectRenderer = SelectRenderer == null ? GameObject.Find("SelectionHighlight").GetComponent<MeshRenderer>() : SelectRenderer;
        }

        private void Start()
        {
            ToggleVisible(false);
        }

        public void ToggleVisible(bool toggle)
        {
            if (SelectRenderer.enabled == toggle) return;
            SelectRenderer.enabled = toggle;
        }
    }
}
