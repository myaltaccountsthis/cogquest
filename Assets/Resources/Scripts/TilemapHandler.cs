using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TerrainUtils;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapHandler : MonoBehaviour
{
	/*
	Tilemap tilemap;
	SpriteRenderer selectionBox;
	new Camera camera;
	
	private Dictionary<Vector3Int, TileBuilding> tileBuildings;
	
	void Awake()
	{
		tilemap = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<Tilemap>();
		selectionBox = GameObject.FindGameObjectWithTag("SelectionBox").GetComponent<SpriteRenderer>();
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		
		tileBuildings = new Dictionary<Vector3Int, TileBuilding>()
		{
			{ new Vector3Int(0, 0, 0), new Dynamo(Vector2Int.zero) }

		};
	}

	void Start()
	{
		RepaintTile(Vector3Int.zero);
	}

	void Update()
	{
		Vector3Int gridPos = GetMouseTilePosition();
		
		TileBase tile = tilemap.GetTile(gridPos);
		if (tile != null)
		{
			// Show debug selection box
			// selectionBox.enabled = true;
			//selectionBox.transform.position = tilemap.CellToWorld(gridPos) + new Vector3(.5f, .5f);

			if (Input.GetMouseButtonDown(0))
			{
				if (tile != null && tileBuildings.TryGetValue(gridPos, out TileBuilding tileBuilding))
				{
					tileBuilding.OnInteract();
				}
			}
		}
		else
		{
			selectionBox.enabled = false;
		}

		foreach (TileBuilding tileBuilding in tileBuildings.Values)
		{
			tileBuilding.OnUpdate(Time.deltaTime);
			if (tileBuilding.NeedsRepainting)
			{
				foreach (Vector2Int pos in tileBuilding.GetTilePositions())
					RepaintTile((Vector3Int) pos);
			}
		}
	}

	private Vector3Int GetMouseTilePosition()
	{
		return GetMouseTilePosition(Input.mousePosition);
	}

	private Vector3Int GetMouseTilePosition(Vector2 mousePos)
	{
		return tilemap.WorldToCell((Vector2) camera.ScreenToWorldPoint(mousePos));
	}

	private void RepaintTile(Vector3Int pos)
	{
		if (tileBuildings.TryGetValue(pos, out TileBuilding tileBuilding))
		{
			tilemap.SetTile(pos, tileBuilding.GetTile());
		}
	}
	*/
}
