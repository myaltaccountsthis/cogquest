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
		int time;
		if (timer < 0f)
			time = Mathf.FloorToInt(-timer);
		else
			time = Mathf.CeilToInt(timer);
		main.text = GameController.FormatMinutesSeconds(time);
		//minutes.text = min.ToString();
		//seconds.text = string.Format("{0:D2}", sec);
	}
}
