
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
	// Array of prefabs for each building
	public Building[] buildingTypes;
	
	void Awake()
	{
		buildingTypes = Resources.LoadAll<Building>("Prefabs/Buildings");
		AddBuilding<Dynamo>(Vector3.zero);
	}

	// Temporary way to statically instantiate buildings
	void AddBuilding<T>(Vector3 position) where T : Building
	{
		Instantiate(buildingTypes.OfType<T>().First(), position + new Vector3(.5f, .5f), Quaternion.identity, transform);
	}
}
