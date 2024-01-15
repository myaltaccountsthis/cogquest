using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public InputAction pauseAction;

    public GameObject pauseMenu;
    public GameObject winScreen;
    public GameObject gameOverScreen;
    
    void Awake()
    {
        pauseAction.performed += ctx => TogglePauseMenuActive();
    }

    void OnEnable()
    {
        pauseAction.Enable();
    }

    public void TogglePauseMenuActive()
    {
        SetPauseMenuActive(!GameController.isPaused);
    }

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