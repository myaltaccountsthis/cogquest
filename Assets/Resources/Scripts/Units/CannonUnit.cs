using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonUnit : RangedUnit
{
	public Sprite[] cannonSprites;

	private SpriteRenderer cannonSpriteRenderer;

	protected override void Awake()
	{
		base.Awake();

		cannonSpriteRenderer = transform.Find("Cannon").GetComponent<SpriteRenderer>();
	}

	public override void DoAttack()
	{
		base.DoAttack();

		cannonSpriteRenderer.sprite = cannonSprites[1];

		CancelInvoke(nameof(CoolCannon));
		Invoke(nameof(CoolCannon), 1f);
	}

	private void CoolCannon()
	{
		cannonSpriteRenderer.sprite = cannonSprites[0];
	}
}
