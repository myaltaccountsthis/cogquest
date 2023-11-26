using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField]
    private float MAX_HEALTH { get; }
    [SerializeField]
    private Dictionary<string, int> cost { get; }
    public float health { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        health = MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
