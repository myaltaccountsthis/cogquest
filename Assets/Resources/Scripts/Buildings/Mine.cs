using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{
    // The rate of resource gain per resource tile per second
    [SerializeField]
    private float BASE_SPEED { get; }
    private float mineSpeed { get; set;}

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
