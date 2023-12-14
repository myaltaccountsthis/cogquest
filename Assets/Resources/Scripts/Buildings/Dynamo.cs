using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dynamo : Building
{
    //private static Sprite[] sprites;

	// static Dynamo()
	// {
	// 	// Might load this dynamically for all buildings, unless implementation will vary
	// 	sprites = Resources.LoadAll<Sprite>("Sprite Sheets/Buildings/Dynamo");
	// }

	// private const float activeDuration = 30f;
	// public float durationLeft { get; private set; }

	// void Update()
	// {
	// 	// repaint if this crosses a (currently hardcoded) interval
	// 	if (durationLeft % 10f < Time.deltaTime && durationLeft != 0f)
	// 		RenderSprite();
	// 	durationLeft = Mathf.Max(durationLeft - Time.deltaTime, 0f);
	// }

	public void OnMouseDown()
	{
		Debug.Log("Clicked");
		//durationLeft = activeDuration;
	}

	// public override Sprite GetSprite()
	// {
	// 	if (durationLeft >= activeDuration * 2 / 3)
	// 		return sprites[3];

	// 	if (durationLeft >= activeDuration * 1 / 3)
	// 		return sprites[2];

	// 	if (durationLeft > 0)
	// 		return sprites[1];

	// 	return sprites[0];
	// }
}
