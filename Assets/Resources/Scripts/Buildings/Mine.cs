using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{
    // The rate of resource gain per resource tile per second
    [SerializeField]
    private float baseSpeed;
    private float mineSpeed;

    public float MineSpeed {
        get => mineSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Calculate real mineSpeed based on resource tiles
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
