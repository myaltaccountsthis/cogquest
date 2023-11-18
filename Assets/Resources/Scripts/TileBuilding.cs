using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class TileBuilding
{
	// Currently this only supports rectangular buildings
	public virtual Vector2Int size => Vector2Int.one;
	public Vector2Int position { get; private set; }
	public virtual bool NeedsRepainting => false;

	public TileBuilding(Vector2Int position)
	{
		this.position = position;
	}

	public Vector2Int[] GetTilePositions()
	{
		Vector2Int[] arr = new Vector2Int[size.x * size.y];
		for (int y =  0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
				arr[y * size.x + x] = new Vector2Int(position.x + x, position.y + y);
		}
		return arr;
	}

	public abstract void OnUpdate(float deltaTime);

	public abstract void OnInteract();

	public abstract Tile GetTile();
}
