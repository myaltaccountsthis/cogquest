using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
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
    public Tilemap tilemap { get; private set; }

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
	private State state;
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
	public static int buildingShadowLayerMask { get; private set; }
	public static int shadowLayerMask { get; private set; }
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
	
	// Build and Delete Building Audio
	public AudioSource buildAudio;
	public Vector2 buildAudioRange = new(-1, -1);
	private Coroutine buildAudioCoroutine;

	public AudioSource destroyAudio;
	public Vector2 destroyAudioRange = new(-1, -1);
	private Coroutine destroyAudioCoroutine;

	void Awake()
	{
        dataManager = GameObject.Find("Init").GetComponent<DataManager>();
		buildMenu = GameObject.Find("BuildMenu").GetComponent<BuildMenu>();
		spawnMenu = GameObject.Find("SpawnMenu").GetComponent<SpawnMenu>();
        tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
		shadows = new List<Image>();
		shadowFolder = GameObject.Find("Shadows").GetComponent<Transform>();
		shadow = Resources.Load<GameObject>("Prefabs/Shadow").GetComponent<Image>();

		// Load tiles
		tiles = new Dictionary<char, TileBase>();
		for (int i = 0; i < TileNames.Length; i++)
			tiles.Add(TileNames[i], Tiles[i]);

		// Load entities
		entities = new List<Entity>();
		entityFolder = GameObject.Find("Entities").transform;
		entityPrefabs = new Dictionary<string, Entity>();

		List<Entity> entitiesToLoad = new List<Entity>();
		entitiesToLoad.AddRange(Resources.LoadAll<Entity>("Prefabs/Buildings"));
		entitiesToLoad.AddRange(Resources.LoadAll<Entity>("Prefabs/Units"));
		entitiesToLoad.AddRange(Resources.LoadAll<Entity>("Prefabs/Bullets"));
		foreach (Entity entity in entitiesToLoad)
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
		state = middleUI.Find("State").GetComponent<State>();

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

		entityLayerMask = LayerMask.GetMask("Buildings", "Units", "PlayerUnits");
		buildingShadowLayerMask = LayerMask.GetMask("Buildings", "Shadow");
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
		UpdateShadows();
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
		
		// audio start and end pos
		buildAudio.time = (buildAudioRange.x < 0 || buildAudioRange.x > buildAudio.clip.length) ? 0 : buildAudioRange.x;
		destroyAudio.time = (destroyAudioRange.x < 0 || destroyAudioRange.x > destroyAudio.clip.length) ? 0 : destroyAudioRange.x;
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

		timer.SetTime(dataManager.gameData.timer);
		state.UpdateState(dataManager.gameData.timer);

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
				else if (hoveredEntity != null && hoveredEntity.team == 0)
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
		// Rally units
		if (Input.GetMouseButtonDown(1) && !mouseOnUI)
		{
			bool rallyAll = Input.GetKey(KeyCode.LeftShift);
			Vector2 min = GetMousePlanePosition(new Vector3(0, 0)), max = GetMousePlanePosition(new Vector3(Screen.width, Screen.height));
			Rect cameraRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
			Unit[] units = entities.Where(entity => entity is Unit unit && unit.active && unit.team == 0 &&
				(rallyAll ? true : cameraRect.Contains(unit.transform.position)))
				.Select(entity => (Unit)entity).ToArray();
			foreach (Unit unit in units)
			{
				unit.Patrol(new Vector2[] { (Vector2)GetMousePlanePosition(Input.mousePosition) + UnityEngine.Random.insideUnitCircle * 2f });
			}
		}

		// Resource gain and coal use each second
		if (IntervalPassed(1))
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
						if (!fort.Occupied)
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

            if (dataManager.resources["Coal"] + resources["Coal"] >= 0)
            {
	            foreach (string resource in resources.Keys)
		            dataManager.resources[resource] += resources[resource];
            }
            else dataManager.resources["Coal"] = 0;
            // remove when UI added
			//Debug.Log(string.Join(", ", dataManager.resources.Keys) + ": " + string.Join(", ", dataManager.resources.Values));
			UpdateResourcesUI();
        }

		// Random enemy spawning (only if timer is negative)
		if (dataManager.gameData.timer - Time.deltaTime <= 0 && IntervalPassed(dataManager.gameData.map.enemySpawnInterval))
		{
			foreach (Fort fort in entities.Where(entity => entity is Fort fort && !fort.Occupied && fort.Tier <= dataManager.gameData.map.furthestZone + 1)
				.Select(entity => (Fort)entity).ToArray())
			{
				SpawnRandomEnemyUnit(fort, 1 + fort.Tier);
			}
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
		dataManager.gameData.timer -= Time.deltaTime;
	}

	void OnApplicationQuit()
	{
		SaveData();
	}

	void OnGUI()
	{
		float newFPS = 1.0f / Time.deltaTime;
		fps = Mathf.Lerp(fps, newFPS, 0.03f);
        GUI.Label(new Rect(2, Screen.height - 22, 15, 20), ((int)fps).ToString());
	}

	/// <summary>
	/// Call this every frame to check if an interval (e.g. every second) has been passed
	/// </summary>
	/// <param name="interval">Period of how often true should be returned</param>
	/// <returns>True if the interval was passed</returns>
	public bool IntervalPassed(float interval)
	{
		// Add constant so all numbers are positive
		float t = -dataManager.gameData.timer + 300f;
		return (t + Time.deltaTime) % interval < t % interval;
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
		return entities.Where(entity => entity != null && entity.isActiveAndEnabled).Select(entity => new DataDictionary<string, string>(entity.GetEntitySaveData())).ToArray();
	}

	public Vector3 GetMousePlanePosition(Vector3 mousePosition)
	{
		//Ray ray = camera.ScreenPointToRay(mousePosition);
		//return new Vector2(ray.origin.x, ray.origin.y);
		Vector3 point = camera.ScreenToWorldPoint(mousePosition);
		return new Vector2(point.x, point.y);
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

		buildAudio.Play();
		if (buildAudioRange.y > 0)
		{
			if (buildAudioCoroutine != null)
			{
				StopCoroutine(buildAudioCoroutine);
				buildAudioCoroutine = null;
			}
			buildAudioCoroutine = StartCoroutine(StopAudioAfterOffset(buildAudio, buildAudioRange.y - Math.Max(0, buildAudioRange.x)));
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

		UpdateResourcesUI();

		destroyAudio.Play();
		if (destroyAudioRange.y > 0)
		{
			if (destroyAudioCoroutine != null)
			{
				StopCoroutine(destroyAudioCoroutine);
				destroyAudioCoroutine = null;
			}
			destroyAudioCoroutine = StartCoroutine(StopAudioAfterOffset(destroyAudio, destroyAudioRange.y - Math.Max(0, destroyAudioRange.x)));
		}
	}

	IEnumerator StopAudioAfterOffset(AudioSource audio, float offset)
	{
		yield return new WaitForSeconds(offset);
		audio.Stop();
		if (audio == buildAudio) buildAudioCoroutine = null;
		if (audio == destroyAudio) destroyAudioCoroutine = null;
	}

	public void OnBuildingDestroyed(Building building)
	{
		entities.Remove(building);
		Destroy(building.gameObject);
	}

	/// <summary>
	/// If enemy fort was damaged, set timer to 0
	/// </summary>
	public void OnEnemyFortDamaged()
	{
		if (dataManager.gameData.timer > 0f)
			dataManager.gameData.timer = .001f;
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
		int furthestZoneIndex = dataManager.gameData.map.furthestZone;
		bool[] unlockedForts = entities.Where(entity => entity is Fort fort).Select(entity => (Fort)entity)
			.OrderBy(fort => fort.Tier).Select(fort => fort.Occupied).ToArray();
		for (int i = 0; i < dataManager.gameData.map.zones.Length; i++)
		{
			if (shadows.Count <= i)
			{
				Zone zone = dataManager.gameData.map.zones[i];
				shadows.Add(Instantiate(shadow, shadowFolder));
				RectTransform rectTransform = shadows[i].GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2(zone.sizeX, zone.sizeY);
				rectTransform.position = new Vector3(zone.posX + zone.sizeX / 2f, zone.posY + zone.sizeY / 2f);
				shadows[i].GetComponent<BoxCollider2D>().size = rectTransform.sizeDelta;
			}
			// Transparent if previous zone, otherwise negative rational function becoming more opaque
			float alpha = (i <= furthestZoneIndex + 1) ? 0 :
				1 - .1f / (i - furthestZoneIndex - 1);
			shadows[i].color = new Color(0, 0, 0, alpha);
			shadows[i].GetComponent<BoxCollider2D>().enabled = !unlockedForts[i];
		}
	}

	/// <summary>
	/// Sets the resource cost shown in the build menu to this resource cost. Used for hovering in game.
	/// </summary>
	public void SetHoveredResourceCost(Entity entity)
	{
		buildMenu.SetHoveredResourceCost(entity);
	}

	public bool SpawnPlayerUnit(Fort fort, Unit unit)
	{
		if (!unit.Cost.All(pair => dataManager.resources[pair.Key] >= pair.Value))
			return false;

		foreach (KeyValuePair<string, int> resourceCost in unit.Cost)
		{
			dataManager.resources[resourceCost.Key] -= resourceCost.Value;
		}

		Dictionary<string, string> saveData = new Dictionary<string, string>()
		{
			{ "patrolWaypoints", Unit.PatrolWaypointsToString(new Vector2[] { fort.transform.position }) },
			{ "patrolMode", "Point" }
		};

		SpawnUnit(fort, unit, saveData);

		UpdateResourcesUI();
		return true;
	}

	/// <summary>
	/// Spawn a unit with supplied saveData (does not require team, class, posX, posY)
	/// </summary>
	/// <param name="fort">Fort to spawn at for team number</param>
	/// <param name="unit">Unit prefab</param>
	/// <param name="saveData">Additional save data (patrolWaypoints, patrolMode)</param>
	public void SpawnUnit(Fort fort, Unit unit, Dictionary<string, string> saveData)
	{
		Vector2 randomSpawn = (Vector2)fort.transform.position + UnityEngine.Random.insideUnitCircle * 2f;

		saveData["posX"] = randomSpawn.x.ToString();
		saveData["posY"] = randomSpawn.y.ToString();
		saveData["team"] = fort.team.ToString();
		saveData["class"] = unit.entityName;
		AddEntity(saveData);
	}

	public void SpawnAggroEnemyUnit(Fort fort, Unit unit)
	{
		// Select previous forts (Tier < fort.Tier) and arrange them in descending order (furthest from player fort first)
		Vector2[] previousForts = entities.Where(entity => entity is Fort f && f.Tier < fort.Tier)
			.OrderByDescending(entity => ((Fort)entity).Tier)
			.Select(entity => (Vector2)entity.transform.position).ToArray();
		Dictionary<string, string> saveData = new Dictionary<string, string>()
		{
			{ "patrolWaypoints", Unit.PatrolWaypointsToString(previousForts) },
			{ "patrolMode", "Waypoints" }
		};

		SpawnUnit(fort, unit, saveData);
	}

	public void OpenSpawnMenu(Fort fort)
	{
		// TODO change 0 to latest zone unlocked
		spawnMenu.OpenMenu(fort, GetAvailableUnits());
	}

	/// <summary>
	/// Returns an enumerable with available units to spawn
	/// </summary>
	/// <param name="fort">Enemy fort, or null if player</param>
	public IEnumerable<Unit> GetAvailableUnits(Fort fort = null)
	{
		return unitPrefabs.Where(unit => (fort == null ? dataManager.gameData.map.furthestZone : fort.Tier) >= unit.zoneToUnlock);
	}

	public void CloseSpawnMenu()
	{
		spawnMenu.CloseMenu();
	}

	public void SpawnRandomEnemyUnit(Fort fort, int count = 1)
	{
		Unit[] units = GetAvailableUnits(fort).ToArray();
		for (int i = 0; i < count; i++)
		{
			SpawnAggroEnemyUnit(fort, units[UnityEngine.Random.Range(0, units.Length - 1)]);
		}
	}

	/// <summary>
	/// Get the layer mask that encompasses the team
	/// </summary>
	/// <param name="team">0 or 1 (0 is player)</param>
	public static LayerMask GetTeamMask(int team)
	{
		return team == 0 ? LayerMask.GetMask("PlayerUnits", "Buildings") : LayerMask.GetMask("Units", "Buildings");
	}

	public static LayerMask GetOtherTeamMask(int team)
	{
		return team == 0 ? LayerMask.GetMask("Units", "Buildings") : LayerMask.GetMask("PlayerUnits", "Buildings");
	}

	public void OnFortOccupied(Fort fort)
	{
		UnlockNewZone(fort.Tier);
	}

	private void UnlockNewZone(int zone)
	{
		if (zone > dataManager.gameData.map.furthestZone)
		{
			dataManager.gameData.map.furthestZone++;
			dataManager.gameData.timer = zone * 30f + 60f;
			UpdateShadows();
			if (zone == dataManager.gameData.map.zones.Length - 1)
			{
				// On game win
				Debug.Log("Won the game");
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

	public static Collider2D[] GetCollisions(this Collider2D collider, int team)
	{
		Collider2D[] colliders = new Collider2D[16];
		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.SetLayerMask(GameController.GetOtherTeamMask(team));
		contactFilter.useTriggers = true;
		Physics2D.OverlapCollider(collider, contactFilter, colliders);
		return colliders;
	}
}