using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dynamo : Building
{
	private static Sprite[] sprites;
	
	static Dynamo()
	{
		sprites = Resources.LoadAll<Sprite>("Sprite Sheets/Buildings/Dynamo");
	}

	private const float activeDuration = 30f;
	public float durationLeft { get; private set; }

	// public override bool NeedsRepainting => shouldRepaint;
	// private bool shouldRepaint;

	public Dynamo(Vector3 position) : base(position)
	{
		durationLeft = 0f;
		// shouldRepaint = false;
	}

	void Update()
	{
		// repaint if this crosses a (currently hardcoded) interval
		if (durationLeft % 10f < Time.deltaTime && durationLeft != 0f)
			RenderSprite();
			// shouldRepaint = true;
		durationLeft = Mathf.Max(durationLeft - Time.deltaTime, 0f);
	}

	public void OnMouseDown()
	{
		durationLeft = activeDuration;
	}
	
	public override Sprite GetSprite()
	{
		if (durationLeft >= activeDuration * 2 / 3)
			return sprites[3];

		if (durationLeft >= activeDuration * 1 / 3)
			return sprites[2];

		if (durationLeft > 0)
			return sprites[1];
		
		return sprites[0];
	}
}
