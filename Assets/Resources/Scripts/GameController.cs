using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
	private GraphicRaycaster graphicRaycaster;
	private Transform topbar;
	private Transform healthBar;
	private Transform healthBarInner;
	private Timer timer;
	private Dictionary<string, Resource> resourcesUI;
	public Dictionary<BuildingCategory, List<Building>> categoryPrefabs { get; private set; }

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

	public static int entityLayerMask { get; private set; }
	public static int buildingLayerMask { get; private set; }
	public static int buildingLayerID { get; private set; }

	// Build Menu stuff
	/// <summary>
	/// Selection box for delete building indicator when hovering over building
	/// </summary>
	private SpriteRenderer selectionBox;
	/// <summary>
	/// Translucent building that follows the mouse indicating where a building should be placed
	/// </summary>
	private Building selectedBuilding;
	private Color selectedBuildingDefaultColor = new Color(1f, 1f, 1f, .5f);
	private Color selectedBuildingInvalidColor = new Color(1f, 0f, 0f, .5f);
	public BuildAction currentBuildAction { get; private set; }
	private Entity hoveredEntity;
	
	// Time
	private float time = 0;

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
		graphicRaycaster = GetComponent<GraphicRaycaster>();
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
		// Load building categories
		categoryPrefabs = new Dictionary<BuildingCategory, List<Building>>();
		foreach (BuildingCategory category in Enum.GetValues(typeof(BuildingCategory)))
			categoryPrefabs.Add(category, new List<Building>());
		foreach (Building building in Resources.LoadAll<Building>("Prefabs/Buildings"))
		{
			categoryPrefabs[building.category].Add(building);
		}

		// Load camera
		camera = Camera.main;
		cameraSize = camera.orthographicSize;
		cameraSpeed = 16f / cameraSize;
		// used as a placeholder value for "null"
		mouseStart = mouseStartInactive;

		entityLayerMask = LayerMask.GetMask("Buildings", "Units");
		buildingLayerMask = LayerMask.GetMask("Buildings");
		buildingLayerID = LayerMask.NameToLayer("Buildings");

		selectionBox = GameObject.FindWithTag("SelectionBox").GetComponent<SpriteRenderer>();
		hoveredEntity = null;
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
		// Update selected building position
		if (currentBuildAction == BuildAction.Build && selectedBuilding != null)
		{
			Vector2 size = selectedBuilding.GetComponent<BoxCollider2D>().size;
			Vector3 pos = GetMousePlanePosition(Input.mousePosition);
			selectedBuilding.transform.position = RoundToGrid(pos, size);
			if (selectedBuilding.IsValidLocation(tilemap))
			{
				selectedBuilding.SetSpriteColor(selectedBuildingDefaultColor);
			}
			else
			{
				selectedBuilding.SetSpriteColor(selectedBuildingInvalidColor);
			}
		}

		dataManager.gameData.timer -= Time.deltaTime;
		// TEMPORARY TIMER RESET
		if (dataManager.gameData.timer < 0)
			dataManager.gameData.timer += 120f;
		timer.SetTime(dataManager.gameData.timer);

		// Detect hovered entity
		Vector3 cameraCenter = camera.transform.position;
		RaycastHit2D result = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, entityLayerMask);
		if (result.collider != null && currentBuildAction != BuildAction.Build)
		{
			// Update health bar position and indicator
			Entity entity = result.collider.GetComponent<Entity>();
			float healthFraction = entity.HealthFraction;
			healthBar.position = result.transform.position + Vector3.up * (result.collider.bounds.extents.y + .8f * healthBar.localScale.x);
			healthBarInner.localScale = new Vector3(entity.HealthFraction, 1, 1);
			healthBarInner.localPosition = Vector3.right * (healthFraction / 2 - .5f);
			healthBar.gameObject.SetActive(true);
			hoveredEntity = entity;
			
			if (currentBuildAction == BuildAction.Delete && entity.deletable)
			{
				selectionBox.transform.position = result.transform.position;
				selectionBox.transform.localScale = (Vector3)result.collider.GetComponent<BoxCollider2D>().size + new Vector3(.1f, .1f, 1);
				selectionBox.enabled = true;
			}
		}
		else
		{
			healthBar.gameObject.SetActive(false);
			selectionBox.enabled = false;
			hoveredEntity = null;
		}

		// Left drag or build
		if (Input.GetMouseButton(0))
		{
			bool mouseWasPressed = Input.GetMouseButtonDown(0);
			// On mouse down, if mouse was previously up
			PointerEventData ped = new PointerEventData(null);
			ped.position = Input.mousePosition;
			List<RaycastResult> uiElements = new List<RaycastResult>();
			graphicRaycaster.Raycast(ped, uiElements);
			if (mouseWasPressed && uiElements.Count == 0)
			{
				// MOUSE DOWN EVENT
				if (currentBuildAction == BuildAction.Build && selectedBuilding != null)
				{
					// Perform build action if applicable
					if (PlaceBuilding())
					{
						// this might not be necessary
					}
				}
				else if (currentBuildAction == BuildAction.Delete && hoveredEntity != null)
				{
					DeleteSelectedBuilding();
				}
				else
				{
					// set start position for camera pan
					mouseStart = GetMousePlanePosition(Input.mousePosition);
				}
				// MOUSE DOWN EVENT END
			}
			
			// If camera pan active (but not first frame)
			if (!mouseWasPressed && mouseStart != mouseStartInactive)
			{
				// TODO check very slight camera shake. camera shake does not occur if camera is orthographic
				Vector3 mouseEnd = GetMousePlanePosition(Input.mousePosition);
				cameraCenter -= mouseEnd - mouseStart;
				// Realign camera start to current camera bc (mouseEnd - mouseStart) will now be 0
			}
		}
		else
		{
			// Stop camera pan
			mouseStart = mouseStartInactive;
			
		}
		if (mouseStart == mouseStartInactive)
		{
			// Camera keyboard movement
			Vector3 direction = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
			if (direction.sqrMagnitude > 0f)
			{
				if (direction.sqrMagnitude > 1f)
					direction.Normalize();
				cameraCenter += direction * (camera.orthographicSize * cameraSpeed * Time.deltaTime);
			}
		}

		// Resource gain and coal use each second
		if ((int)(time + Time.deltaTime) > (int)time)
        {
			// Get value change for all resources each second
			Dictionary<string, int> resources = new Dictionary<string, int>();
			foreach (string resource in dataManager.resources.Keys)
				resources.Add(resource, 0);

			// Get all buildings and subtract coal use
			foreach (Entity entity in entities)
            {
				if (entity.gameObject.layer == buildingLayerID)
                {
					Building building = (Building)entity;
					resources["Coal"] -= building.CoalUse;

					// Add mined resources if the building is a mine
					if (entity is Mine mine)
					{
						foreach (string resource in mine.resources.Keys)
						{
							resources[resource] += mine.resources[resource] * mine.MineSpeed;
						}
					}
                }
            }
			foreach (string resource in resources.Keys)
				dataManager.resources[resource] += resources[resource];
			// remove when UI added
			Debug.Log(string.Join(", ", resources.Keys) + ": " + string.Join(", ", resources.Values));
			UpdateResourcesUI();
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
		time += Time.deltaTime;
	}

	void OnApplicationQuit()
	{
		SaveData();
	}

	public void OnPointerDown()
	{
	}

	public void OnPointerUp()
	{
		
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
		dataManager.gameData.map.entities = SaveEntitiesToData();
		dataManager.SaveData();
		dataManager.SaveMapUncompressed();
	}

	private void LoadMapTiles()
	{
		tilemap.ClearAllTiles();
		foreach (Zone zone in dataManager.gameData.map.zones)
		{
			for (int i = 0; i < zone.sizeY; i++)
			{
				for (int j = 0; j < zone.sizeX; j++)
				{
					tilemap.SetTile(new Vector3Int(zone.posX + j, zone.posY + zone.sizeY - i - 1), tiles[zone.tiles[i][j]]);
				}
			}
		}
	}

	private Entity AddEntity(Dictionary<string, string> entityData)
	{
		Entity entity = Instantiate(entityPrefabs[entityData["class"]], entityFolder);
		entity.LoadEntitySaveData(entityData);
		entities.Add(entity);
		return entity;
	}

	private void LoadEntitiesFromData()
	{
		foreach (Dictionary<string, string> entityData in dataManager.gameData.map.entities.Select(data => data.ToDictionary()))
		{
			AddEntity(entityData);
		}
	}

	private DataDictionary<string, string>[] SaveEntitiesToData()
	{
		return entities.Select(entity => new DataDictionary<string, string>(entity.GetEntitySaveData())).ToArray();
	}

	public Vector3 GetMousePlanePosition(Vector3 mousePosition)
	{
		Ray ray = camera.ScreenPointToRay(mousePosition);
		return new Vector2(ray.origin.x, ray.origin.y);
	}

	public Vector3 RoundToGrid(Vector3 pos, Vector2 size)
	{
		float offsetX = size.x % 2 / 2, offsetY = size.y % 2 / 2;
		return new Vector3(Mathf.Round(pos.x - offsetX) + offsetX, Mathf.Round(pos.y - offsetY) + offsetY, 0);
	}

	/// <summary>
	/// Place the selected building
	/// </summary>
	/// <returns>true if placement was successful</returns>
	public bool PlaceBuilding()
	{
		if (!selectedBuilding.IsValidLocation(tilemap))
			return false;

		// Check if player has sufficient resources
		if (!selectedBuilding.Cost.All(pair => dataManager.resources[pair.Key] >= pair.Value))
			return false;

        foreach (KeyValuePair<string, int> resourceCost in selectedBuilding.Cost)
        {
			dataManager.resources[resourceCost.Key] -= resourceCost.Value;
        }

		Entity entity = AddEntity(selectedBuilding.GetEntitySaveData());
		if (entity is Mine mine)
        {
			mine.UpdateResources(tilemap);
        }

		return true;
	}

	public void DeleteSelectedBuilding()
	{
		if (hoveredEntity == null)
			return;

		if (!hoveredEntity.deletable)
			return;

		Building building = (Building)hoveredEntity;

		// Refund player for half the building's resources, based on health
		float refundPercent = building.HealthFraction / 2f;
		foreach (KeyValuePair<string, int> resourceCost in building.Cost)
		{
			dataManager.resources[resourceCost.Key] += Mathf.FloorToInt(resourceCost.Value * refundPercent);
		}

		OnBuildingDestroyed(building);
	}

	public void OnBuildingDestroyed(Building building)
	{
		entities.Remove(building);
		Destroy(building.gameObject);
	}
	
	/// <summary>
	/// Called when the player selects one of the buildings from the build menu
	/// </summary>
	/// <param name="building">Prefab of the selected building</param>
	public void SelectBuilding(Building building)
	{
		if (currentBuildAction != BuildAction.Build)
			return;

		if (selectedBuilding != null)
		{
			if (selectedBuilding.ENTITY_NAME == building.ENTITY_NAME)
				return;
			Destroy(selectedBuilding.gameObject);
		}

		selectedBuilding = Instantiate(building);
		SpriteRenderer spriteRenderer = selectedBuilding.GetComponent<SpriteRenderer>();
		spriteRenderer.color = selectedBuildingDefaultColor;
		spriteRenderer.sortingLayerName = "Buildings";
		spriteRenderer.sortingOrder = 10;
		spriteRenderer.gameObject.layer = LayerMask.NameToLayer("Default");
	}

	public void SelectBuildAction(BuildAction buildAction)
	{
		if (buildAction != currentBuildAction)
		{
			currentBuildAction = buildAction;

			switch (buildAction)
			{
				case BuildAction.Build:
					break;
				case BuildAction.Delete:
					break;
				case BuildAction.Pan:
					break;
			}

			if (currentBuildAction != BuildAction.Build)
			{
				if (selectedBuilding != null)
				{
					Destroy(selectedBuilding.gameObject);
					selectedBuilding = null;
				}
			}
		}
	}
}

public static class Extensions
{
	public static BoundsInt ColliderToBoundsInt(this Collider2D collider)
	{
		Vector3 size = collider.bounds.extents * 2;
		Vector3 min = collider.transform.position - size / 2;
		return new BoundsInt(Mathf.RoundToInt(min.x), Mathf.RoundToInt(min.y), Mathf.RoundToInt(min.z),
			Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y), Mathf.Max(1, Mathf.RoundToInt(size.z)));
	}

	public static void SetSpriteColor(this Building building, Color color)
	{
		building.GetComponent<SpriteRenderer>().color = color;
	}
}