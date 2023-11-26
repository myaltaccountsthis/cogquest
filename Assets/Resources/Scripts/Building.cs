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
	

	public abstract Sprite GetSprite();
}
