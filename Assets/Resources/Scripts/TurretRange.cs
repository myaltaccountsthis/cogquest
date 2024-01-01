using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurretRange : MonoBehaviour
{
	public UnityAction<Collider2D> onEnemyDetected;

	void OnTriggerEnter2D(Collider2D other)
	{
		// in the future, maybe add team check
		onEnemyDetected(other);
	}
}
