using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Building : Entity
{
	[SerializeField]
	protected int coalUse;

	public int CoalUse {
		get => coalUse;
	}
    
    // Currently this only supports rectangular buildings
	// public virtual Vector2Int size => Vector2Int.one;

	void Start()
	{
		
	}

	void Update()
	{
		
	}

}
