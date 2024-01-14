using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnMenu : MonoBehaviour
{
	private new RectTransform transform;
	private GameController gameController;
	private Transform container;
	private RectTransform optionPrefab;

	void Awake()
	{
		transform = GetComponent<RectTransform>();
		gameController = GameObject.Find("Canvas").GetComponent<GameController>();
		container = transform.Find("Container");
		optionPrefab = Resources.Load<RectTransform>("Prefabs/UnitOption");
	}

	void Start()
	{
		gameObject.SetActive(false);
	}

	public void OpenMenu(Fort fort, IEnumerable<Unit> units)
	{
		foreach (Transform child in container)
			Destroy(child.gameObject);

		// Create new options
		int i = 0;
		foreach (Unit unit in units)
		{
			RectTransform option = Instantiate(optionPrefab, container);
			option.anchoredPosition = new Vector2(i % 3 * 45, i / 3 * -45);
			UIOption buildOption = option.GetComponent<UIOption>();
			buildOption.onPointerEnter = () => SelectHoverUnit(unit);
			buildOption.onPointerExit = () => StopHoverUnit();
			buildOption.SetSprites(unit.previewSprites);
			option.GetComponent<Button>().onClick.AddListener(() => gameController.SpawnPlayerUnit(fort, unit));
			i++;
		}

		transform.anchoredPosition = fort.transform.position + Vector3.up * fort.collider.size.y / 2f;
		gameObject.SetActive(true);
	}

	public void CloseMenu()
	{
		gameObject.SetActive(false);
	}

	public void SelectHoverUnit(Unit unit)
	{
		gameController.SetHoveredResourceCost(unit);
	}

	public void StopHoverUnit()
	{
		gameController.SetHoveredResourceCost(null);
	}
}
