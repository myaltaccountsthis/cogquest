using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public InputAction pauseAction;

    public GameObject pauseMenu;
    public GameObject winScreen;
    public GameObject gameOverScreen;
    private GameController gameController;
    
    void Awake()
    {
        pauseAction.performed += ctx => TogglePauseMenuActive();
        gameController = GameObject.Find("Canvas").GetComponent<GameController>();
    }

    void OnEnable()
    {
        pauseAction.Enable();
    }

	void OnDisable()
	{
		pauseAction.Disable();
	}

	public void TogglePauseMenuActive()
    {
        if (winScreen.activeInHierarchy || gameOverScreen.activeInHierarchy)
            return;
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
        winScreen.transform.Find("TimeElapsed").Find("Time").GetComponent<TextMeshProUGUI>().text = gameController.GetPlayTimeFormatted();
        winScreen.SetActive(active);
    }

    public void SetGameOverScreenActive(bool active)
    {
        GameController.SetPaused(active);
        gameOverScreen.SetActive(active);
    }
}