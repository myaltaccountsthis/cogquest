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
}
