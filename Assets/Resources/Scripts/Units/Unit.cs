using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : Entity
{
    [SerializeField]
    protected float speed;
	[SerializeField]
	protected float attackRange;
	[SerializeField]
	private Vector2[] patrolWaypoints;
	[SerializeField]
	private float patrolInterval;

	public Range range;

	public UnitBehavior behavior { get; private set; }

	private Entity currentTarget;
	private int currentPatrolIndex;
	private bool patrolWaiting;
	private bool isAttacking;

	protected override void Awake()
	{
		base.Awake();

		range.onEnemyDetected += OnEnemyDetected;
		behavior = UnitBehavior.None;
		currentTarget = null;
		currentPatrolIndex = -1;
		patrolWaiting = false;
		isAttacking = false;

		range.team = team;

		Debug.Assert(GetComponent<Rigidbody2D>().isKinematic, "Unit rigidbody type should be set to kinematic");
	}

	protected override void Start()
	{
		base.Start();

		Patrol();
	}

	protected override void Update()
	{
		if (!active)
			return;

		base.Update();

		Entity target = range.target;
		switch (behavior)
		{
			case UnitBehavior.Patrolling:
				// TODO Walk towards current patrol index
				if (currentPatrolIndex >= patrolWaypoints.Length || patrolWaiting)
					break;

				Vector3 waypoint = patrolWaypoints[currentPatrolIndex];
				if (MoveToPoint(waypoint))
				{
					patrolWaiting = true;
					currentPatrolIndex = (currentPatrolIndex + 1) % patrolWaypoints.Length;
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
					Vector3 targetPos = target.transform.position;
					if (MoveToPoint(target.transform.position,  attackRange))
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
				else if (!IsInRange(target.transform.position))
				{
					behavior = UnitBehavior.Following;
				}
				else
				{
					Attack();
				}

				break;
		}
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

		transform.position += moveVector;
		if (moveVector.magnitude > 0)
			transform.eulerAngles = moveVector.DirectionToEulerAngles();
		
		return IsInRange(point, distance);
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
			currentPatrolIndex = 0;
			range.shouldRetarget = true;
		}
	}

	private void OnEnemyDetected(Entity other)
	{
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
		Debug.Log("Attacked");
		Invoke(nameof(TestDoneAttacking), 1);

		return true;
	}

	private void TestDoneAttacking()
	{
		isAttacking = false;
	}

	public override void LoadEntitySaveData(Dictionary<string, string> saveData)
	{
		base.LoadEntitySaveData(saveData);

		if (saveData.TryGetValue("patrolWaypoints", out string patrolWaypointsStr))
			patrolWaypoints = patrolWaypointsStr.Split(";").Select(str => {
				float[] arr = str.Split(",").Select(float.Parse).ToArray();
				return new Vector2(arr[0], arr[1]);
			}).ToArray();
	}

	public override Dictionary<string, string> GetEntitySaveData()
	{
		string str = string.Join(";", patrolWaypoints.Select(vec2 => string.Format("{0},{1}", vec2.x, vec2.y)));
		return base.GetEntitySaveData().ChainAdd("patrolWaypoints", str);
	}
}

public enum UnitBehavior
{
	None,
	Patrolling,
	Following,
	Attacking
}
