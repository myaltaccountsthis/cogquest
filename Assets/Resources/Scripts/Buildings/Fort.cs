using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fort : Building
{

	[SerializeField]
    private int TIER;

    public int Tier
    {
        get => TIER;
    }

	protected override void Awake() {
        base.Awake();
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

	/// <summary>
	/// Updates the fort's color and information based on team
	/// </summary>
	public override void OnDestroyed()
	{
		team = team == 0 ? 1 : 0;
		health = MAX_HEALTH;
		UpdateSpriteColor();

		if (Occupied)
		{
			gameController.OnFortOccupied(this);
			if(TIER == 3)
				gameController.WinSequence();
		}
		else
		{
			gameController.OnFortLost(this);

			if (TIER == 0)
				gameController.DefeatSequence();
		}
		
		healthBar.SetPercentage(HealthFraction);
	}

	public override void OnDamaged()
	{
		base.OnDamaged();
	}
}
