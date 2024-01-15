using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistUnit : MeleeUnit
{
	public Sprite[] fistSprites;

	private SpriteRenderer fistSpriteRenderer;

	protected override void Awake()
	{
		base.Awake();

		fistSpriteRenderer = transform.Find("AttackHitbox").GetComponent<SpriteRenderer>();
	}

	public override void DoAttack()
	{
		base.DoAttack();

		fistSpriteRenderer.sprite = fistSprites[1];

		CancelInvoke(nameof(RetractArms));
		Invoke(nameof(RetractArms), .2f);
	}

	private void RetractArms()
	{
		fistSpriteRenderer.sprite = fistSprites[0];
	}
}
