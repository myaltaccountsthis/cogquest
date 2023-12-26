using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BuildOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
}
