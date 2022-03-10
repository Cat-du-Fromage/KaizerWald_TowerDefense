using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private UIGameInformation InGameInformation;
        
        [SerializeField] private GameObject InGameMenu;
        [SerializeField] private GameObject GameOverScreen;

        private void Awake()
        {
            InGameInformation ??= FindObjectOfType<UIGameInformation>();
            GameOverScreen ??= FindObjectOfType<UIGameInformation>().gameObject;
            InGameMenu ??= FindObjectOfType<InGameMenuComponent>().gameObject;
        }

        private void Start()
        {
            InGameInformation.OnHealthIsZero += OnGameLost;
        }

        private void OnDestroy()
        {
            InGameInformation.OnHealthIsZero -= OnGameLost;
        }

        private void Update()
        {
            if (GameOverScreen.activeSelf) return;
            if (!Keyboard.current.escapeKey.wasReleasedThisFrame) return;
            ToggleMenu();
        }
        
        private void ToggleMenu() => InGameMenu.SetActive(!InGameMenu.activeSelf);

        private void OnGameLost()
        {
            if (InGameMenu.activeSelf)
            {
                InGameMenu.SetActive(false);
            }
            GameOverScreen.SetActive(true);
        }
    }
}
