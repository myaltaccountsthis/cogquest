using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
	[SerializeField]
	private float speed;
	[SerializeField]
	private float lifetime;

	private float timeAlive;

	protected override void Awake()
	{
		base.Awake();

		timeAlive = 0f;
	}

	protected override void Update()
	{
		if (!active)
			return;

		base.Update();

		//transform.Translate(0f, Time.deltaTime * speed, 0f);
		if (timeAlive >= lifetime)
			Destroy(gameObject);

		timeAlive += Time.deltaTime;
	}

	// Other collider is self
	void OnCollisionEnter2D(Collision2D collision)
	{
		Entity entity = collision.gameObject.GetComponent<Entity>();
		if (entity != null && entity.team != team)
		{
			// TODO deal damage
		}

		Destroy(gameObject);
	}

	public override void LoadEntitySaveData(Dictionary<string, string> saveData)
	{
		base.LoadEntitySaveData(saveData);

		if (saveData.TryGetValue("timeAlive", out string timeAliveStr))
			timeAlive = float.Parse(timeAliveStr);

		GetComponent<Rigidbody2D>().velocity = transform.TransformDirection(Vector3.up) * speed;
	}

	public override Dictionary<string, string> GetEntitySaveData()
	{
		return base.GetEntitySaveData().ChainAdd("timeAlive", timeAlive.ToString());
	}
}
