using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense
{
    public class MainMenuButtons : MonoBehaviour
    {
        [SerializeField]private Button Play;
        [SerializeField]private Button Quit;
        private void Start()
        {
            Play.onClick.AddListener(OnPressPlay);
            Quit.onClick.AddListener(OnPressQuit);
        }

        private void OnDestroy()
        {
            Play.onClick.RemoveAllListeners();
            Quit.onClick.RemoveAllListeners();
        }

        private void OnPressPlay() => SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        private void OnPressQuit() => Application.Quit();
    }
}
