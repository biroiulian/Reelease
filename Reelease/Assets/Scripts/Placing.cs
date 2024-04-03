using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Placing : MonoBehaviour
{
    public bool isTryingToPlace = false;
    public Camera Camera;
    public GameObject PlaceablesContainer;

    public Material ValidPositionMaterial;
    public Material InvalidPositionMaterial;
    private GameObject PlaceableInstance;
    private Vector3 lastPosition;

    private Material[] originalMaterials;
    private Material[] invalidMaterials;
    private Material[] validMaterials;

    private bool isAnimal = false;
    private bool canPlace = false;

    public void CancelPlacingCommand()
    {
        Destroy(PlaceableInstance);
        isTryingToPlace = false;
    }

    public void ApplyPlacingCommand()
    {
        // Applied
        Debug.Log("Start apply placing.");
        if (isAnimal)
        {
            Debug.Log("Is animal.");
            if (canPlace)
            {
                Debug.Log("Can place.");
                PlaceableInstance.GetComponent<Animator>().enabled = true;
                if (PlaceableInstance.GetComponent<AnimalMovement>() is not null)
                {
                    PlaceableInstance.GetComponent<AnimalMovement>().enabled = true;
                }
                else
                {
                    PlaceableInstance.GetComponent<SimpleAnimalMovement>().enabled = true;
                }

                isTryingToPlace = false;
                PlaceableInstance.GetComponentInChildren<SkinnedMeshRenderer>().materials = originalMaterials;
                PlaceableInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                Debug.Log("Applied placing.");
            }
            else
            {
                Destroy(PlaceableInstance);
                Debug.Log("Couldn't apply placing.");
            }
        }
        else
        {
            if (canPlace)
            {
                PlaceableInstance.GetComponentInChildren<MeshRenderer>().materials = originalMaterials;
            }
            else
            {
                Destroy(PlaceableInstance);
            }

            isTryingToPlace = false;
        }
        return;
    }

    public void StartPlacingAnimal(ItemResource p)
    {
        isAnimal = true;
        isTryingToPlace = true;

        PlaceableInstance = Instantiate(p.placeablePrefab);
        // This will be used when saving, to have an easy way of knowing what this instance's type is.
        PlaceableInstance.name = p.itemType.ToString();
        PlaceableInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        PlaceableInstance.transform.LookAt(Camera.transform);
        PlaceableInstance.transform.rotation = new Quaternion(0, PlaceableInstance.transform.rotation.y, 0, PlaceableInstance.transform.rotation.w);
        PlaceableInstance.GetComponent<Animator>().enabled = false;

        // Disable movement script (check which type)
        if (PlaceableInstance.GetComponent<AnimalMovement>() is not null)
        {
            PlaceableInstance.GetComponent<AnimalMovement>().enabled = false;
        }
        else
        {
            PlaceableInstance.GetComponent<SimpleAnimalMovement>().enabled = false;
        }

        // Put the placeable object under the desired parent
        PlaceableInstance.transform.parent = PlaceablesContainer.transform;

        // Switch original materials with preview ones, and save the old ones
        originalMaterials = PlaceableInstance.GetComponentInChildren<SkinnedMeshRenderer>().materials;
        invalidMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            invalidMaterials[i] = InvalidPositionMaterial;
        }
        validMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            validMaterials[i] = ValidPositionMaterial;
        }
        //

        // See if the initial position is valid or invalid
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo, 100.0f))
        {
            PlaceableInstance.transform.position = hitInfo.point;
            if (Mathf.Abs(hitInfo.normal.x) < 0.2f)
            {
                PlaceableInstance.GetComponentInChildren<SkinnedMeshRenderer>().materials = validMaterials;
            }
            else
            {
                PlaceableInstance.GetComponentInChildren<SkinnedMeshRenderer>().materials = invalidMaterials;
            }
        }
        //
    }

    public void StartPlacingEnviroment(ItemResource p)
    {
        isAnimal = false;
        isTryingToPlace = true;

        PlaceableInstance = Instantiate(p.placeablePrefab);

        // This will be used when saving, to have an easy way of knowing what this instance's type is.
        PlaceableInstance.tag = p.itemType.ToString();

        // Put the placeable object under the desired parent
        PlaceableInstance.transform.parent = PlaceablesContainer.transform;

        // Switch original materials with preview ones, and save the old ones
        originalMaterials = PlaceableInstance.GetComponentInChildren<MeshRenderer>().materials;
        invalidMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            invalidMaterials[i] = InvalidPositionMaterial;
        }
        validMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            validMaterials[i] = ValidPositionMaterial;
        }
        //

        // See if the initial position is valid or invalid
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo, 100.0f))
        {
            PlaceableInstance.transform.position = hitInfo.point;
            if (Mathf.Abs(hitInfo.normal.x) < 0.5f)
            {
                PlaceableInstance.GetComponentInChildren<MeshRenderer>().materials = validMaterials;
            }
            else
            {
                PlaceableInstance.GetComponentInChildren<MeshRenderer>().materials = invalidMaterials;
            }
        }
        //
    }

    // Update is called once per frame
    void Update()
    {
        if (isTryingToPlace)
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Ray ray = Camera.ScreenPointToRay(Input.GetTouch(0).position);
                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    if (Physics.Raycast(ray, out RaycastHit hitInfo))
                    {
                        Debug.Log("normal x: " + hitInfo.normal.x);
                        if (isAnimal)
                        {
                            PlaceableInstance.transform.LookAt(Camera.transform);
                            PlaceableInstance.transform.rotation = new Quaternion(0, PlaceableInstance.transform.rotation.y, 0, PlaceableInstance.transform.rotation.w);
                        }
                        if (-0.15f < hitInfo.normal.x && hitInfo.normal.x < 0.15f)
                        {
                            lastPosition = hitInfo.point;
                            PlaceableInstance.transform.position = hitInfo.point;
                            if (isAnimal) PlaceableInstance.GetComponentInChildren<SkinnedMeshRenderer>().materials = validMaterials;
                            else PlaceableInstance.GetComponentInChildren<MeshRenderer>().materials = validMaterials;
                            canPlace = true;
                        }
                        else
                        {
                            lastPosition = hitInfo.point;
                            PlaceableInstance.transform.position = hitInfo.point;
                            if (isAnimal) PlaceableInstance.GetComponentInChildren<SkinnedMeshRenderer>().materials = invalidMaterials;
                            else PlaceableInstance.GetComponentInChildren<MeshRenderer>().materials = invalidMaterials;
                            canPlace = false;
                        }
                    }
                }
            }
        }
    }

    public GameObject GetLastPlaced()
    {
        return PlaceableInstance;
    }
}
