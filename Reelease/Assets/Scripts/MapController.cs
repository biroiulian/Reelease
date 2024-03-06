using UnityEngine;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using static UnityEngine.Mesh;
using System.Linq;

public enum ColorMode { NoiseMap, LerpedColorMap, SharpColorMap };
public enum DrawMode { Flat, ThreeDimensional };
public enum NoiseType { Perlin, WarpedPerlin, PartiallyWarpedPerlin, Falloff};

/// <summary>
/// Used to represent a color and its ratio on the map. 
/// By ratio, I mean how much of the (0,1) interval is allocated to that color.
/// </summary>
[System.Serializable]
public class ColorFloatPair
{
    public UnityEngine.Color color;
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
    public int mapSize = 300;
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

    public bool autoUpdate;
    public bool hydraulicErosion = true;
    public int noDroplets = 10000;
    public int radius = 3;
    public int heightMultiplicator = 2;
    public bool haveFalloff = false;
    public float falloffRadius = 1.5f;
    public bool resetFalloff = false;
    public bool haveTerraces = false;
    public int terracesEasing = 5;
    public NoiseType noiseType = NoiseType.Perlin;
    public float domainWarpStrength = 10f;
    public float XYwarp = 2f;
    #endregion

    private float[,] noiseMap;

    public void GenerateMap()
    {

        noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, noiseType, domainWarpStrength, XYwarp, XYwarp, falloffRadius, resetFalloff, terracesEasing, noDroplets, haveTerraces, haveFalloff, hydraulicErosion);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator), display.GetTexture(display.GetColorMap(noiseMap, ColorsWithRatios, colorMode), mapSize, mapSize));

    }


    private void OnValidate()
    {
        //if (fillingCapacity<=0) fillingCapacity = 0.01f;
        //if (mapWidth < 1) mapWidth = 1;
        //if (mapWidth > 1000) mapWidth = 1000;
        if (terracesEasing < 1) { terracesEasing = 1; }
        if (mapSize < 1) mapSize = 1;
        if (mapSize > 500) mapSize = 500;
        if (octaves > 20) octaves = 20;
        if (noiseScale > 100) noiseScale = 100;
        if (persistance < 0.1f) persistance = 0.1f;
        if (persistance > 1) persistance = 1f;
        if (lacunarity < 1) lacunarity = 1;
        if (lacunarity > 3) lacunarity = 3;
    }

}
