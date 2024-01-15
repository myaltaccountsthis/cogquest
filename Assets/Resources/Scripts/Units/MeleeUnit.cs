using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeUnit : AnimatedUnit
{
	private Collider2D hitbox;

	[SerializeField]
	private float buildingDamageMultiplier = 1f;

	protected override void Awake()
	{
		base.Awake();

		hitbox = transform.Find("AttackHitbox").GetComponent<Collider2D>();
	}

	public override void DoAttack()
	{
		base.DoAttack();

		foreach (Collider2D collider in hitbox.GetCollisions(team, 3))
		{
			if (collider != null && collider.TryGetComponent(out Entity entity) && entity.team != team)
			{
				entity.TakeDamage(entity is Building ? buildingDamageMultiplier * damage : damage);
			}
		}
	}
}
