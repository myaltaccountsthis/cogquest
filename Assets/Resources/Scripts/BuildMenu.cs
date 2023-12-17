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
	private Transform entityFolder;
	private GameController gameController;

	private RectTransform buildingOptionPrefab;
	private RectTransform buildActionOutline;

	void Awake()
	{
		categoryName = transform.Find("CategoryName").GetComponent<TextMeshProUGUI>();
		categories = transform.Find("Categories");
		options = transform.Find("Options");
		actions = transform.Find("Actions");
		entityFolder = GameObject.Find("Entities").transform;
		gameController = GameObject.Find("Canvas").GetComponent<GameController>();

		buildingOptionPrefab = Resources.Load<RectTransform>("Prefabs/BuildingOption");
		buildActionOutline = actions.Find("Outline").GetComponent<RectTransform>();
	}

	void Start()
	{
		// Update category UI
		LoadCategory(BuildingCategory.Harvesters);
		SelectBuildAction(BuildAction.Pan);
	}

	private void SelectBuildAction(BuildAction buildAction)
	{
		buildActionOutline.anchoredPosition = new Vector2(((int)buildAction) * 45 + 20, 0);
		gameController.SelectBuildAction(buildAction);
	}

	public void SelectBuildAction(string action)
	{
		SelectBuildAction((BuildAction)Enum.Parse(typeof(BuildAction), action));
	}

	private void LoadCategory(BuildingCategory category)
	{
		// Destroy existing options
		foreach (Transform child in options)
			Destroy(child.gameObject);

		// Create new options
		int i = 0;
		foreach (Building building in gameController.categoryPrefabs[category])
		{
			RectTransform option = Instantiate(buildingOptionPrefab, options).GetComponent<RectTransform>();
			option.anchoredPosition = new Vector2(i % 3 * 45, i / 3 * -45);
			option.GetComponent<Button>().onClick.AddListener(() => gameController.SelectBuilding(building));
			option.GetComponent<Image>().sprite = building.GetComponent<SpriteRenderer>().sprite;
			i++;
		}
	}

	public void LoadCategory(string category)
	{
		LoadCategory((BuildingCategory)Enum.Parse(typeof(BuildingCategory), category));
	}
}

public enum BuildAction
{
	Build,
	Delete,
	Pan
}