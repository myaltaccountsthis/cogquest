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

    public int Tier
    {
        get => TIER;
    }

	protected override void Awake() {
        base.Awake();

        gameController = GameObject.Find("Canvas").GetComponent<GameController>();
    }

    
	public override void DoMouseDown()
	{
        gameController.OpenSpawnMenu(this);
	}

	public override void DoMouseCancel()
	{
        gameController.CloseSpawnMenu();
	}

	public override void LoadEntitySaveData(Dictionary<string, string> saveData)
	{
		base.LoadEntitySaveData(saveData);
	}

	public override void OnDestroyed()
	{
		team = team == 0 ? 1 : 0;
		health = MAX_HEALTH;
		UpdateSpriteColor();

		if (Occupied)
		{
			gameController.OnFortOccupied(this);
			if(TIER == 3) gameController.WinSequence();
		}
		else if (TIER == 0)
		{
			gameController.DefeatSequence();
		}
	}

	public override void OnDamaged()
	{
		if (!Occupied)
		{
			gameController.OnEnemyFortDamaged();
		}
	}
}
