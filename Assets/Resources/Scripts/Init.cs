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
		GameController.isPaused = false;
		AudioListener.pause = false;
	}
	
	public void ChangeScene(string scene)
	{
		GameController.isPaused = false;
		SceneManager.LoadScene(scene);
	}
	
	public void ResetAndChangeScene(string scene) {
		// TODO: dataManager - integrate with database
		dataManager.ResetData();
		SceneManager.LoadScene(scene);
	}
	
	public void Exit() {
		Application.Quit();
	}
}
