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
	private BuildMenu buildMenu;
	private SpawnMenu spawnMenu;

	// Tile loading
	public char[] TileNames;
	public TileBase[] Tiles;
	public Dictionary<char, TileBase> tiles;
    private Tilemap tilemap;

	// Entity loading
	[HideInInspector] public List<Entity> entities;
	public Dictionary<string, Entity> entityPrefabs { get; private set; }
	private List<Unit> unitPrefabs;
	private Transform entityFolder;

	// UI
	private GraphicRaycaster[] graphicRaycasters;
	private Transform topbar;
	private Transform middleUI;
	private RectTransform healthBar;
	private Transform healthBarInner;
	private Timer timer;
	private Dictionary<string, Resource> resourcesUI;
	public Dictionary<BuildingCategory, List<string>> categoryPrefabs { get; private set; }
	private float fps;

	// Camera controls
	private new Camera camera;
	private float cameraSpeed;
	private float cameraSize;
	private const float minCameraSize = 4f;
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
	private RectTransform selectionBox;
	private Image selectionBoxImage;
	private Color selectionBoxDeleteColor = Color.red;
	private Color selectionBoxHoverColor = Color.white;
	/// <summary>
	/// Translucent building that follows the mouse indicating where a building should be placed
	/// </summary>
	private Building selectedBuilding;
	/// <summary>
	/// Whether or not a building is selected and ready to build on click
	/// </summary>
	public bool IsHoveringBuilding => selectedBuilding != null;
	private Color selectedBuildingDefaultColor = new Color(1f, 1f, 1f, .5f);
	private Color selectedBuildingInvalidColor = new Color(1f, 0f, 0f, .5f);
	public BuildAction currentBuildAction { get; private set; }
	private Entity hoveredEntity;

	private List<Image> shadows;
	private Transform shadowFolder;
	private Image shadow;
	private Entity prevClickedEntity;
	
	// Time
	private float time = 0;

	void Awake()
	{
        dataManager = GameObject.Find("Init").GetComponent<DataManager>();
		buildMenu = GameObject.Find("BuildMenu").GetComponent<BuildMenu>();
		spawnMenu = GameObject.Find("SpawnMenu").GetComponent<SpawnMenu>();
        tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
		shadow = Resources.Load<GameObject>("Prefabs/Shadow").GetComponent<Image>();
		shadowFolder = GameObject.Find("Shadows").GetComponent<Transform>();

		// Load tiles
		tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
		tiles = new Dictionary<char, TileBase>();
		for (int i = 0; i < TileNames.Length; i++)
			tiles.Add(TileNames[i], Tiles[i]);

		// Load entities - TODO load units
		entities = new List<Entity>();
		entityFolder = GameObject.Find("Entities").transform;
		entityPrefabs = new Dictionary<string, Entity>();
		foreach (Entity entity in Resources.LoadAll<Entity>("Prefabs/Buildings")
			.Concat(Resources.LoadAll<Entity>("Prefabs/Units"))
			.Concat(new List<Entity> { Resources.Load<Entity>("Prefabs/Bullet") }))
		{
			entityPrefabs.Add(entity.entityName, entity);
		}
		unitPrefabs = entityPrefabs.Values.Where(entity => entity is Unit).Select(entity => (Unit)entity).ToList();

		// Load UI
		graphicRaycasters = new GraphicRaycaster[] { GetComponent<GraphicRaycaster>(), GameObject.Find("WorldCanvas").GetComponent<GraphicRaycaster>() };
		topbar = transform.Find("Topbar");
		middleUI = topbar.Find("Middle");
		healthBar = GameObject.Find("HealthBar").GetComponent<RectTransform>();
		healthBarInner = GameObject.Find("HealthBarInner").transform;
		healthBar.gameObject.SetActive(false);
		timer = middleUI.Find("Timer").GetComponent<Timer>();
		// Loads all resource components into dictionary
		resourcesUI = new Dictionary<string, Resource>(
			topbar.Find("Resources").GetComponentsInChildren<Resource>()
				.Select(resource => new KeyValuePair<string, Resource>(resource.name, resource))
		);
		// Load building categories
		categoryPrefabs = new Dictionary<BuildingCategory, List<string>>();
		foreach (BuildingCategory category in Enum.GetValues(typeof(BuildingCategory)))
			categoryPrefabs.Add(category, new List<string>());
		foreach (Building building in Resources.LoadAll<Building>("Prefabs/Buildings"))
		{
			categoryPrefabs[building.category].Add(building.entityName);
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

		selectionBox = GameObject.FindWithTag("SelectionBox").GetComponent<RectTransform>();
		selectionBoxImage = selectionBox.Find("Image").GetComponent<Image>();
		selectionBox.gameObject.SetActive(false);
		hoveredEntity = null;
		prevClickedEntity = null;
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

		// TEMP TESTING STUFF
		//AddEntity(new Dictionary<string, string>()
		//{
		//	{ "posX", "2" },
		//	{ "posY", "2" },
		//	{ "team", "1" },
		//	{ "class", "TestUnit" }
		//});
    }

    // Update is called once per frame
    void Update()
	{
		entities.RemoveAll(entity => entity == null || !entity.isActiveAndEnabled);
		
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

		PointerEventData ped = new PointerEventData(null);
		ped.position = Input.mousePosition;
		List<RaycastResult> uiElements = new List<RaycastResult>();
		foreach (GraphicRaycaster graphicRaycaster in graphicRaycasters)
			graphicRaycaster.Raycast(ped, uiElements);
		bool mouseOnUI = uiElements.Count != 0;

		// Detect hovered entity
		Vector3 cameraCenter = camera.transform.position;
		RaycastHit2D result = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, entityLayerMask);
		// check if hovering entity and not in build mode
		if (result.collider != null && currentBuildAction != BuildAction.Build)
		{
			// Update health bar position and indicator
			Entity entity = result.collider.GetComponent<Entity>();
			float healthFraction = entity.HealthFraction;
			healthBar.anchoredPosition = result.transform.position + Vector3.up * result.collider.bounds.extents.y;
			//healthBar.position = result.transform.position + Vector3.up * (result.collider.bounds.extents.y + .8f * healthBar.localScale.x);
			healthBarInner.localScale = new Vector3(healthFraction, 1, 1);
			//healthBarInner.localPosition = Vector3.right * (healthFraction / 2 - .5f);
			healthBar.gameObject.SetActive(true);
			hoveredEntity = entity;
			
			if (entity is Building)
			{
				selectionBox.anchoredPosition = result.transform.position;
				selectionBox.sizeDelta = result.collider.GetComponent<BoxCollider2D>().size;
				//selectionBox.transform.localScale = (Vector3)result.collider.GetComponent<BoxCollider2D>().size + new Vector3(.1f, .1f, 1);
				selectionBox.gameObject.SetActive(true);

				selectionBoxImage.color = currentBuildAction == BuildAction.Delete && entity.deletable ? selectionBoxDeleteColor : selectionBoxHoverColor;
			}

			// Update info text
			buildMenu.SetHoveredResourceCost(entity, false);
		}
		// if not hovering building
		else
		{

			if (currentBuildAction != BuildAction.Build && hoveredEntity != null)
			{
				buildMenu.mouseHoveredEntity = null;
				buildMenu.UpdateInfo();
			}

			healthBar.gameObject.SetActive(false);
			selectionBox.gameObject.SetActive(false);
			hoveredEntity = null;
		}

		// Left drag or build
		if (Input.GetMouseButton(0))
		{
			bool mouseWasPressed = Input.GetMouseButtonDown(0);
			// On mouse down, if mouse was previously up

			if (mouseWasPressed && !mouseOnUI)
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
				else if (hoveredEntity != null)
				{
					hoveredEntity.DoMouseDown();
					prevClickedEntity = hoveredEntity;
				}
				else if (prevClickedEntity != null)
				{
					prevClickedEntity.DoMouseCancel();
					prevClickedEntity = null;
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
				// Entity active check just in case (if in the future an entity exists but is not active)
				if (entity.active && entity.gameObject.layer == buildingLayerID)
				{
					Building building = (Building)entity;
					resources["Coal"] -= building.CoalUse;

					if (entity is Fort fort)
					{
						if (!fort.occupied)
							resources["Coal"] += fort.CoalUse;
					}

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
			//Debug.Log(string.Join(", ", dataManager.resources.Keys) + ": " + string.Join(", ", dataManager.resources.Values));
			UpdateResourcesUI();
        }

		if (!mouseOnUI)
		{
			// zoom, shift to keep cursor at same world point
			Vector3 worldPoint1 = camera.ScreenToWorldPoint(Input.mousePosition);
			cameraSize = Mathf.Clamp(cameraSize * Mathf.Pow(cameraZoomFactor, -Input.mouseScrollDelta.y), minCameraSize, maxCameraSize);
			camera.orthographicSize = cameraSize;

			Vector3 worldPoint2 = camera.ScreenToWorldPoint(Input.mousePosition);
			cameraCenter += worldPoint1 - worldPoint2;
		}

		cameraCenter.x = Mathf.Clamp(cameraCenter.x, bounds.xMin + .5f, bounds.xMax - .5f);
		cameraCenter.y = Mathf.Clamp(cameraCenter.y, bounds.yMin + .5f, bounds.yMax - .5f);
		camera.transform.position = cameraCenter;
		time += Time.deltaTime;
	}

	void OnApplicationQuit()
	{
		SaveData();
	}

	void OnGUI()
	{
		float newFPS = 1.0f / Time.deltaTime;
		fps = Mathf.Lerp(fps, newFPS, 0.03f);
        GUI.Label(new Rect(2, Screen.height - 22, 15, 20), ((int) fps).ToString());
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

	public Entity AddEntity(Dictionary<string, string> entityData)
	{
		Entity entity = Instantiate(entityPrefabs[entityData["class"]], entityFolder);
		entity.LoadEntitySaveData(entityData);
		if (entity is Mine mine)
		{
			mine.UpdateResources(tilemap);
		}
		entities.Add(entity);
		entity.active = true;
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
		UpdateResourcesUI();

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

		UpdateResourcesUI();
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
			if (selectedBuilding.entityName == building.entityName)
				return;
			Destroy(selectedBuilding.gameObject);
		}

		selectedBuilding = Instantiate(building);
		SpriteRenderer spriteRenderer = selectedBuilding.GetComponent<SpriteRenderer>();
		spriteRenderer.sortingLayerName = "Buildings";
		spriteRenderer.sortingOrder = 10;
		spriteRenderer.gameObject.layer = LayerMask.NameToLayer("Default");
		selectedBuilding.SetSpriteColor(selectedBuildingDefaultColor);
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

	public void UpdateShadows()
	{
		int zoneIndex = dataManager.gameData.map.furthestZone;
		for (int i = 0; i < dataManager.gameData.map.zones.Length; i++)
		{
			if (shadows.Count <= i)
			{
				Zone zone = dataManager.gameData.map.zones[i];
				shadows.Add(Instantiate(shadow, shadowFolder));
				RectTransform rectTransform = shadows[i].GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2(zone.sizeX, zone.sizeY);
				rectTransform.position = new Vector3(zone.posX - zone.sizeX / 2, zone.posY - zone.sizeY / 2, 0);
			}
			float alpha = (i - zoneIndex <= 1) ? 0 :
				1 - .1f / (i - zoneIndex - 1);
			shadows[i].color = new Color(0, 0, 0, alpha);
		}
	}

	/// <summary>
	/// Sets the resource cost shown in the build menu to this resource cost. Used for hovering in game.
	/// </summary>
	public void SetHoveredResourceCost(Entity entity)
	{
		buildMenu.SetHoveredResourceCost(entity);
	}

	public bool SpawnUnit(Fort fort, Unit unit)
	{
		if (!unit.Cost.All(pair => dataManager.resources[pair.Key] >= pair.Value))
			return false;

		foreach (KeyValuePair<string, int> resourceCost in unit.Cost)
		{
			dataManager.resources[resourceCost.Key] -= resourceCost.Value;
		}

		Dictionary<string, string> saveData = new Dictionary<string, string>()
		{
			{ "posX", fort.transform.position.x.ToString() },
			{ "posY", fort.transform.position.y.ToString() },
			{ "team", fort.team.ToString() },
			{ "class", unit.entityName },
			{ "patrolWaypoints", Unit.PatrolWaypointsToString(new Vector2[]
				{
					Vector2.up, Vector2.right, Vector2.down, Vector2.left
				}.Select(vec => vec + (Vector2)fort.transform.position).ToArray())
			},
			{ "patrolMode", "Point" }
		};

		AddEntity(saveData);
		UpdateResourcesUI();
		return true;
	}

	public void OpenSpawnMenu(Fort fort)
	{
		// TODO change 0 to latest zone unlocked
		spawnMenu.OpenMenu(fort, unitPrefabs.Where(unit => 0 >= unit.zoneToUnlock));
	}

	public void CloseSpawnMenu()
	{
		spawnMenu.CloseMenu();
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

	public static Vector3 DirectionToEulerAngles(this Vector3 direction)
	{
		return new Vector3(0, 0, direction.DirectionToAngle());
	}

	public static float DirectionToAngle(this Vector3 direction)
	{
		return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
	}

	public static Dictionary<TKey, TValue> ChainAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
	{
		dict.Add(key, value);
		return dict;
	}
}