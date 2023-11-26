
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
	// Array of prefabs for each building
	public Building[] buildingTypes;
	// Collection of buildings on map
	public List<Building> buildings = new();
	
	void Awake()
	{
		buildingTypes = Resources.LoadAll<Building>("Prefabs/Buildings");
		AddBuilding<Dynamo>(Vector3.zero);
	}

	// Temporary way to statically instantiate buildings
	void AddBuilding<T>(Vector3 position) where T : Building
	{
		buildings.Add(Instantiate(buildingTypes.OfType<T>().First(), position, Quaternion.identity, transform));
	}
}
