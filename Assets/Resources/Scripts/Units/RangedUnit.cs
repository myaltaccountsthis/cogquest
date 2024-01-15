using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedUnit : AnimatedUnit
{
	[SerializeField]
	private Projectile projectilePrefab;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void DoAttack()
	{
		base.DoAttack();

		Dictionary<string, string> entityData = projectilePrefab.GetEntitySaveData();
		Vector2 pos = transform.TransformPoint(Vector3.up * .375f);
		entityData["posX"] = pos.x.ToString();
		entityData["posY"] = pos.y.ToString();
		entityData["rotation"] = rotation.ToString();
		entityData["team"] = team.ToString();
		gameController.AddEntity(entityData);
	}
}
