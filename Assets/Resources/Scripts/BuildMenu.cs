using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BuildMenu : MonoBehaviour
{
	private TextMeshProUGUI categoryName;
	private Transform categories;
	private Transform options;
	private Transform actions;
	private Transform costMenu;
	private Transform costContainer;
	private TextMeshProUGUI optionName;
	private Toggle infoToggle;
	private Transform infoFrame;
	private TextMeshProUGUI infoText;
	private Transform entityFolder;
	private GameController gameController;

	private RectTransform buildingOptionPrefab;
	private ResourceCost resourceCostPrefab;
	private RectTransform buildActionOutline;
	private RectTransform categoryOutline;

	/// <summary>
	/// Option currently shown in the cost menu
	/// </summary>
	private string currentOption;
	private string hoveredOption;
	private string selectedOption;

	private Dictionary<string, Sprite> resourceIcons;

	private const float resourceCostMargin = 4f;

	void Awake()
	{
		categoryName = transform.Find("CategoryName").GetComponent<TextMeshProUGUI>();
		categories = transform.Find("Categories");
		options = transform.Find("Options");
		actions = transform.Find("Actions");
		costMenu = transform.Find("Cost");
		costContainer = costMenu.Find("CostContainer");
		optionName = costMenu.Find("OptionName").GetComponent<TextMeshProUGUI>();
		infoToggle = transform.Find("InfoToggle").GetComponent<Toggle>();
		infoFrame = transform.Find("Info");
		infoText = infoFrame.Find("TextLabel").GetComponent<TextMeshProUGUI>();
		entityFolder = GameObject.Find("Entities").transform;
		gameController = GameObject.Find("Canvas").GetComponent<GameController>();

		buildingOptionPrefab = Resources.Load<RectTransform>("Prefabs/BuildingOption");
		resourceCostPrefab = Resources.Load<ResourceCost>("Prefabs/ResourceCost");
		buildActionOutline = actions.Find("Outline").GetComponent<RectTransform>();
		categoryOutline = categories.Find("Outline").GetComponent<RectTransform>();

		resourceIcons = new Dictionary<string, Sprite>();
		foreach (Sprite sprite in Resources.LoadAll<Sprite>("Sprite Sheets/ResourceIcons"))
			resourceIcons.Add(sprite.name, sprite);
	}

	void Start()
	{
		SetInfoVisibility(infoToggle.isOn);
		infoToggle.onValueChanged.AddListener(SetInfoVisibility);
		UpdateInfo();
		// Update category UI
		LoadCategory(BuildingCategory.Harvesters);
		SelectBuildAction(BuildAction.Pan);
	}

	private void SelectBuildAction(BuildAction buildAction)
	{
		buildActionOutline.anchoredPosition = new Vector2(((int)buildAction) * 45 + 20, 0);
		gameController.SelectBuildAction(buildAction);
		if (buildAction == BuildAction.Build)
		{
			// do nothing
		}
		else
		{
			selectedOption = null;
			UpdateInfo();
		}
	}

	public void SelectBuildAction(string action)
	{
		SelectBuildAction((BuildAction)Enum.Parse(typeof(BuildAction), action));
	}

	/// <summary>
	/// Updates Info and Resource Costs
	/// </summary>
	private void UpdateInfo()
	{
		string buildingName = null;
		if (hoveredOption != null)
			buildingName = hoveredOption;
		else if (selectedOption != null)
			buildingName = selectedOption;

		if (buildingName == null)
		{
			costMenu.gameObject.SetActive(false);
			infoText.text = "";
		}
		else
		{
			foreach (Transform child in costContainer)
				Destroy(child.gameObject);

			Entity building = gameController.entityPrefabs[buildingName];
			optionName.text = building.displayName;
			costMenu.gameObject.SetActive(true);
			List<ResourceCost> resourceCosts = new List<ResourceCost>();
			float totalWidth = 0f;
			foreach (KeyValuePair<string, int> pair in building.Cost)
			{
				ResourceCost resourceCost = Instantiate(resourceCostPrefab, costContainer);
				resourceCost.SetText(pair.Value);
				resourceCost.SetIcon(resourceIcons.GetValueOrDefault(pair.Key, null));
				totalWidth += resourceCost.GetMinimumWidth();
				resourceCosts.Add(resourceCost);
			}
			totalWidth += resourceCostMargin * (building.Cost.Count - 1);
			float x = -totalWidth / 2f;
			foreach (ResourceCost resourceCost in resourceCosts)
			{
				// position explanation
				/*
				64 total, width=64, margin=4, n=1
				left pivot: -32
				center pivot: 0
				left: -(total=64)/2=-32

				132 total, width=64, margin=4, n=2
				left pivot: -66, 2
				center pivot: -34, 34
				left: -(total=132)/2 = -66
				Ans+(width=64)+(margin=4) = 2

				200 total, width=64, margin=4, n=3
				left pivot: -100, -32, 36
				center pivot: -68, 0, 68
				left: -(total=200)/2 = -100 
				*/
				resourceCost.rectTransform.anchoredPosition = new Vector2(x, 0);
				x += resourceCost.GetMinimumWidth();
			}

			infoText.text = building.GetEntityInfo();
		}
	}

	private void SelectBuilding(Building building)
	{
		if (gameController.currentBuildAction != BuildAction.Build)
			return;

		selectedOption = building.entityName;
		UpdateInfo();
		gameController.SelectBuilding(building);
	}

	private void SelectHoverBuilding(string buildingName)
	{
		hoveredOption = buildingName;
		UpdateInfo();
	}

	private void StopHoverBuilding(string buildingName)
	{
		if (hoveredOption == buildingName)
		{
			hoveredOption = null;
			UpdateInfo();
		}
	}

	private void LoadCategory(BuildingCategory category)
	{
		// Destroy existing options
		foreach (Transform child in options)
			Destroy(child.gameObject);

		// Create new options
		int i = 0;
		foreach (string buildingName in gameController.categoryPrefabs[category])
		{
			Building building = (Building)gameController.entityPrefabs[buildingName];
			RectTransform option = Instantiate(buildingOptionPrefab, options).GetComponent<RectTransform>();
			option.anchoredPosition = new Vector2(i % 3 * 45, i / 3 * -45);
			BuildOption buildOption = option.GetComponent<BuildOption>();
			buildOption.onPointerEnter = () => SelectHoverBuilding(buildingName);
			buildOption.onPointerExit = () => StopHoverBuilding(buildingName);
			option.GetComponent<Button>().onClick.AddListener(() => SelectBuilding(building));
			option.GetComponent<Image>().sprite = building.GetComponent<SpriteRenderer>().sprite;
			i++;
		}

		categoryOutline.anchoredPosition = new Vector2(0, ((int)category - 1) * -45 - 20);
	}

	public void LoadCategory(string category)
	{
		LoadCategory((BuildingCategory)Enum.Parse(typeof(BuildingCategory), category));
	}

	public void SetInfoVisibility(bool enable)
	{
		infoFrame.gameObject.SetActive(enable);
	}
}

public enum BuildAction
{
	Build,
	Delete,
	Pan
}