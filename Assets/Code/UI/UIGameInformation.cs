using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using KWUtils;

namespace TowerDefense
{
    public class UIGameInformation : MonoBehaviour
    {
        //Health bar Info
        [SerializeField] private int MaxHealth = 100;
        [SerializeField] private Slider PlayerHealth;
        [SerializeField] private TextMeshProUGUI HealthBarText;

        //Resources Info
        private int currentResources;
        [SerializeField] private int BaseResources = 100;
        [SerializeField] private TextMeshProUGUI ResourcesText;
        
        private void Awake()
        {
            PlayerHealth  ??= gameObject.GetWithTagComponentInChildren<UITagHealthBar, Slider>();
            HealthBarText ??= gameObject.GetWithTagComponentInChildren<UITagHealthBar, TextMeshProUGUI>();
            ResourcesText ??= gameObject.GetWithTagComponentInChildren<UITagResources, TextMeshProUGUI>();
        }

        private void Start()
        {
            InitializeResourcesPanel();
            InitializeHealthBar();
        }

        private void OnDestroy()
        {
            PlayerHealth.onValueChanged.RemoveAllListeners();
        }

        //Resources Panel
        //====================
        private void UpdateResourcesUI() => ResourcesText.text = currentResources.ToString();
        private void InitializeResourcesPanel()
        {
            currentResources = BaseResources;
            UpdateResourcesUI();
        }
        
        public void AddResources(int num)
        {
            currentResources += num;
            UpdateResourcesUI();
        }
        
        public void RemoveResources(int num)
        {
            currentResources -= num;
            UpdateResourcesUI();
        }

        //Health Bar
        //====================
        private void InitializeHealthBar()
        {
            PlayerHealth.maxValue = MaxHealth;
            HealthBarText.text = $"{MaxHealth} / {MaxHealth}";
            PlayerHealth.onValueChanged.AddListener(OnHealthChange);
        }
        
        private void OnHealthChange(float healthValue) => HealthBarText.text = $"{(int)healthValue} / {MaxHealth}";

        public void TakeDamage() => PlayerHealth.value -= PlayerHealth.value > 0 ? 1 : 0;
    }
}
