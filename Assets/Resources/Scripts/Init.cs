using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
	private DataManager dataManager;

	void Awake()
	{
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 1;
		dataManager = GetComponent<DataManager>();
		GameController.SetPaused(false);
	}
	
	public void ChangeScene(string scene)
	{
		GameController gameController = FindFirstObjectByType<GameController>();
		if (gameController != null)
			gameController.SaveData();
		SceneManager.LoadScene(scene);
	}
	
	public void ResetAndChangeScene(string scene) {
		dataManager.ResetData();
		SceneManager.LoadScene(scene);
	}
	
	public void Exit() {
		Application.Quit();
	}
}
