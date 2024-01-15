using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class State : MonoBehaviour
{
	private TextMeshProUGUI textLabel;

	private static readonly Color peaceColor = new Color(.5f, .725f, 1f);
	private static readonly Color attackColor = new Color(1f, .275f, .275f);
	private static readonly Color winColor = new Color(.745f, .949f, .133f);

	void Awake()
	{
		textLabel = GetComponent<TextMeshProUGUI>();
	}

	public void UpdateState(float timeLeft, bool won = false)
	{
		if (won)
		{
			textLabel.text = "VICTORY";
			textLabel.color = winColor;
		}
		else
		{
			if (timeLeft < 0)
			{
				textLabel.text = "ATTACK";
				textLabel.color = attackColor;
			}
			else
			{
				textLabel.text = "PEACE";
				textLabel.color = peaceColor;
			}
		}
	}
}
