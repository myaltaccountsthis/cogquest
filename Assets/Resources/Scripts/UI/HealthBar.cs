using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	private RectTransform rectTransform;
	private Transform inner;
	private Image[] images;
	private Color[] defaultColors;

	private const float TOTAL_FADE_TIME = 2f;
	private float fadeTime;

	private bool _isHovered;
	public bool isHovered
	{
		get => _isHovered;
		set
		{
			_isHovered = value;
            if (value)
            {
				SetActive(true);
            }
        }
	}

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		inner = transform.Find("Mask").Find("HealthBarInner");
		images = GetComponentsInChildren<Image>();
		defaultColors = new Color[images.Length];
		for (int i = 0; i < images.Length; i++)
			defaultColors[i] = images[i].color;

		isHovered = false;
		fadeTime = 0;
	}

	void Update()
	{
		fadeTime -= Time.deltaTime;
		if (!isHovered)
		{
			if (fadeTime <= 0f)
			{
				SetActive(false);
			}
			else if (fadeTime <= 1f)
			{
				SetAlpha(fadeTime);
			}
		}
	}

	public void ResetFade()
	{
		fadeTime = TOTAL_FADE_TIME;
	}

	public void SetPosition(Collider2D collider)
	{
		rectTransform.anchoredPosition = collider.transform.position + Vector3.up * collider.bounds.extents.y;
	}

	public void SetPercentage(float healthFraction)
	{
		inner.localScale = new Vector3(healthFraction, 1, 1);
	}

	public void SetAlpha(float alpha)
	{
		for (int i = 0; i < images.Length; i++)
		{
			images[i].color = Color.Lerp(Color.clear, defaultColors[i], alpha);
		}
	}

	public void SetActive(bool active)
	{
		if (active)
		{
			SetAlpha(1f);
		}

		gameObject.SetActive(active);
	}
}
