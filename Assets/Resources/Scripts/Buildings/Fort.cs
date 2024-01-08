using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;

public class Fort : Building
{
    private GameController gameController;

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

        gameController = GameObject.Find("Canvas").GetComponent<GameController>();
        occupied = TIER == 0;
        SetSpriteColor(occupied ? OCCUPIED_COLOR : UNOCCUPIED_COLOR);
    }

    
	public override void DoMouseDown()
	{
        gameController.OpenSpawnMenu(this);
	}

	public override void DoMouseCancel()
	{
        gameController.CloseSpawnMenu();
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
