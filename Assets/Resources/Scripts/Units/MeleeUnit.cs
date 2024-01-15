using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeUnit : Unit
{
	private Collider2D hitbox;

	protected override void Awake()
	{
		base.Awake();

		hitbox = transform.Find("AttackHitbox").GetComponent<Collider2D>();
	}

	public override void DoAttack()
	{
		foreach (Collider2D collider in hitbox.GetCollisions(team, 3))
		{
			if (collider != null && collider.TryGetComponent(out Entity entity) && entity.team != team)
			{
				entity.TakeDamage(damage);
			}
		}
	}
}
