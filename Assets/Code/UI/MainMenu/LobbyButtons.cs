using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PongGame
{
    public class LobbyButtons : MonoBehaviour
    {
        [SerializeField]private Button leave;
        // Start is called before the first frame update
        void Start()
        {
            leave.onClick.AddListener(() =>
            {
                GoToMainMenu();
            });
        }
        
        private void GoToMainMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}
