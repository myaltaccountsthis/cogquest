using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dynamo : TileBuilding
{
	private static List<Tile> tiles;

	static Dynamo()
	{
		tiles = new List<Tile>() {
			Resources.Load<Tile>("Sprite Sheets/Dynamo 0"),
			Resources.Load<Tile>("Sprite Sheets/Dynamo 1"),
			Resources.Load<Tile>("Sprite Sheets/Dynamo 2"),
			Resources.Load<Tile>("Sprite Sheets/Dynamo 3")
		};
	}

	private const float activeDuration = 30f;
	public float durationLeft { get; private set; }

	public override bool NeedsRepainting => shouldRepaint;
	private bool shouldRepaint;

	public Dynamo(Vector2Int position) : base(position)
	{
		durationLeft = 0f;
		shouldRepaint = false;
	}

	public override void OnUpdate(float deltaTime)
	{
		// repaint if this crosses a (currently hardcoded) interval
		if (durationLeft % 10f < deltaTime && durationLeft != 0f)
			shouldRepaint = true;
		durationLeft = Mathf.Max(durationLeft - deltaTime, 0f);
	}

	public override void OnInteract()
	{
		durationLeft = activeDuration;
	}

	public override Tile GetTile()
	{
		if (durationLeft >= activeDuration * 2 / 3)
			return tiles[3];

		if (durationLeft >= activeDuration * 1 / 3)
			return tiles[2];

		if (durationLeft > 0)
			return tiles[1];

		return tiles[0];
	}
}
