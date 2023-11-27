using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fort : Building
{
    private int TIER { get; }
    public bool occupied { get; private set; }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetCoalProduction()
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
