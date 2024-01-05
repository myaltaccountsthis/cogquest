using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Range : MonoBehaviour
{
	[HideInInspector]
	public int team;
	public UnityAction<Entity> onEnemyDetected;

	private LinkedList<Entity> enemiesInRange;
	private float t;

	public bool shouldRetarget;
	private const float retargetInterval = .5f;

	public Entity target => enemiesInRange.FirstOrDefault();

	void Awake()
	{
		onEnemyDetected = (_) => { };
		enemiesInRange = new LinkedList<Entity>();
		t = 0;
		shouldRetarget = true;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Entity entity = other.GetComponent<Entity>();
		if (entity != null && entity.team != team)
		{
			enemiesInRange.AddLast(entity);
			if (enemiesInRange.Count == 1)
			{
				onEnemyDetected(entity);
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Entity entity = other.GetComponent<Entity>();
		if (entity != null)
		{
			enemiesInRange.Remove(entity);
		}
	}

	private Entity GetClosestEnemy()
	{
		Entity closest = null;
		float closestDistance = float.MaxValue;
		foreach (Entity entity in enemiesInRange)
		{
			float distance = (entity.transform.position - transform.position).magnitude;
			if (distance < closestDistance)
			{
				closest = entity;
				closestDistance = distance;
			}
		}
		return closest;
	}

	/// <summary>
	/// Called every once in a while to try finding closest enemy
	/// </summary>
	protected void Retarget()
	{
		if (enemiesInRange.Count > 0)
		{
			Entity closest = GetClosestEnemy();
			// Retarget if there is a closer enemy
			if (closest != null && closest != enemiesInRange.First.Value)
			{
				enemiesInRange.Remove(closest);
				enemiesInRange.AddFirst(closest);
				onEnemyDetected(closest);
			}
		}
	}

	void Update()
	{
		if (enemiesInRange.Count > 0)
		{
			t += Time.deltaTime;
			Entity entity;
			// TODO make sure this check if entity is alive works
			while ((entity = enemiesInRange.First.Value) == null || !entity.isActiveAndEnabled)
			{
				enemiesInRange.RemoveFirst();
				shouldRetarget = true;
				if (enemiesInRange.Count > 0)
					onEnemyDetected(enemiesInRange.First.Value);
			}
			if (t >= retargetInterval)
			{
				// Don't want this calling Retarget() every frame
				t %= retargetInterval;
				Retarget();
			}
		}
	}
}
