using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : Entity
{

    [SerializeField]
    protected float AGGRO_RADIUS;
    [SerializeField]
    protected float SPEED;
}
