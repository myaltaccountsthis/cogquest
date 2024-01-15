using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Building : Entity
{
	public BuildingCategory category = BuildingCategory.None;
	[SerializeField] private int coalUse;

	public int CoalUse
	{
		get => coalUse;
	}

	// Buildings can only be box colliders
	public new BoxCollider2D collider { get; private set; }

	private static readonly HashSet<string> blacklistedTiles = new HashSet<string>() { "Rock", "Wall" };

	protected override void Awake()
	{
		base.Awake();

		collider = GetComponent<BoxCollider2D>();
	}

	public TileBase[] GetOverlappingTiles(Tilemap tilemap)
	{
		return tilemap.GetTilesBlock(collider.ColliderToBoundsInt());
	}

	public virtual bool IsValidLocation(Tilemap tilemap)
	{
		if (Physics2D.BoxCastAll(transform.position, GetCollisionSize(), 0f, Vector2.zero, 0f, GameController.buildingShadowLayerMask).Length > 0)
			return false;
		foreach (TileBase tile in GetOverlappingTiles(tilemap))
		{
			if (tile == null || blacklistedTiles.Contains(tile.name))
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

    protected override List<string> GetEntityInfoList()
    {
        List<string> list = base.GetEntityInfoList();
		list.Add(
			(coalUse >= 0 ? "Coal use: " + coalUse : "Coal gain: " + -coalUse)
		);
		return list;
    }

	public override void LoadEntitySaveData(Dictionary<string, string> saveData)
	{
		base.LoadEntitySaveData(saveData);

		Physics2D.SyncTransforms();
	}
}

public enum BuildingCategory
{
	None,
	Harvesters,
	Walls,
	Attack
}