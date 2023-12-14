using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Fort : Building
{
	[SerializeField]
    private int TIER;
    [SerializeField]
    private Sprite[] sprites;
    private bool occupied;
    private float coalProduction;

    public override void Awake() {
        base.Awake();
        occupied = TIER == 0;
        coalProduction = occupied ? GetCoalProduction() : 0;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

    }

    public float GetCoalProduction()
    {
        return occupied ? Mathf.Max(TIER, 1) * 10 : 0;
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
