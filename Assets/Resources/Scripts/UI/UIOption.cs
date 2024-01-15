using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public UnityAction onPointerEnter;
	public UnityAction onPointerExit;

	public void OnPointerEnter(PointerEventData eventData)
	{
		onPointerEnter();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		onPointerExit();
	}

	/// <summary>
	/// Called once, creates new game objects with Image component
	/// </summary>
	public void SetSprites(Sprite[] sprites)
	{
		if (sprites.Length >= 1)
		{
			GetComponent<Image>().sprite = sprites[0];
			Image imagePrefab = Resources.Load<Image>("Prefabs/BuildingOptionImage");
			for (int i = 1; i < sprites.Length; i++)
			{
				Sprite sprite = sprites[i];
				Image clone = Instantiate(imagePrefab, transform);
				clone.sprite = sprite;
				clone.rectTransform.sizeDelta = sprite.bounds.size * 40;
				clone.rectTransform.pivot = sprite.pivot / (sprite.bounds.size * sprite.pixelsPerUnit);
				clone.rectTransform.anchoredPosition = Vector2.zero;
			}
		}
	}
}
