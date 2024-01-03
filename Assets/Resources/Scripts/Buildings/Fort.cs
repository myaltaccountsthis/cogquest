using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;

public class Fort : Building
{
	[SerializeField]
    private int TIER;
    [SerializeField]
    private Sprite[] sprites;
    public bool occupied;
    private readonly Color UNOCCUPIED_COLOR = Color.red;
    private readonly Color OCCUPIED_COLOR = Color.white;

    public int Tier
    {
        get => TIER;
    }

    public override void Awake() {
        base.Awake();
        occupied = TIER == 0;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (occupied)
        {
            spriteRenderer.color = OCCUPIED_COLOR;
        }
        else
        {
            spriteRenderer.color = UNOCCUPIED_COLOR;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

    }

    public void OnDestroyed()
    {
        occupied = !occupied;
        if (!occupied)
        {
            health = MAX_HEALTH;
        }
    }

}
