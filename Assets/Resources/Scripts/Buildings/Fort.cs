using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;

public class Fort : Building
{
	[SerializeField]
    private int TIER;
    public bool occupied;
    private readonly Color UNOCCUPIED_COLOR = Color.red;
    private readonly Color OCCUPIED_COLOR = Color.white;

    public int Tier
    {
        get => TIER;
    }

	  protected override void Awake() {
        base.Awake();
        occupied = TIER == 0;
        SetSpriteColor(occupied ? OCCUPIED_COLOR : UNOCCUPIED_COLOR);
    }

    public void OnDestroyed()
    {
        occupied = !occupied;
        if (!occupied)
        {
            health = MAX_HEALTH;
        }
    }

}
