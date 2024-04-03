using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum ObjectType { Tree, Grass };

[Serializable]
public struct PlaceableObject
{
    //public ObjectType ObjectType;
    public ItemType[] ModelsVariation;
    public int NoInstances;
    //public float Radius;
    public bool ignoreRandom;
    public Interval HeightInterval;
    public Interval ScaleVariation;
    public float TerrainHeightDiff;

    public NoiseArgs NoiseArgs;

    public List<Vector3> PlacedPositions;

    private float[,] SpreadMap;

    public void SetSpreadMap(float[,] spreadMap) { SpreadMap = spreadMap; }
    public float[,] GetSpreadMap() { return SpreadMap; }
}

public class EnviromentController : MonoBehaviour
{
    [SerializeField]
    public PlaceableObject[] PlaceableObjects;

    public bool autoUpdate = true;
    public bool resetPlacement = false;

    public ResourceDictionary ResourceDictionary;
    public GameObject EnviromentContainer;

    private List<EnviromentItem> placedDecorItems = new List<EnviromentItem>();

    public void DrawEnviroment()
    {
        if (resetPlacement) GenerateEnviroment();
        else LoadEnviroment();
    }

    public void GenerateEnviroment()
    {
        Initialize();
        for (int i = 0; i < PlaceableObjects.Length; i++)
        {
            GenerateObjPositions(ref PlaceableObjects[i]);
        }
        DrawRandomObjects();
    }

    private void Initialize()
    {
        DeleteEnviroment();
        int seedOffset = 1;
        for (int i = 0; i < PlaceableObjects.Length; i++)
        {
            PlaceableObjects[i].SetSpreadMap(Noise.GenerateBinaryNoiseMap(PlaceableObjects[i].NoiseArgs, seedOffset));
            seedOffset++;
        }
    }

    public void DeleteEnviroment()
    {
        for (int i = EnviromentContainer.transform.childCount; i > 0; --i)
            DestroyImmediate(EnviromentContainer.transform.GetChild(0).gameObject);
        placedDecorItems.Clear();
    }

    public void LoadEnviroment()
    {
        DrawRandomObjects();
    }

    public void LoadEnviroment(EnviromentData envData)
    {
        Debug.Log("Drawing saved enviroment.");
        placedDecorItems = envData.items.ToList();
        DrawSavedObjects();
    }

    private void DrawSavedObjects()
    {
        foreach (EnviromentItem env in placedDecorItems)
        {
            var instance = Instantiate(ResourceDictionary.GetItemResource(env.itemType).placeablePrefab, EnviromentContainer.transform);
            instance.transform.position = new Vector3(env.position.x, env.position.y, env.position.z);
            // Variation tweaks
            // assign a random rotation on the Y axis
            instance.transform.rotation.eulerAngles.Set(env.rotation.x, env.rotation.y, env.rotation.z);
            // assign a random height scale
            instance.transform.localScale = new Vector3(env.scale.x, env.scale.y, env.scale.z);
            instance.transform.parent = EnviromentContainer.transform;
        }
    }

    private void DrawRandomObjects()
    {
        for (int i = 0; i < PlaceableObjects.Length; i++)
        {
            Debug.Log("Drawing " + PlaceableObjects[i].PlacedPositions.Count + " objects...");

            for (int j = 0; j < PlaceableObjects[i].PlacedPositions.Count; j++)
            {
                var itemTypeToInstantiate = PlaceableObjects[i].ModelsVariation[UnityEngine.Random.Range(0, PlaceableObjects[i].ModelsVariation.Length)];
                var instance = Instantiate(ResourceDictionary.GetItemResource(itemTypeToInstantiate).placeablePrefab, EnviromentContainer.transform);
                instance.transform.position = new Vector3(PlaceableObjects[i].PlacedPositions[j].x, PlaceableObjects[i].PlacedPositions[j].y, PlaceableObjects[i].PlacedPositions[j].z);
                // Variation tweaks
                // assign a random rotation on the Y axis
                instance.transform.Rotate(new Vector3(0f, 0f, UnityEngine.Random.Range(0, 180) * 1f));
                // assign a random height scale
                var randScaleFactor = UnityEngine.Random.Range(PlaceableObjects[i].ScaleVariation.lowerLimit, PlaceableObjects[i].ScaleVariation.upperLimit);
                instance.transform.localScale = new Vector3(
                    randScaleFactor * instance.transform.localScale.x,
                    randScaleFactor * instance.transform.localScale.y,
                    randScaleFactor * instance.transform.localScale.z);

                // Save locally
                placedDecorItems.Add(new EnviromentItem() 
                { 
                    itemType = itemTypeToInstantiate, 
                    position = new Coords3D() { x = instance.transform.position.x, y = instance.transform.position.y, z = instance.transform.position.z }, 
                    rotation = new Coords3D() { x = instance.transform.rotation.eulerAngles.x, y = instance.transform.rotation.eulerAngles.y, z = instance.transform.rotation.eulerAngles.z },
                    scale = new Coords3D() { x = instance.transform.localScale.x, y = instance.transform.localScale.y, z = instance.transform.localScale.z } 
                });
            }
        }
    }

