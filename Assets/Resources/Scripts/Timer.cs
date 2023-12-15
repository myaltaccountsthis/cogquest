using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
	private TextMeshProUGUI minutes;
	private TextMeshProUGUI seconds;

	void Awake()
	{
		minutes = transform.Find("Minutes").GetComponent<TextMeshProUGUI>();
		seconds = transform.Find("Seconds").GetComponent<TextMeshProUGUI>();
	}

	public void SetTime(float timer)
	{
		int time = Mathf.CeilToInt(timer);
		int min = time / 60, sec = time % 60;
		minutes.text = min.ToString();
		seconds.text = string.Format("{0:D2}", sec);
	}
}
