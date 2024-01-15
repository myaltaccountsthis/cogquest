using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

        public GameObject pauseMenu;
        public GameObject winScreen;
        public GameObject gameOverScreen;
        
        public void SetPauseMenuActive(bool active)
        {
                GameController.SetPaused(active);
                pauseMenu.SetActive(active);
        }

        public void SetWinScreenActive(bool active)
        {
                GameController.SetPaused(active);
                winScreen.SetActive(active);
        }

        public void SetGameOverScreenActive(bool active)
        {
                GameController.SetPaused(active);
                gameOverScreen.SetActive(active);
        }
}