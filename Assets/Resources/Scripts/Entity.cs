using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public abstract class Entity : MonoBehaviour
{
    public string entityName;
    public string displayName;

    protected GameController gameController;
    public HealthBar healthBar { get; private set; }

	[SerializeField]
    protected float MAX_HEALTH;
    public float maxHealth => MAX_HEALTH;
    public float health { get; protected set; }
    [SerializeField]
    protected DataDictionary<string, int> cost;
    public bool deletable;
    public int team;

    public bool active;

    protected SpriteRenderer spriteRenderer;
    private new Collider2D collider;
    public bool Occupied => team == 0;
    public static readonly Color UNOCCUPIED_COLOR = new Color(1f, .5f, .5f);
    public static readonly Color OCCUPIED_COLOR = Color.white;

    public float HealthFraction => health / MAX_HEALTH;

    public Dictionary<string, int> Cost {
        get => cost.ToDictionary();
    }

    /// <summary>
    /// In order from bottom to top
    /// </summary>
    public Sprite[] previewSprites
    {
        get
        {
            return allSpriteRenderers.Select(sr => sr.sprite).ToArray();
        }
    }

    public SpriteRenderer[] allSpriteRenderers
    {
        get
        {
			List<SpriteRenderer> list = new() { GetComponent<SpriteRenderer>() };
            list.AddRange(otherSpriteRenderers);
            return list.ToArray();
		}
    }
    public SpriteRenderer[] otherSpriteRenderers;

    protected virtual float rotation
    {
        get => transform.eulerAngles.z;
        set => transform.eulerAngles = new Vector3(0, 0, value);
	}

	protected virtual void Awake() {
        health = MAX_HEALTH;
        active = false;
		spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        gameController = GameObject.Find("Canvas").GetComponent<GameController>();
        healthBar = gameController.InstantiateHealthBar();
        healthBar.SetActive(false);

		Debug.Assert(GetComponent<SpriteRenderer>().sortingLayerName != "Default", "Entity sorting layer should not be default");
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

	// Update is called once per frame
	protected virtual void Update()
    {
        if (GameController.isPaused)
            return;
        
        if (!active)
            return;

        healthBar.SetPosition(collider);
	}

	public virtual void DoMouseDown()
	{

	}

    public virtual void DoMouseCancel()
    {

    }

    public void TakeDamage(float damage)
    {
        if (!active)
            return;

        health -= damage;
        OnDamaged();

        if (this is Building building)
        {
            building.PlayDamagedAudio();
        }
        
        if (health <= 0f)
        {
            OnDestroyed();
        }
	}

    public virtual void OnDamaged()
    {
		healthBar.SetPercentage(HealthFraction);
        healthBar.ResetFade();
        healthBar.SetActive(true);
        if (this is Fort && !Occupied)
        {
            gameController.OnEnemyInvaded();
        }
    }

    public virtual void OnDestroyed()
    {
		Destroy(gameObject);
	}

	void OnDestroy()
	{
        if (healthBar != null)
		    Destroy(healthBar.gameObject);
	}

	public Vector3Int GetIntPosition()
    {
        return new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
    }

    public virtual void LoadEntitySaveData(Dictionary<string, string> saveData)
    {
        // update position, orientation, etc.
        if (saveData.TryGetValue("rotation", out string rotationStr))
            rotation = float.Parse(rotationStr);
        if (saveData.TryGetValue("posX", out string posXStr) && saveData.TryGetValue("posY", out string posYStr))
            transform.position = new Vector3(float.Parse(posXStr), float.Parse(posYStr));
        if (saveData.TryGetValue("health", out string healthStr))
            health = float.Parse(healthStr);
        if (saveData.TryGetValue("team", out string teamStr))
            team = int.Parse(teamStr);

        UpdateSpriteColor();
		healthBar.SetPercentage(health);
	}

    public virtual void UpdateSpriteColor()
	{
		SetSpriteColor(Occupied ? OCCUPIED_COLOR : UNOCCUPIED_COLOR);
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
            { "team", team.ToString() },
            { "class", entityName }
        };
    }

    protected virtual List<string> GetEntityInfoList()
    {
        return new List<string> { "Name: " + displayName, string.Format("Health: {0}/{1}", active ? health : MAX_HEALTH, MAX_HEALTH) };
    }

    public string GetEntityInfo()
    {
        return string.Join("\n\n", GetEntityInfoList());
	}

	/// <summary>
	/// Change all sprite renderers in this building to a certain color (used for turrets)
	/// </summary>
	public void SetSpriteColor(Color color)
	{
		spriteRenderer.color = color;
        foreach (SpriteRenderer sr in otherSpriteRenderers)
        {
            sr.color = color;
        }
    }
}
