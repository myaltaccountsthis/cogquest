using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public virtual string ENTITY_NAME => GetType().Name;

    [SerializeField]
    protected float MAX_HEALTH;
    protected float health;
    [SerializeField]
    protected Dictionary<string, int> cost;

    public float Health {
        get => health;
    }

    public float HealthFraction => Health / MAX_HEALTH;
    
    public Dictionary<string, int> Cost {
        get => cost;
    }

    public Sprite sprite {
        get => GetComponent<SpriteRenderer>().sprite;
    }

    public virtual void Awake() {
        health = MAX_HEALTH;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
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
            transform.eulerAngles = new Vector3(0, 0, float.Parse(rotationStr));
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
            { "rotation", transform.rotation.eulerAngles.z.ToString() },
            { "health", health.ToString() },
            { "class", ENTITY_NAME }
        };
    }
}
