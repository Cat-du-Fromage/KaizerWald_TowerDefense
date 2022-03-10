using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense
{
    public class InGameMenuComponent : MonoBehaviour
    {
        [SerializeField]private Button Resume;
        [SerializeField]private Button ReturnToMenu;
        [SerializeField]private Button Quit;
        private void Start()
        {
            Resume.onClick.AddListener(OnResume);
            ReturnToMenu.onClick.AddListener(OnReturnToMenu);
            Quit.onClick.AddListener(OnPressQuit);
        }

        private void OnDestroy()
        {
            Resume.onClick.RemoveAllListeners();
            ReturnToMenu.onClick.RemoveAllListeners();
            Quit.onClick.RemoveAllListeners();
        }
        
        private void OnResume() => gameObject.SetActive(false);
        private void OnReturnToMenu() => SceneManager.LoadSceneAsync(0, LoadSceneMode.Single); //SceneManager.LoadScene("SceneManager.GetSceneAt(1), LoadSceneMode.Single");
        private void OnPressQuit() => Application.Quit();
    }
}
