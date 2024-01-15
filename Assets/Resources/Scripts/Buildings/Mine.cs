using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Returns number of mineable tiles under the mine
    /// </summary>
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

    /// <summary>
    /// Updates dictionary of what resources the mine is currently mining
    /// </summary>
    public void UpdateResources(Tilemap tilemap)
    {
        resources.Clear();
        foreach (TileBase tile in GetOverlappingTiles(tilemap))
        {
            //Debug.Log(tile.name);
            foreach (Tile mineableTile in mineableTiles)
            {
                if (tile != null && tile.name == mineableTile.name)
                {
                    string name = tile.name;
                    if (name == "Tree")
                        name = "Wood";

                    resources.TryAdd(name, 0);
                    resources[name]++;
                }
            }
        }
    }

    protected override List<string> GetEntityInfoList()
    {
        string resourceGen = string.Join('\n', resources == null
            ? mineableTiles.Select(tile => tile.name + ": " + MineSpeed)
            : resources.Select(pair => pair.Key + ": " + pair.Value * MineSpeed));

        List<string> list = base.GetEntityInfoList();
        list.Add(
            "GENERATION\n" +
            resourceGen
        );
        return list;
	}
}
