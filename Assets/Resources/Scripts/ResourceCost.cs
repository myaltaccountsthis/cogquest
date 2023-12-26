using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCost : MonoBehaviour
{
	public RectTransform rectTransform { get; private set; }

	private TextMeshProUGUI textLabel;
	private Image icon;

	void Awake()
	{
		textLabel = transform.Find("TextLabel").GetComponent<TextMeshProUGUI>();
		icon = transform.Find("Icon").GetComponent<Image>();
		rectTransform = (RectTransform)transform;
	}

	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}

	public void SetText(int value)
	{
		textLabel.text = value.ToString();
	}

	public float GetTextWidth()
	{
		//return textLabel.textBounds.size.x;
		return textLabel.rectTransform.rect.width;
	}

	public float GetMinimumWidth()
	{
		return GetTextWidth() + icon.rectTransform.rect.width;
	}
}
