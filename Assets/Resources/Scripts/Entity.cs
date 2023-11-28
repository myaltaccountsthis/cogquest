using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField]
    protected float MAX_HEALTH;
    protected float health;
    [SerializeField]
    protected Dictionary<string, int> cost;

    public float Health {
        get => health;
    }
    
    public Dictionary<string, int> Cost {
        get => cost;
    }

    public Sprite sprite {
        get => GetComponent<SpriteRenderer>().sprite;
    }

    void Awake() {
        health = MAX_HEALTH;
    }

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