    private void GenerateObjPositions(ref PlaceableObject obj)
    {
        var validArea = GetValidObjPositions(obj);

        if (validArea.Count < obj.NoInstances) Debug.LogWarning("Don't have enough place to position grass.");

        obj.PlacedPositions = new List<Vector3>();
        for (int i = 0; i < Mathf.Min(validArea.Count, obj.NoInstances); i++)
        {
            var chosenPointIndex = UnityEngine.Random.Range(0, validArea.Count);
            var chosenPoint = validArea[chosenPointIndex];
            validArea.RemoveAt(chosenPointIndex);
            obj.PlacedPositions.Add(chosenPoint);
        }

    }

    /// <summary>
    /// Possible object positions from which we can choose.
    /// </summary>
    private List<Vector3> GetValidObjPositions(PlaceableObject obj)
    {
        var verts = MeshGenerator.terrainMeshData.vertices;
        var vertsSize = Mathf.Sqrt(verts.Length);

        var validPos = new List<Vector3>();

        var index = 0;
        for (int i = 0; i < vertsSize; i++)
        {
            for (int j = 0; j < vertsSize; j++)
            {
                var vert = verts[index];
                if (!obj.ignoreRandom)
                {
                    // if it's in the generated random area
                    if (obj.GetSpreadMap()[i, j] == 1)
                    {
                        // if it's in the specified height interval
                        if (vert.y <= obj.HeightInterval.upperLimit && vert.y >= obj.HeightInterval.lowerLimit)
                        {
                            // if it's between the accepted steepness
                            if (HasAcceptedHeightDifference(vert, index, verts, obj.TerrainHeightDiff))
                            {
                                var shiftedPosition = ShiftPosition(vert, index, verts);
                                validPos.Add(shiftedPosition);
                            }
                        }
                    }
                }
                else  // bypass the random perlin distribution
                {
                    if (vert.y <= obj.HeightInterval.upperLimit && vert.y >= obj.HeightInterval.lowerLimit)
                    {
                        // if it's between the accepted steepness
                        if (HasAcceptedHeightDifference(vert, index, verts, obj.TerrainHeightDiff))
                        {
                            var shiftedPosition = ShiftPosition(vert, index, verts);
                            validPos.Add(shiftedPosition);
                        }
                    }
                }
                index++;
            }
        }

        return validPos;
    }

    /// <summary>
    /// We're shifting the position so the object placed to not be exactly on the vertex.
    /// We're placing it between the vertex and one of its neighbours.
    /// </summary>
    private Vector3 ShiftPosition(Vector3 v, int index, Vector3[] verts)
    {
        var size = (int)MathF.Sqrt(verts.Count());

        int xIndex = index / size;
        int yIndex = index % size;

        int direction = UnityEngine.Random.Range(0, 9);

        if (verts[size * xIndex + yIndex] != v) Debug.LogWarning("Vertex doesn't check up.");

        for (int x = Mathf.Max(xIndex - 1, 0); x <= Mathf.Min(xIndex + 1, size); x++)
        {
            for (int y = Mathf.Max(yIndex - 1, 0); y <= Mathf.Min(yIndex + 1, size); y++)
            {
                if (direction == 0)
                {
                    var nearVert = verts[x * size + y];
                    return new Vector3((nearVert.x + v.x) / 2, (nearVert.y + v.y) / 2, (nearVert.z + v.z) / 2);
                }

                direction--;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// This returns information about enviroment items that are currently on the map, 
    /// like grass, in a json-friendly format.
    /// </summary>
    public EnviromentData GetEnviromentData()
    {
        if (placedDecorItems is null || placedDecorItems.Count == 0)
        {
            Debug.LogWarning("Enviroment items are not initialized and are being requested.");
        }

        return new EnviromentData() { items = placedDecorItems.ToArray() };
    }

    /// <summary>
    /// My messy begginer way of trying to get the slope of the surface. 
    /// This should be substituted with a method that uses ray casting. 
    /// </summary>
    private bool HasAcceptedHeightDifference(Vector3 v, int index, Vector3[] verts, float heightDifference)
    {
        var size = (int)MathF.Sqrt(verts.Count());

        int xIndex = index / size;
        int yIndex = index % size;

        if (verts[size * xIndex + yIndex] != v) Debug.LogWarning("Vertex doesn't check up.");

        for (int x = Mathf.Max(xIndex - 1, 0); x <= Mathf.Min(xIndex + 1, size); x++)
        {
            for (int y = Mathf.Max(yIndex - 1, 0); y <= Mathf.Min(yIndex + 1, size); y++)
            {
                if (Mathf.Abs(verts[size * x + y].y - v.y) > heightDifference)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

// Structs used to store information in a json format.
public struct EnviromentData
{
    public EnviromentItem[] items;
}

public struct EnviromentItem
{
    public Coords3D position;
    public Coords3D rotation;
    public Coords3D scale;
    public ItemType itemType;
}

public struct Coords3D
{
    public float x, y, z;
}
