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
    public GameObject[] ModelsVariation;
    public int NoInstances;
    //public float Radius;
    public bool ignoreRandom;
    public Interval HeightInterval;
    public Interval ScaleVariation;
    public float TerrainHeightDiff;

    public float Scale;
    public int Octaves;
    public float Persistance;
    public float Lacunarity;

    public List<Vector3> PlacedPositions;

    private float[,] SpreadMap;

    public void SetSpreadMap(float[,] spreadMap) {  SpreadMap = spreadMap; }
    public float[,] GetSpreadMap() {  return SpreadMap; }
}

public class EnviromentController : MonoBehaviour
{
    [SerializeField]
    public PlaceableObject[] PlaceableObjects;
    private Vector3[] PlacedObjects;

    public bool autoUpdate = true;
    public bool resetPlacement = true;

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
        DrawObjects();
    }

    private void Initialize()
    {
        DeleteObjects();
        int seedOffset = 1;
        for (int i = 0; i < PlaceableObjects.Length; i++)
        {
            PlaceableObjects[i].SetSpreadMap(Noise.GenerateBinaryNoiseMap(PlaceableObjects[i].Scale, PlaceableObjects[i].Octaves, PlaceableObjects[i].Persistance, PlaceableObjects[i].Lacunarity, seedOffset));
            seedOffset++;
        }
    }

    private void DeleteObjects()
    {
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }

    public void LoadEnviroment()
    {
        DrawObjects();
    }


    private void DrawObjects()
    {       
        for (int i = 0; i < PlaceableObjects.Length; i++)
        {
            Debug.Log("Drawing " + PlaceableObjects[i].PlacedPositions.Count + " objects...");

            for( int j = 0; j < PlaceableObjects[i].PlacedPositions.Count; j++)
            {
                var instance = Instantiate<GameObject>(PlaceableObjects[i].ModelsVariation[UnityEngine.Random.Range(0, PlaceableObjects[i].ModelsVariation.Length)]);
                instance.transform.position = new Vector3(PlaceableObjects[i].PlacedPositions[j].x, PlaceableObjects[i].PlacedPositions[j].y, PlaceableObjects[i].PlacedPositions[j].z);
                // Variation tweaks
                // assign a random rotation on the Y axis
                instance.transform.Rotate(new Vector3(0f, 0f, UnityEngine.Random.Range(0, 180) * 1f));
                // assign a random height scale
                instance.transform.localScale = new Vector3(
                    instance.transform.localScale.x, 
                    UnityEngine.Random.Range(PlaceableObjects[i].ScaleVariation.lowerLimit, PlaceableObjects[i].ScaleVariation.upperLimit) * instance.transform.localScale.y, 
                    instance.transform.localScale.z);
                instance.transform.parent = transform;
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
        for(int i = 0; i < vertsSize; i++)
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
                if(direction == 0)
                {
                    var nearVert = verts[x * size + y];
                    return new Vector3((nearVert.x + v.x) / 2, (nearVert.y + v.y) / 2, (nearVert.z + v.z) / 2);
                }

                direction--;
            }
        }

        return Vector3.zero;
    }

    private bool HasAcceptedHeightDifference(Vector3 v, int index, Vector3[] verts, float heightDifference)
    {
        var size = (int)MathF.Sqrt(verts.Count());

        int xIndex = index/size;
        int yIndex = index%size;

        if (verts[size * xIndex + yIndex] != v) Debug.LogWarning("Vertex doesn't check up."); 

        for (int x = Mathf.Max(xIndex - 1, 0); x <= Mathf.Min(xIndex + 1, size); x++)
        {
            for (int y = Mathf.Max(yIndex - 1, 0); y <= Mathf.Min(yIndex + 1, size); y++)
            {
                if (Mathf.Abs(verts[size*x + y].y - v.y) > heightDifference)
                {
                    return false;
                }
            }
        }

        return true;
    }

}
