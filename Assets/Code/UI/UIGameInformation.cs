using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TowerDefense
{
    public class UIGameInformation : MonoBehaviour
    {
        //Health Bar
        [SerializeField] private int MaxHealth = 100;
        [SerializeField] private TextMeshProUGUI HealthBarText;
        [SerializeField] private Slider PlayerHealth;
        

        private void Awake()
        {
            PlayerHealth ??= transform.GetComponentInChildren<Slider>();
            HealthBarText ??= PlayerHealth.transform.GetComponentInChildren<TextMeshProUGUI>();
        }

        void Start()
        {
            PlayerHealth.maxValue = MaxHealth;
            HealthBarText.text = $"{MaxHealth} / {MaxHealth}";

            PlayerHealth.onValueChanged.AddListener(OnHealthChange);
        }
        private void OnHealthChange(float healthValue) => HealthBarText.text = $"{(int)healthValue} / {MaxHealth}";

        public void TakeDamage() => PlayerHealth.value -= 1;
/*
        private void Update()
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
                TakeDamage();
        }
        */
    }
}
