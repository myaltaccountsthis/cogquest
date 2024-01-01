using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Building : Entity
{
	public BuildingCategory category = BuildingCategory.None;
	[SerializeField]
	private int coalUse;

	public int CoalUse
	{
		get => coalUse;
	}

	// Buildings can only be box colliders
	protected new BoxCollider2D collider;
	protected SpriteRenderer spriteRenderer;


	protected override void Awake()
	{
		base.Awake();

		collider = GetComponent<BoxCollider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public TileBase[] GetOverlappingTiles(Tilemap tilemap)
	{
		return tilemap.GetTilesBlock(collider.ColliderToBoundsInt());
	}

	public virtual bool IsValidLocation(Tilemap tilemap)
	{
		if (Physics2D.BoxCastAll(transform.position, GetCollisionSize(), 0f, Vector2.zero, 0f, GameController.buildingLayerMask).Length > 0)
			return false;
		foreach (TileBase tile in GetOverlappingTiles(tilemap))
		{
			if (tile != null && tile.name == "Rock")
				return false;
		}
		return true;
	}

	/// <summary>
	/// Get the size used for building overlap check (slightly smaller than actual size)
	/// </summary>
	public Vector2 GetCollisionSize()
	{
		return new Vector2(collider.size.x - .1f, collider.size.y - .1f);
	}

	/// <summary>
	/// Change all sprite renderers in this building to a certain color (used for turrets)
	/// </summary>
	public virtual void SetSpriteColor(Color color)
	{
		spriteRenderer.color = color;
	}
}

public enum BuildingCategory
{
	None,
	Harvesters,
	Walls,
	Attack
}