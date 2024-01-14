using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
	private TextMeshProUGUI main;
	private TextMeshProUGUI minutes;
	private TextMeshProUGUI seconds;

	void Awake()
	{
		main = GetComponent<TextMeshProUGUI>();
		minutes = transform.Find("Minutes").GetComponent<TextMeshProUGUI>();
		seconds = transform.Find("Seconds").GetComponent<TextMeshProUGUI>();
	}

	public void SetTime(float timer)
	{
		int time = Mathf.CeilToInt(Mathf.Max(0f, timer));
		int min = time / 60, sec = time % 60;
		main.text = string.Format("{0}:{1:D2}", min, sec);
		//minutes.text = min.ToString();
		//seconds.text = string.Format("{0:D2}", sec);
	}
}
