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
}
