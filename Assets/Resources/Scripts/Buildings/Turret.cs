using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Building
{
    [SerializeField]
    private float fireRate;

	private Transform gun;
	private SpriteRenderer gunSpriteRenderer;

	protected override float rotation
	{
		get => gun.eulerAngles.z;
		set => gun.eulerAngles = new Vector3(0, 0, value);
	}

	protected override void Awake()
	{
		base.Awake();

		gun = transform.Find("Gun");
		gunSpriteRenderer = gun.GetComponent<SpriteRenderer>();

		gun.GetComponent<TurretRange>().onEnemyDetected = OnEnemyDetected;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void OnEnemyDetected(Collider2D other)
	{
		Debug.Log("Detected " + other.name);
	}

	public override void SetSpriteColor(Color color)
	{
		base.SetSpriteColor(color);
		gunSpriteRenderer.color = color;
	}
}
