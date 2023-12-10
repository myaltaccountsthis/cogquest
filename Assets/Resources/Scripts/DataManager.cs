using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.IO.Compression;
using System.Linq;
using System.Data;

public class DataManager : MonoBehaviour
{
    public GameData gameData { get; private set; }
	public Dictionary<string, int> resources;

    private string FILE_PATH;

	void Awake()
	{
        FILE_PATH = Application.persistentDataPath + "/gamedata.dat";
		resources = new Dictionary<string, int>();
		LoadData();
	}

	void OnApplicationQuit()
	{
		SaveData();
	}

	// Data Save/Load functions

	private void LoadData()
	{
		gameData = new GameData();
		try
		{
			if (File.Exists(FILE_PATH))
			{
				FileLoader.ReadOverwriteJson(FILE_PATH, gameData);
				
				// Update dictionaries
				resources = gameData.resources.ToDictionary();
				Debug.Log("Loaded data");
				//Debug.Log(JsonUtility.ToJson(gameData));
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("Failed to load data, resorting to default values.\n" + e.ToString());
		}
	}

	public void SaveData()
	{
		try
		{
			// Update dictionaries
			gameData.resources = new DataDictionary<string, int>(resources);
			
			FileLoader.WriteAndCompressJson(FILE_PATH, gameData);
			Debug.Log("Saved data");
			//Debug.Log(JsonUtility.ToJson(gameData));
		}
		catch (Exception e)
		{
			Debug.LogWarning("Failed to save data.\n" + e.ToString());
		}
	}

	[Serializable]
	public class GameData
    {
		public int score = 0;
		public float timer = 180f;
		public int unlockedZones = 1;
		// resources is not directly editable, need to instantiate a new one on save
		public DataDictionary<string, int> resources = new DataDictionary<string, int>();
		public Entity[] entities;
		public Map map;
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
			int i = 0;
			foreach (TKey key in dictionary.Keys)
			{
				keys[i] = key;
				values[i] = dictionary[key];
				i++;
			}
		}

		public Dictionary<TKey, TValue> ToDictionary()
		{
			return new Dictionary<TKey, TValue>(keys.Select((key, i) => new KeyValuePair<TKey, TValue>(key, values[i])));
		}
	}
}
