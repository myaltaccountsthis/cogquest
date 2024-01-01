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

	protected override void Awake() {
        base.Awake();
        occupied = TIER == 0;
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
