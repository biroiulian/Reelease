using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 touchStart;
    public Camera cam;
    public float groundY = 0;
    public float LowerCameraLimitX;
    public float UpperCameraLimitX;
    public float LowerCameraLimitZ;
    public float UpperCameraLimitZ;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = GetWorldPosition(groundY);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 direction = touchStart - GetWorldPosition(groundY);
            cam.transform.position += direction;
        }

        if (cam.transform.position.x < LowerCameraLimitX)
        {
            cam.transform.position.Set(LowerCameraLimitX, cam.transform.position.y, cam.transform.position.z);
        }
        if (cam.transform.position.x > UpperCameraLimitX)
        {
            cam.transform.position.Set(UpperCameraLimitX, cam.transform.position.y, cam.transform.position.z);
        }
        if (cam.transform.position.z < LowerCameraLimitZ)
        {
            cam.transform.position.Set(cam.transform.position.x, cam.transform.position.y, LowerCameraLimitZ);
        }
        if (cam.transform.position.z > UpperCameraLimitZ)
        {
            cam.transform.position.Set(cam.transform.position.x, cam.transform.position.y, UpperCameraLimitZ);
        }
    }
    private Vector3 GetWorldPosition(float z)
    {
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.down, new Vector3(z, 0, 0));
        float distance;
        ground.Raycast(mousePos, out distance);
        return mousePos.GetPoint(distance);
    }
}