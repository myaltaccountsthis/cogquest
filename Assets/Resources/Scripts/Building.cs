using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Building : MonoBehaviour
{
	// Currently this only supports rectangular buildings
	// public virtual Vector2Int size => Vector2Int.one;
	
	public Vector3 position
	{
		get => transform.position;
		set
		{
			transform.position = value;
		}
	}
	// public virtual bool NeedsRepainting => false;

	public Building(Vector3 position)
	{
		this.position = position;
	}

	private void Awake()
	{
		RenderSprite();
	}

	public void RenderSprite()
	{
		GetComponent<SpriteRenderer>().sprite = GetSprite();
	}

	
	
	/*
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
	*/

	// public abstract void OnUpdate(float deltaTime);

	public abstract Sprite GetSprite();
	

	// public abstract Tile GetTile();
}
