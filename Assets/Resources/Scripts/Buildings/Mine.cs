using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Mine : Building
{
    // The rate of resource gain per resource tile per second
    [SerializeField]
    private int mineSpeed;
    [SerializeField]
    private Tile[] mineableTiles;
    public Dictionary<string, int> resources;

    public int MineSpeed {
        get => mineSpeed;
    }

	protected override void Awake()
    {
        base.Awake();
        resources = new Dictionary<string, int>();
    }

	public override bool IsValidLocation(Tilemap tilemap)
	{
        if (!base.IsValidLocation(tilemap))
            return false;

        return GetNumValidTiles(tilemap) > 0;
	}

    private int GetNumValidTiles(Tilemap tilemap)
    {
        int num = 0;
        foreach (TileBase tile in GetOverlappingTiles(tilemap))
        {
            foreach (Tile mineableTile in mineableTiles)
            {
                if (tile != null && tile.name == mineableTile.name)
                {
                    num++;
                }
            }
        }
        return num;
    }

    public void UpdateResources(Tilemap tilemap)
    {
        resources.Clear();
        foreach (TileBase tile in GetOverlappingTiles(tilemap))
        {
            Debug.Log(tile.name);
            foreach (Tile mineableTile in mineableTiles)
            {
                if (tile != null && tile.name == mineableTile.name)
                {
                    string name = tile.name;
                    if (name == "Tree")
                        name = "Wood";

                    if (!resources.ContainsKey(name))
                        resources.Add(name, 0);
                    resources[name]++;
                }
            }
        }
    }

}
