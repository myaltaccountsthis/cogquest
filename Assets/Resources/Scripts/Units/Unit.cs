using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Unit : Entity
{
    [SerializeField]
    protected float speed;
	[SerializeField]
	protected float attackRange;
	[SerializeField]
	protected Vector2[] patrolWaypoints;
	[SerializeField]
	protected float patrolInterval;
	[SerializeField]
	protected float attackInterval;
	[SerializeField]
	protected float damage;

	public Range range;
	public int zoneToUnlock;

	private UnitBehavior behavior = UnitBehavior.None;
	private PatrolMode patrolMode = PatrolMode.Waypoints;
	private Vector2 currentPatrolWaypoint;

	private Entity currentTarget = null;
	private int currentPatrolIndex;
	private bool patrolWaiting = false;
	protected bool isAttacking = false;
	private bool retargetCooldown = false;

	private AudioSource attackAudio;

	protected override void Awake()
	{
		base.Awake();

		range.onEnemyDetected += OnEnemyDetected;

		Debug.Assert(GetComponent<Rigidbody2D>().isKinematic, "Unit rigidbody type should be set to kinematic");

		attackAudio = gameObject.GetComponent<AudioSource>();
	}

	protected override void Start()
	{
		base.Start();

		// Automatically activate range if spawned by player
		if (Occupied)
			range.Activate();
		Patrol();
	}

	protected override void Update()
	{
		if (GameController.isPaused)
			return;

		if (!active)
			return;

		base.Update();

		Entity target = range.target;
		switch (behavior)
		{
			case UnitBehavior.Patrolling:
				// In the case that player rallies
				if (target != null && !retargetCooldown)
					FollowTarget(target);
				
				if (patrolWaiting || currentPatrolIndex >= patrolWaypoints.Length)
					break;

				if (MoveToPoint(currentPatrolWaypoint))
				{
					patrolWaiting = true;
					currentPatrolIndex = (currentPatrolIndex + 1) % patrolWaypoints.Length;
					SetNextPatrolPoint();
					Invoke(nameof(EnablePatrol), patrolInterval);
				}

				break;
			case UnitBehavior.Following:
				if (target == null)
				{
					Patrol();
				}
				else
				{
					if (MoveToPoint(GetTargetPos(target), attackRange))
					{
						Attack();
					}
				}

				break;
			case UnitBehavior.Attacking:
				if (isAttacking)
					break;

				if (target == null)
				{
					Patrol();
				}
				else if (!IsInRange(GetTargetPos(target)))
				{
					behavior = UnitBehavior.Following;
				}
				else
				{
					LookAtPoint(GetTargetPos(target));
					Attack();
				}

				break;
		}
	}

	private Vector3 GetTargetPos(Entity target)
	{
		if (target == null)
			return Vector3.zero;
		Vector3 targetPos = target.transform.position;
		if (target is Building building && this is not RangedUnit)
			targetPos = building.collider.ClosestPoint(transform.position);
		return targetPos;
	}

	private bool IsInRange(Vector3 point)
	{
		return IsInRange(point, attackRange);
	}

	private bool IsInRange(Vector3 point, float distance)
	{
		return (point - transform.position).magnitude <= distance;
	}

	/// <summary>
	/// Incrementally moves this unit to the point (called every frame)
	/// </summary>
	/// <returns>true if point was reached this frame</returns>
	private bool MoveToPoint(Vector3 point, float distance = 0)
	{		
		Vector3 direction = point - transform.position;
		float moveDistance = Time.deltaTime * speed;
		Vector3 moveVector;
		bool onTop = moveDistance >= direction.magnitude;
		if (onTop)
		{
			moveVector = direction;
		}
		else
		{
			moveVector = direction.normalized * moveDistance;
		}

		transform.position = gameController.GetBoundedPoint(transform.position + moveVector);
		LookAtPoint(point);

		return IsInRange(point, distance);
	}

	private void LookAtPoint(Vector3 point)
	{
		Vector3 direction = point - transform.position;
		if (direction.magnitude > 0)
			transform.eulerAngles = direction.DirectionToEulerAngles();
	}

	/// <summary>
	/// Used to continue patrolling after waiting some time
	/// </summary>
	private void EnablePatrol()
	{
		patrolWaiting = false;
	}

	public void Patrol()
	{
		if (behavior != UnitBehavior.Patrolling)
		{
			behavior = UnitBehavior.Patrolling;
			//currentPatrolIndex = 0;
			if (patrolWaypoints.Length == 0)
			{
				patrolWaypoints = new Vector2[] { transform.position };
				patrolMode = PatrolMode.Point;
			}
			SetNextPatrolPoint();
			range.shouldRetarget = true;
		}
	}

	public void Patrol(Vector2[] waypoints)
	{
		behavior = UnitBehavior.Patrolling;
		retargetCooldown = true;
		patrolWaiting = false;
		currentPatrolIndex = 0;
		patrolWaypoints = waypoints;
		SetNextPatrolPoint();
		range.shouldRetarget = true;
		range.Rescan();
		CancelInvoke(nameof(AfterRetargetCooldown));
		CancelInvoke(nameof(EnablePatrol));
		Invoke(nameof(AfterRetargetCooldown), 1f);
	}

	private void AfterRetargetCooldown()
	{
		retargetCooldown = false;
		Entity target = range.target;
		if (target)
		{
			OnEnemyDetected(range.target);
		}
	}

	private void SetNextPatrolPoint()
	{
		if (patrolMode == PatrolMode.Point)
		{
			if (currentPatrolWaypoint == patrolWaypoints[currentPatrolIndex])
			{
				float angle = Random.value * Mathf.PI * 2;
				// No need to worry about adjusting angle from unit circle to heading
				currentPatrolWaypoint = patrolWaypoints[currentPatrolIndex] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f;
			}
			else
				currentPatrolWaypoint = patrolWaypoints[currentPatrolIndex];
		}
		else
		{
			currentPatrolWaypoint = patrolWaypoints[currentPatrolIndex];
		}
	}

	private void OnEnemyDetected(Entity other)
	{
		if (behavior != UnitBehavior.Attacking && !retargetCooldown)
			FollowTarget(other);
	}

	private void FollowTarget(Entity target)
	{
		behavior = UnitBehavior.Following;
		currentTarget = target;
	}

	/// <summary>
	/// Force this unit to only attack a certain target
	/// </summary>
	public void ForceAttackTarget(Entity target)
	{
		range.shouldRetarget = false;
		FollowTarget(target);
	}

	/// <summary>
	/// Attack the target. Should set isAttacking to false when done
	/// </summary>
	/// <returns>true if attack was successful (regardless of if it hit)</returns>
	public bool Attack()
	{
		if (isAttacking)
			return false;
		
		isAttacking = true;
		behavior = UnitBehavior.Attacking;
		if(!attackAudio.isPlaying) attackAudio.Play();
		DoAttack();
		Invoke(nameof(TestDoneAttacking), attackInterval);

		return true;
	}

	public abstract void DoAttack();

	private void TestDoneAttacking()
	{
		isAttacking = false;
	}

	public override void OnDamaged()
	{
		base.OnDamaged();

		// If player attacks patrolling enemy unit
		if (!range.active)
		{
			range.Activate();
		}
	}

	public override void LoadEntitySaveData(Dictionary<string, string> saveData)
	{
		base.LoadEntitySaveData(saveData);

		if (saveData.TryGetValue("patrolWaypoints", out string patrolWaypointsStr))
			patrolWaypoints = patrolWaypointsStr.Split(";").Select(str => {
				float[] arr = str.Split(",").Select(float.Parse).ToArray();
				return new Vector2(arr[0], arr[1]);
			}).ToArray();
		if (saveData.TryGetValue("patrolMode", out string patrolModeStr))
			patrolMode = (PatrolMode)System.Enum.Parse(typeof(PatrolMode), patrolModeStr);
		if (saveData.TryGetValue("patrolWaypointIndex", out string patrolWaypointIndexStr))
			currentPatrolIndex = int.Parse(patrolWaypointIndexStr);
		if (team == 0)
			gameObject.layer = LayerMask.NameToLayer("PlayerUnits");
		range.team = team;
		//range.Activate();
	}

	public override Dictionary<string, string> GetEntitySaveData()
	{
		return base.GetEntitySaveData().ChainAdd("patrolWaypoints", PatrolWaypointsToString(patrolWaypoints))
			.ChainAdd("patrolMode", patrolMode.ToString()).ChainAdd("patrolWaypointIndex", currentPatrolIndex.ToString());
	}

	public static string PatrolWaypointsToString(Vector2[] patrolWaypoints)
	{
		return string.Join(";", patrolWaypoints.Select(vec2 => string.Format("{0},{1}", vec2.x, vec2.y)));
	}
}

public enum UnitBehavior
{
	None,
	Patrolling,
	Following,
	Attacking
}

public enum PatrolMode
{
	Point,
	Waypoints
}