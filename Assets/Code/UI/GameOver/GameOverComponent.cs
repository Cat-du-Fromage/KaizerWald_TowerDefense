using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense
{
    public class GameOverComponent : MonoBehaviour
    {
        [SerializeField]private Button MainMenu;
        [SerializeField]private Button Quit;
        private void Start()
        {
            MainMenu.onClick.AddListener(OnPressMainMenu);
            Quit.onClick.AddListener(OnPressQuit);
        }

        private void OnDestroy()
        {
            MainMenu.onClick.RemoveAllListeners();
            Quit.onClick.RemoveAllListeners();
        }

        private void OnPressMainMenu() => SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        private void OnPressQuit() => Application.Quit();
    }
}
