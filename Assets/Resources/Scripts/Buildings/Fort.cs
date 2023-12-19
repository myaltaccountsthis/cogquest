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

    public override void Awake() {
        base.Awake();
        occupied = TIER == 0;
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

    public void OnDestroyed()
    {
        occupied = !occupied;
        if (!occupied)
        {
            health = MAX_HEALTH;
        }
    }

}
