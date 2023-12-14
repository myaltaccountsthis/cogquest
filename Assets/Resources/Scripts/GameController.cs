using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
	// Managers
	private DataManager dataManager;

	// Tile loading
	public char[] TileNames;
	public TileBase[] Tiles;
	public Dictionary<char, TileBase> tiles;
    private Tilemap tilemap;

	// Entity loading
	[HideInInspector] public List<Entity> entities;
	private Dictionary<string, Entity> entityPrefabs;
	private Transform entityFolder;

	// UI
	private Transform topbar;
	private Transform healthBar;
	private Transform healthBarInner;
	private Timer timer;
	private Dictionary<string, Resource> resourcesUI;

	// Camera controls
	private new Camera camera;
	private float cameraSpeed;
	private float cameraSize;
	private const float minCameraSize = 2f;
	private const float maxCameraSize = 15f;
	private const float cameraZoomFactor = 1.05f;
	private Vector3 mouseStart;

	private BoundsInt bounds;

	private static readonly Vector3 mouseStartInactive = Vector3.back;

	private int entityLayerMask;

	void Awake()
	{
        dataManager = GameObject.Find("Init").GetComponent<DataManager>();
        tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();

		// Load tiles
		tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
		tiles = new Dictionary<char, TileBase>();
		for (int i = 0; i < TileNames.Length; i++)
			tiles.Add(TileNames[i], Tiles[i]);

		// Load entities - TODO load units
		entities = new List<Entity>();
		entityFolder = GameObject.Find("Entities").transform;
		entityPrefabs = new Dictionary<string, Entity>();
		foreach (Entity entity in Resources.LoadAll<Entity>("Prefabs/Buildings"))
		{
			entityPrefabs[entity.ENTITY_NAME] = entity;
		}

		// Load UI
		topbar = transform.Find("Topbar");
		healthBar = GameObject.Find("HealthBar").GetComponent<Transform>();
		healthBarInner = GameObject.Find("HealthBarInner").transform;
		healthBar.gameObject.SetActive(false);
		timer = topbar.Find("Timer").GetComponent<Timer>();
		// Loads all resource components into dictionary
		resourcesUI = new Dictionary<string, Resource>(
			topbar.Find("Resources").GetComponentsInChildren<Resource>()
				.Select(resource => new KeyValuePair<string, Resource>(resource.name, resource))
		);

		// Load camera
		camera = Camera.main;
		cameraSize = camera.orthographicSize;
		cameraSpeed = 16f / cameraSize;
		// used as a placeholder value for "null"
		mouseStart = mouseStartInactive;

		entityLayerMask = LayerMask.GetMask("Entities");
	}

	// Start is called before the first frame update
	void Start()
    {
		LoadMapTiles();
		LoadEntitiesFromData();
		UpdateResourcesUI();
		Zone[] zones = dataManager.gameData.map.zones;
		bounds = new BoundsInt(zones.Min(zone => zone.posX), zones.Min(zone => zone.posY), 0,
			zones.Max(zone => zone.posX + zone.sizeX), zones.Max(zone => zone.posY + zone.sizeY), 0);
		Camera.main.transform.position = dataManager.gameData.cameraPosition;
    }

    // Update is called once per frame
    void Update()
    {
		dataManager.gameData.timer -= Time.deltaTime;
		// TEMPORARY TIMER RESET
		if (dataManager.gameData.timer < 0)
			dataManager.gameData.timer += 120f;
		timer.SetTime(dataManager.gameData.timer);

		Vector3 cameraCenter = camera.transform.position;
		RaycastHit2D result = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, entityLayerMask);
		if (result.collider != null)
		{
			// Update health bar position and indicator
			Entity entity = result.collider.GetComponent<Entity>();
			float healthFraction = entity.HealthFraction;
			healthBar.position = result.transform.position + Vector3.up * (result.collider.bounds.extents.y + .8f * healthBar.localScale.x);
			healthBarInner.localScale = new Vector3(entity.HealthFraction, 1, 1);
			healthBarInner.localPosition = Vector3.right * (healthFraction / 2 - .5f);
			healthBar.gameObject.SetActive(true);
		}
		else
		{
			healthBar.gameObject.SetActive(false);
		}

		// Left drag
		if (Input.GetMouseButton(0))
		{
			// On mouse down, if mouse was previously up
			if (Input.GetMouseButtonDown(0))
			{
				// set start position for camera pan
				mouseStart = GetMousePlanePosition(Input.mousePosition);
			}
			// If camera pan active (but not first frame)
			else if (mouseStart != mouseStartInactive)
			{
				// TODO check very slight camera shake. camera shake does not occur if camera is orthographic
				Vector3 mouseEnd = GetMousePlanePosition(Input.mousePosition);
				cameraCenter -= (mouseEnd - mouseStart);
				// Realign camera start to current camera bc (mouseEnd - mouseStart) will now be 0
			}
		}
		else
		{
			// Stop camera pan
			mouseStart = mouseStartInactive;
			// Camera keyboard movement
			Vector3 direction = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
			if (direction.sqrMagnitude > 0f)
			{
				if (direction.sqrMagnitude > 1f)
					direction.Normalize();
				cameraCenter += direction * (camera.orthographicSize * cameraSpeed * Time.deltaTime);
			}
		}

		// zoom, shift to keep cursor at same world point
		Vector3 worldPoint1 = camera.ScreenToWorldPoint(Input.mousePosition);
		cameraSize = Mathf.Clamp(cameraSize * Mathf.Pow(cameraZoomFactor, -Input.mouseScrollDelta.y), minCameraSize, maxCameraSize);
		camera.orthographicSize = cameraSize;

		Vector3 worldPoint2 = camera.ScreenToWorldPoint(Input.mousePosition);
		cameraCenter += worldPoint1 - worldPoint2;

		cameraCenter.x = Mathf.Clamp(cameraCenter.x, bounds.xMin + .5f, bounds.xMax - .5f);
		cameraCenter.y = Mathf.Clamp(cameraCenter.y, bounds.yMin + .5f, bounds.yMax - .5f);
		camera.transform.position = cameraCenter;
	}

	void OnApplicationQuit()
	{
		SaveData();
	}

	public void UpdateResourcesUI()
	{
		foreach (KeyValuePair<string, Resource> pair in resourcesUI)
		{
			pair.Value.UpdateText(dataManager.resources[pair.Key]);
		}
	}

	public void SaveData()
	{
		SaveEntitiesToData();
		dataManager.SaveData();
		dataManager.SaveMapUncompressed();
	}

	private void LoadMapTiles()
	{
		foreach (Zone zone in dataManager.gameData.map.zones)
		{
			for (int i = 0; i < zone.sizeY; i++)
			{
				for (int j = 0; j < zone.sizeX; j++)
				{
					tilemap.SetTile(new Vector3Int(zone.posX + j, zone.posY + i), tiles[zone.tiles[i][j]]);
				}
			}
		}
	}

	private void LoadEntitiesFromData()
	{
		foreach (Dictionary<string, string> entityData in dataManager.gameData.map.entities.Select(data => data.ToDictionary()))
		{
			Entity entity = Instantiate(entityPrefabs[entityData["class"]], entityFolder);
			entity.LoadEntitySaveData(entityData);
			entity.gameObject.layer = LayerMask.NameToLayer("Entities");
			entities.Add(entity);
		}
	}

	private DataDictionary<string, string>[] SaveEntitiesToData()
	{
		return entities.Select(entity => new DataDictionary<string, string>(entity.GetEntitySaveData())).ToArray();
	}

	private Vector3 GetMousePlanePosition(Vector3 mousePosition)
	{
		Ray ray = camera.ScreenPointToRay(mousePosition);
		return new Vector2(ray.origin.x, ray.origin.y);
	}
}