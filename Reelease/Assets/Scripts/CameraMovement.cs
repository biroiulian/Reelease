using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class CameraMovement : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID
    public Camera Camera;
    public bool Rotate;
    protected Plane Plane;

    public float DecreaseCameraPanSpeed = 1; //Default speed is 1
    public float CameraUpperHeightBound; //Zoom out
    public float CameraLowerHeightBound; //Zoom in
    public float DistanceFromFloor;
    public Vector2 xLimits;
    public Vector2 zLimits;
    public Vector3 WorldCreationPosition;
    public Vector3 WorldCreationRotation;
    public Vector3 GameplayPosition;
    public Vector3 GameplayRotation;
    public float rotationSpeedWc = 3f;

    private CameraMode cameraMode = CameraMode.Gameplay;

    private Vector3 cameraStartPosition;

    private Vector3 terrainPos = new Vector3(20, 0, -20);

    private void Awake()
    {
        if (Camera == null)
            Camera = Camera.main;

        cameraStartPosition = Camera.transform.position;
    }

    private void Update()
    {
        if (cameraMode == CameraMode.Gameplay)
        {
            //Update Plane
            if (Input.touchCount >= 1)
                Plane.SetNormalAndPosition(transform.up, transform.position);


            // Shoot a ray from the camera towards floor, and keep distance from the floor precisely between some bounds.
            Ray ray = new Ray(Camera.transform.position, Vector3.down);
            float floorHeight = 0;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 20))
            {
                floorHeight = hitInfo.point.y;
            }

            //
            var Delta1 = Vector3.zero;
            var Delta2 = Vector3.zero;

            // save initial position
            Vector3 camPositionBeforeAdjustment = Camera.transform.position;

            // Scroll (Pan function)
            if (Input.touchCount >= 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                //Get distance camera should travel
                Delta1 = PlanePositionDelta(Input.GetTouch(0)) / DecreaseCameraPanSpeed;
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    Camera.transform.Translate(Delta1, Space.World);
            }

            if (Camera.transform.position.y < floorHeight + DistanceFromFloor)
            {
                Camera.transform.position = new Vector3(Camera.transform.position.x, floorHeight + DistanceFromFloor, Camera.transform.position.z);
            }
            if (Camera.transform.position.x < xLimits.x)
            {
                Camera.transform.position = new Vector3(xLimits.x, Camera.transform.position.y, Camera.transform.position.z);
            }
            if (Camera.transform.position.x > xLimits.y)
            {
                Camera.transform.position = new Vector3(xLimits.y, Camera.transform.position.y, Camera.transform.position.z);
            }
            if (Camera.transform.position.z < zLimits.x)
            {
                Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, zLimits.x);
            }
            if (Camera.transform.position.z > zLimits.y)
            {
                Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, zLimits.y);
            }


            //Pinch (Zoom Function)
            if (Input.touchCount >= 2)
            {
                var pos1 = PlanePosition(Input.GetTouch(0).position);
                var pos2 = PlanePosition(Input.GetTouch(1).position);
                var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                var pos2b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

                //calc zoom
                var zoom = Vector3.Distance(pos1, pos2) /
                           Vector3.Distance(pos1b, pos2b);

                //edge case
                if (zoom == 0 || zoom > 10)
                    return;

                //Move cam amount the mid ray
                camPositionBeforeAdjustment = Camera.transform.position;
                var newPos = Vector3.LerpUnclamped(pos1, Camera.transform.position, 1 / zoom);

                if (newPos.y < floorHeight + DistanceFromFloor)
                {
                    newPos.y = floorHeight + DistanceFromFloor;
                }

                Camera.transform.position = newPos;

                //Restricts zoom height 

                //Upper (ZoomOut)
                if (Camera.transform.position.y > (cameraStartPosition.y + CameraUpperHeightBound))
                {
                    Camera.transform.position = camPositionBeforeAdjustment;
                }
                //Lower (Zoom in)
                if (Camera.transform.position.y < (cameraStartPosition.y - CameraLowerHeightBound) || Camera.transform.position.y <= 1)
                {
                    Camera.transform.position = camPositionBeforeAdjustment;
                }

                //Rotation Function
                if (Rotate && pos2b != pos2)
                    Camera.transform.RotateAround(pos1, Plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, Plane.normal));
            }
        }
        else if (cameraMode == CameraMode.WorldCreation)
        {
            Camera.transform.RotateAround(terrainPos, Vector3.up, rotationSpeedWc * Time.deltaTime);

        }
    }

    //Returns the point between first and final finger position
    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;


        //delta
        var rayBefore = Camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = Camera.ScreenPointToRay(touch.position);
        if (Plane.Raycast(rayBefore, out var enterBefore) && Plane.Raycast(rayNow, out var enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = Camera.ScreenPointToRay(screenPos);
        if (Plane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }

    public void SetWorldCreationPerspective()
    {
        Debug.Log("Set creation perspective.");
        cameraMode = CameraMode.WorldCreation;
        Camera.transform.position = WorldCreationPosition;
        Camera.main.transform.rotation = Quaternion.Euler(WorldCreationRotation);
        Debug.Log("Method: Set camera rotation to: " + Camera.transform.rotation);
    }

    public void SetGameplayPerspective()
    {
        Debug.Log("Set gameplay perspective.");
        cameraMode = CameraMode.Gameplay;
        Camera.transform.position = GameplayPosition;
        Camera.main.transform.rotation = Quaternion.Euler(GameplayRotation);
        Debug.Log("Method: Set camera rotation to: " + Camera.transform.rotation);
    }
#endif

    private enum CameraMode{
        Gameplay, WorldCreation
    }
}