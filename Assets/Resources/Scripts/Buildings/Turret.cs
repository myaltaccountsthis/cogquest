using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Building
{
    [SerializeField]
    private float fireRate;
	[SerializeField]
	[Tooltip("In degrees")]
	private float turnRate;
	
	public Range range;
	public Transform gun;

	private GameController gameController;
	
	private SpriteRenderer gunSpriteRenderer;
	[SerializeField]
	private Projectile bulletPrefab;

	private float timeBetweenAttacks;
	private bool canShoot;

	protected override float rotation
	{
		get => gun.eulerAngles.z;
		set => gun.eulerAngles = new Vector3(0, 0, value);
	}

	protected override void Awake()
	{
		base.Awake();

		gameController = GameObject.Find("Canvas").GetComponent<GameController>();
		gunSpriteRenderer = gun.GetComponent<SpriteRenderer>();
		bulletPrefab = Resources.Load<Projectile>("Prefabs/Bullet");
		//gun.GetComponent<Range>().onEnemyDetected += OnEnemyDetected;

		range.team = team;
		timeBetweenAttacks = 1f / fireRate;
		canShoot = false;
		Invoke(nameof(CooldownDebounce), 1f);
	}

	protected override void Update()
	{
		if (GameController.isPaused)
			return;
		
		if (!active)
			return;

		base.Update();

		Entity target = range.target;
		if (target != null)
		{
			Vector3 direction = target.transform.position - transform.position;
			float targetAngle = direction.DirectionToAngle();
			rotation = Mathf.MoveTowardsAngle(rotation, targetAngle, turnRate * Time.deltaTime);
			// Make sure gun is angled accurately enough
			if (Mathf.Abs(Mathf.DeltaAngle(rotation, targetAngle)) < 1f && canShoot)
			{
				Shoot();
			}
		}
	}

	private void Shoot()
	{
		canShoot = false;

		// TODO make this shoot a bullet
		Dictionary<string, string> entityData = bulletPrefab.GetEntitySaveData();
		Vector2 bulletPos = gun.TransformPoint(new Vector3(0f, collider.size.y / 2));
		entityData["posX"] = bulletPos.x.ToString();
		entityData["posY"] = bulletPos.y.ToString();
		entityData["rotation"] = rotation.ToString();
		gameController.AddEntity(entityData);

		Invoke(nameof(CooldownDebounce), timeBetweenAttacks);
	}

	private void CooldownDebounce()
	{
		canShoot = true;
	}

	//private void OnEnemyDetected(Entity other)
	//{

	//}

	public override void LoadEntitySaveData(Dictionary<string, string> saveData)
	{
		base.LoadEntitySaveData(saveData);

		range.team = team;
		range.Activate();
	}

	public override void SetSpriteColor(Color color)
	{
		base.SetSpriteColor(color);
		gunSpriteRenderer.color = color;
	}
}
