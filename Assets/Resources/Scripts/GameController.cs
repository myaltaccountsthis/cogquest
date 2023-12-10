using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    public string[] TileNames;
    public TileBase[] Tiles;
    public TextAsset mapFile;

    public Dictionary<string, TileBase> tiles;

    private CameraManager cameraManager;
    private DataManager dataManager;
    private Tilemap tilemap;

    private Map map;

	void Awake()
	{
        cameraManager = GameObject.FindWithTag("MainCamera").GetComponent<CameraManager>();
        dataManager = GameObject.Find("Init").GetComponent<DataManager>();
        tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
        tiles = new Dictionary<string, TileBase>();
        for (int i = 0; i < TileNames.Length; i++)
            tiles.Add(TileNames[i], Tiles[i]);
        LoadMapUncompressed();

        //map.zones = new Zone[] { new Zone() };
        //SaveMapUncompressed();
    }

	// Start is called before the first frame update
	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadMapUncompressed()
    {
        map = new Map();
        JsonUtility.FromJsonOverwrite(mapFile.text, map);
        foreach (Zone zone in map.zones)
        {
            for (int i = 0; i < zone.sizeY; i++)
            {
                for (int j = 0; j < zone.sizeX; j++)
                {
                    tilemap.SetTile(new Vector3Int(zone.posX + j, zone.posY + i), tiles[zone.tiles[i * zone.sizeX + j]]);
                }
            }
        }
    }

    private void LoadMapCompressed()
    {

    }

    // for testing
    private void SaveMapUncompressed()
    {
        FileLoader.WriteFile(Application.persistentDataPath + "/testmap.txt", JsonUtility.ToJson(map));
    }
}

[Serializable]
public class Map
{
    public Zone[] zones = new Zone[0];

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