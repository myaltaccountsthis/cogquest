using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyIn : MonoBehaviour
{
    public Vector3 initialPosition;
    public Vector3 velocity;
    public float smoothTime;
    public float delay = 0;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
        transform.position = transform.TransformPoint(initialPosition);
    }
    void Update()
    {
        if (delay <= 0)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            delay -= Time.deltaTime;
        }
    }
}
