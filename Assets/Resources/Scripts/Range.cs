using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Range : MonoBehaviour
{
	[HideInInspector]
	public int team;
	public UnityAction<Entity> onEnemyDetected = (_) => {};

	private LinkedList<Entity> enemiesInRange;
	private float t;

	public bool shouldRetarget;
	private const float retargetInterval = .5f;

	public Entity target => enemiesInRange.FirstOrDefault();

	public bool active;

	void Awake()
	{
		enemiesInRange = new LinkedList<Entity>();
		t = 0;
		shouldRetarget = true;
		active = false;
	}

	public void Activate()
	{
		active = true;
		Rescan();
	}

	public void Rescan()
	{
		enemiesInRange.Clear();
		Collider2D[] colliders = GetComponent<Collider2D>().GetCollisions(team);
		foreach (Collider2D collider in colliders)
		{
			if (collider != null)
				OnTriggerEnter2D(collider);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (active && other.TryGetComponent(out Entity entity) && entity.team != team)
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
		if (active && other.TryGetComponent(out Entity entity))
		{
			enemiesInRange.Remove(entity);
		}
	}

	/// <summary>
	/// Removes any null or same-team entities
	/// </summary>
	private void CleanUp()
	{
		foreach (Entity entity in enemiesInRange.Where(entity => entity == null || entity.team == team).ToArray())
		{
			enemiesInRange.Remove(entity);
		}
	}

	private Entity GetClosestEnemy()
	{
		Entity closest = null;
		float closestDistance = float.MaxValue;
		CleanUp();
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
			if (closest != null && closest != enemiesInRange.First?.Value)
			{
				enemiesInRange.Remove(closest);
				enemiesInRange.AddFirst(closest);
				onEnemyDetected(closest);
			}
		}
	}

	void Update()
	{
		if (GameController.isPaused)
			return;
		
		if (enemiesInRange.Count > 0)
		{
			t += Time.deltaTime;
			Entity entity;
			// Check if entity is non-null, active, and alive
			while (enemiesInRange.First != null && ((entity = enemiesInRange.First.Value) == null || !entity.isActiveAndEnabled || entity.health <= 0))
			{
				enemiesInRange.RemoveFirst();
				shouldRetarget = true;
			}
			if (enemiesInRange.Count > 0)
				onEnemyDetected(enemiesInRange.First.Value);
			if (t >= retargetInterval)
			{
				// Don't want this calling Retarget() every frame
				t %= retargetInterval;
				Retarget();
			}
		}
	}
}
