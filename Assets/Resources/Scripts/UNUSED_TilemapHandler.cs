using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TerrainUtils;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapHandler : MonoBehaviour
{
	Tilemap tilemap;
	SpriteRenderer selectionBox;
	new Camera camera;
	
	void Awake()
	{
		tilemap = GameObject.FindGameObjectWithTag("Tilemap").GetComponent<Tilemap>();
		selectionBox = GameObject.FindGameObjectWithTag("SelectionBox").GetComponent<SpriteRenderer>();
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		
	}

	void Start()
	{
		RepaintTile(Vector3Int.zero);
	}

	void Update()
	{
		if (GameController.isPaused)
			return;
		
		Vector3Int gridPos = GetMouseTilePosition();
		
		TileBase tile = tilemap.GetTile(gridPos);
		if (tile != null)
		{
			// Show debug selection box
			// selectionBox.enabled = true;
			//selectionBox.transform.position = tilemap.CellToWorld(gridPos) + new Vector3(.5f, .5f);

			if (Input.GetMouseButtonDown(0))
			{
			}
		}
		else
		{
			selectionBox.enabled = false;
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

	}
}
