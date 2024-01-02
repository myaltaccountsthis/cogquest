using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public string entityName;
    public string displayName;

    [SerializeField]
    protected float MAX_HEALTH;
    public float health { get; protected set; }
    [SerializeField]
    protected DataDictionary<string, int> cost;
    public bool deletable;

    public float HealthFraction => health / MAX_HEALTH;

    public Dictionary<string, int> Cost {
        get => cost.ToDictionary();
    }

    public Sprite sprite {
        get => GetComponent<SpriteRenderer>().sprite;
    }

    protected virtual float rotation
    {
        get => transform.eulerAngles.z;
        set => transform.eulerAngles = new Vector3(0, 0, value);
	}

	protected virtual void Awake() {
        health = MAX_HEALTH;

        Debug.Assert(GetComponent<SpriteRenderer>().sortingLayerName != "Default");
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

	// Update is called once per frame
	protected virtual void Update()
    {

    }

    public Vector3Int GetIntPosition()
    {
        return new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
    }

    public void LoadEntitySaveData(Dictionary<string, string> saveData)
    {
        // TODO update position, orientation, etc.
        if (saveData.TryGetValue("rotation", out string rotationStr))
            rotation = float.Parse(rotationStr);
        if (saveData.TryGetValue("posX", out string posXStr) && saveData.TryGetValue("posY", out string posYStr))
            transform.position = new Vector3(float.Parse(posXStr), float.Parse(posYStr));
        if (saveData.TryGetValue("health", out string healthStr))
            health = float.Parse(healthStr);
    }

    /// <summary>
    /// Returns data for this entity to be saved
    /// </summary>
    public virtual Dictionary<string, string> GetEntitySaveData()
    {
        return new Dictionary<string, string>()
        {
            { "posX", transform.position.x.ToString() },
            { "posY", transform.position.y.ToString() },
            { "rotation", rotation.ToString() },
            { "health", health.ToString() },
            { "class", entityName }
        };
    }

    protected virtual List<string> GetEntityInfoList()
    {
        return new List<string> { "Name: " + displayName };
    }

    public string GetEntityInfo()
    {
        return string.Join("\n\n", GetEntityInfoList());
    }
}
