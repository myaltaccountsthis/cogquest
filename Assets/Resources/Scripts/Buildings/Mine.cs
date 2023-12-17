using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Mine : Building
{
    // The rate of resource gain per resource tile per second
    [SerializeField]
    private float baseSpeed;
    private float mineSpeed;
    [SerializeField]
    private Tile[] mineableTiles;

    public float MineSpeed {
        get => mineSpeed;
    }

	public override bool IsValidLocation(Tilemap tilemap)
	{
        return base.IsValidLocation(tilemap);
	}
}
