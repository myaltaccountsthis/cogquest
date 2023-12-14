using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Resource : MonoBehaviour
{
	private TextMeshProUGUI textLabel;

	void Awake()
	{
		textLabel = transform.Find("TextLabel").GetComponent<TextMeshProUGUI>();
	}

	public void UpdateText(int newValue)
	{
		textLabel.text = newValue.ToString();
	}
}
