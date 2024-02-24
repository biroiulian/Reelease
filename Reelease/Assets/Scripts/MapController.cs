using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor.AssetImporters;
using Unity.VisualScripting;

public enum ColorMode { NoiseMap, LerpedColorMap, SharpColorMap };
public enum DrawMode { Flat, ThreeDimensional };

/// <summary>
/// Used to represent a color and its ratio on the map. 
/// By ratio, I mean how much of the (0,1) interval is allocated to that color.
/// </summary>
[System.Serializable]
public class ColorFloatPair
{
    public Color color;
    public float ratio;
}

[System.Serializable]
public struct HeightInterval
{
    public float lowerLimit;
    public float upperLimit;
}

public class MapController : MonoBehaviour
{
    public static float SCREEN_RATIO = 1;

    #region Editor Visible Variables
    public int mapWidth = 1;
    public int mapHeight;
    public int screenWidth= 20;
    public int screenHeight;
    public int Xposition;
    public int Yposition;
    public int Zposition;

    public float noiseScale = 2;
    public int octaves = 1;
    public float persistance;
    public float lacunarity;
    public int seed = 0;

    public ColorFloatPair[] ColorsWithRatios;

    public ColorMode colorMode = ColorMode.NoiseMap;
    public DrawMode drawMode = DrawMode.ThreeDimensional;

    public int heightMultiplicator = 2;

    public int riverCount = 1;
    public float fillingCapacity;
    public HeightInterval riverStartHeightInterval;
    public bool showLocalMin = false;
    public bool showPassages = false;

    public bool autoUpdate;
    #endregion

    public void GenerateMap()
    {
        SetHeightBasedOnScreenRatio();

        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity);

        var additionalPoints = new Dictionary<Color, Collection<Point>>();

        if (riverCount > 0) {
            ErosionAlg.FillingCapacity = fillingCapacity;
            var riversList = ErosionAlg.GetRivers(noiseMap, riverCount, riverStartHeightInterval);
            additionalPoints.Add(Color.blue, riversList);
        }

        if (showLocalMin)
        {
            var localMinPoints = ErosionAlg.GetLocalMinPoints(noiseMap);
            additionalPoints.Add(Color.yellow, localMinPoints);
        }
        if(showPassages)
        {
            var passagePoints = ErosionAlg.GetPassagePoints(noiseMap);
            additionalPoints.Add(Color.cyan, passagePoints);
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        Color[] cMap = display.GetColorMap(noiseMap, ColorsWithRatios, colorMode, additionalPoints);

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator), display.GetTexture(cMap, mapWidth, mapHeight));

    }

    private void SetHeightBasedOnScreenRatio()
    {
        Debug.Log("Sys h: " + Display.main.systemHeight + " sys w: " + Display.main.systemWidth);
        SCREEN_RATIO = (float)Display.main.systemHeight / (float)Display.main.systemWidth;
        mapHeight = (int)Mathf.Ceil(mapWidth * SCREEN_RATIO);
        screenHeight = (int)Mathf.Ceil(screenWidth * SCREEN_RATIO);
    }

    private void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapWidth > 1000) mapWidth = 1000;
        if (octaves > 20) octaves = 20;
        if (noiseScale > 100) noiseScale = 100;
        if (persistance < 0.1f) persistance = 0.1f;
        if (persistance > 1) persistance = 1f;
        if (lacunarity < 1) lacunarity = 1;
        if (lacunarity > 3) lacunarity = 3;
    }

}
