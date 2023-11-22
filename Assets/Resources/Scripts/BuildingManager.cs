
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
	public Building[] buildingTypes;
	public List<Building> buildings = new();
	
	void Awake()
	{
		buildingTypes = Resources.LoadAll<Building>("Prefabs/Buildings");
		AddBuilding<Dynamo>(Vector3.zero);
	}

	void AddBuilding<T>(Vector3 position) where T : Building
	{
		buildings.Add(Instantiate(buildingTypes.OfType<T>().First(), position, Quaternion.identity, transform));
	}
}
