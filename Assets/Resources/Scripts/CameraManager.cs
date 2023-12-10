using System;
using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private new Camera camera;
    private float cameraSpeed;
    private float cameraSize;
    private const float minCameraSize = 2f;
    private const float maxCameraSize = 15f;
    private const float cameraZoomFactor = 1.05f;
    private Vector3 mouseStart;

    public BoundsInt bounds;
    
    void Awake()
    {
        camera = GetComponent<Camera>();
        cameraSpeed = 20f;
        cameraSize = 5f;
        // used as a placeholder value for "null"
        mouseStart = Vector3.down;
    }
    
    private Vector3 GetMousePlanePosition(Vector3 mousePosition)
    {
        Ray ray = camera.ScreenPointToRay(mousePosition);
        return ray.origin - ray.direction;
    }
    
    void Update()
    {
        Vector3 cameraCenter = camera.transform.position;

        if (Input.GetMouseButton(0))
        {
            // On mouse down, if mouse was previously up
            if (Input.GetMouseButtonDown(0))
            {
                // set start position for camera pan
                mouseStart = GetMousePlanePosition(Input.mousePosition);
            }
            // If camera pan active (but not first frame)
            else if (mouseStart != Vector3.down)
            {
                // TODO check very slight camera shake. camera shake does not occur if camera is orthographic
                Vector3 mouseEnd = GetMousePlanePosition(Input.mousePosition);
                cameraCenter -= (mouseEnd - mouseStart);
                // Realign camera start to current camera bc (mouseEnd - mouseStart) will now be 0
            }
        }
        else
        {
            // Stop camera pan
            mouseStart = Vector3.down;
            // Camera keyboard movement
            Vector3 direction = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
            if (direction.sqrMagnitude > 0f)
            {
                if (direction.sqrMagnitude > 1f)
                    direction.Normalize();
                cameraCenter += Quaternion.AngleAxis(camera.transform.eulerAngles.y, Vector3.up) * direction * (cameraSpeed * Time.deltaTime);
            }
        }
        
        // zoom, shift to keep cursor at same world point
        Vector3 worldPoint1 = camera.ScreenToWorldPoint(Input.mousePosition);
        cameraSize = Mathf.Clamp(cameraSize * Mathf.Pow(cameraZoomFactor, -Input.mouseScrollDelta.y), minCameraSize, maxCameraSize);
        camera.orthographicSize = cameraSize;
        
        Vector3 worldPoint2 = camera.ScreenToWorldPoint(Input.mousePosition);
        cameraCenter += worldPoint1 - worldPoint2;

        cameraCenter.x = Mathf.Clamp(cameraCenter.x, bounds.xMin + .5f, bounds.xMax - .5f);
        cameraCenter.y = Mathf.Clamp(cameraCenter.y, bounds.yMin + .5f, bounds.yMax - .5f);
        camera.transform.position = cameraCenter;
    }
}
