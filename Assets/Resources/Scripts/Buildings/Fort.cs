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

    void Awake() {
        occupied = TIER == 0;
        coalProduction = occupied ? GetCoalProduction() : 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
