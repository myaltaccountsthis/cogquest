using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Data;

public class DataManager : MonoBehaviour
{
    public GameData gameData { get; private set; }
	public Dictionary<string, int> resources;
	private string FILE_PATH;

	public TextAsset mapFile;

	void Awake()
	{
        FILE_PATH = Application.persistentDataPath + "/gamedata.dat";
		resources = new Dictionary<string, int>()
		{
			{ "Coal", 0 },
			{ "Wood", 20 },
			{ "Stone", 0 },
			{ "Copper", 0 },
			{ "Iron", 0 }
		};
		Debug.Log("Resources: " + string.Join(", ", resources.Keys));
		gameData = new GameData();
		LoadData();
	}

	void OnApplicationQuit()
	{
	}

	// Data Save/Load functions

	private void LoadData()
	{
		// TODO change to load compressed
		gameData.map = LoadMapUncompressed();
		try
		{
			if (File.Exists(FILE_PATH))
			{
				FileLoader.ReadOverwriteJson(FILE_PATH, gameData);

				// TODO remove
				gameData.map = LoadMapUncompressed();
				Debug.Log("OVERRIDE MAP");

				// Update dictionaries
				foreach (KeyValuePair<string, int> resource in gameData.resources.ToDictionary())
				{
					// This overrides resource keys that exist in data, allowing us to add more keys to resources retroactively
					resources[resource.Key] = resource.Value;
				}

				Debug.Log("Loaded data");
				// Debug.Log(JsonUtility.ToJson(gameData));
			}
			else
			{
				// Center camera on first zone
				Zone firstZone = gameData.map.zones[0];
				gameData.cameraPosition = new Vector3(firstZone.posX + firstZone.sizeX / 2, firstZone.posY + firstZone.sizeY / 2, Camera.main.transform.position.z);
				
				Debug.Log("Created data");
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("Failed to load data, resorting to default values.\n" + e.ToString());
		}
	}

	// Save the current data (only game controller should call this in game)
	public void SaveData()
	{
		try
		{
			// Update dictionaries
			gameData.resources = new DataDictionary<string, int>(resources);
			gameData.cameraPosition = Camera.main.transform.position;

			FileLoader.WriteAndCompressJson(FILE_PATH, gameData);
			Debug.Log("Saved data");
			//Debug.Log(JsonUtility.ToJson(gameData));
		}
		catch (Exception e)
		{
			Debug.LogWarning("Failed to save data.\n" + e.ToString());
		}
	}

	private Map LoadMapUncompressed()
	{
		Map map = new Map();
		JsonUtility.FromJsonOverwrite(mapFile.text, map);
		return map;
	}

	// TODO need to test
	private Map LoadMapCompressed()
	{
		Map map = new Map();
		JsonUtility.FromJsonOverwrite(FileLoader.Decompress(mapFile.text), map);
		return map;
	}

	// for testing
	public void SaveMapUncompressed()
	{
		FileLoader.WriteFile(Application.persistentDataPath + "/testmap.txt", JsonUtility.ToJson(gameData.map));
	}
}

[Serializable]
public class GameData
{
	public int score = 0;
	public float timer = 180f;
	public int unlockedZones = 1;
	// resources is not directly editable, need to instantiate a new one on save
	public DataDictionary<string, int> resources = new();
	public Map map;
	public Vector3 cameraPosition = Vector3.zero;
}

[Serializable]
public class DataDictionary<TKey, TValue>
{
	public TKey[] keys;
	public TValue[] values;

	public DataDictionary()
	{
		keys = new TKey[0];
		values = new TValue[0];
	}

	public DataDictionary(Dictionary<TKey, TValue> dictionary)
	{
		keys = new TKey[dictionary.Count];
		values = new TValue[dictionary.Count];
		int i = 0;
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			keys[i] = pair.Key;
			values[i] = pair.Value;
			i++;
		}
	}

	public Dictionary<TKey, TValue> ToDictionary()
	{
		return new Dictionary<TKey, TValue>(keys.Select((key, i) => new KeyValuePair<TKey, TValue>(key, values[i])));
	}
}

[Serializable]
public class Map
{
	public Zone[] zones = new Zone[0];
	/// <summary>
	/// Entities to save/load (DataManager uses its own variable b/c dynamic list)
	/// </summary>
	public DataDictionary<string, string>[] entities = new DataDictionary<string, string>[0];
}

[Serializable]
public class Zone
{
	public int level;
	public int posX;
	public int posY;
	public int sizeX;
	public int sizeY;
	// tiles.Length = sizeX * sizeY
	public string[] tiles;
}