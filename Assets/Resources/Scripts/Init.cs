using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
	void Awake()
	{
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 1;
	}
}