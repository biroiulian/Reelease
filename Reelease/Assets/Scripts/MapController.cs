using UnityEngine;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using static UnityEngine.Mesh;
using System.Linq;
using System.Collections;
using System.Security.Cryptography;
using Color = UnityEngine.Color;

public enum NoiseType { Perlin, WarpedPerlin, PartiallyWarpedPerlin, Falloff};

[System.Serializable]
public struct Interval
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
    public int heightMultiplicator = 2;
    public bool improvedPerlin = true;
    public bool useSmooth = true;

    public bool hydraulicErosion = true;
    public ErosionArguments erosionArgs;

    public bool haveFalloff = false;
    public float falloffRadius = 1.5f;
    public AnimationCurve falloffCurve;
    public bool resetFalloff = false;

    public float waterLevel = 0.01f;
    public float sinkStrength = 0.01f;

    public float domainWarpStrength = 10f;
    public float XYwarp = 2f;

    public NoiseType noiseType = NoiseType.Perlin;
    public bool updateEnviroment = false;
    public EnviromentController enviromentController;
    public bool autoUpdate;

    public TestMapDisplay testDisplay;
    #endregion

    private string filePath = "/mapData.json";

    private void Start()
    {
        //StartCoroutine(MapVisualiseErosion(2f));
    }

    private double[,] noiseMap;

    public void GenerateMap()
    {
        MapDisplay display;

        noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, noiseType, domainWarpStrength, XYwarp, XYwarp/2, falloffRadius, resetFalloff, erosionArgs, waterLevel, sinkStrength, falloffCurve, haveFalloff, hydraulicErosion, useSmooth, improvedPerlin);

        testDisplay.DrawNoiseMap(noiseMap);

        display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

        if (updateEnviroment)
        {
            enviromentController.GenerateEnviroment();
        }
    }

    public void GenerateMap(bool useErosion = true, bool useEnviroment = true)
    {
        hydraulicErosion = useErosion;
        updateEnviroment = useEnviroment;
        MapDisplay display;

        noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffRadius, resetFalloff, erosionArgs, waterLevel, sinkStrength, falloffCurve, haveFalloff, useErosion);

        display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

        if (useEnviroment)
        {
            enviromentController.GenerateEnviroment();
        }
        else
        {
            enviromentController.DeleteEnviroment();
        }
    }

    public void SaveMap()
    {
        Debug.Log("Attempting save of the map.");
        if (JsonDataService.FileExists(filePath))
        {
            Debug.LogWarning("Suspicious behaviour: Trying to save map but map already exists. Maybe you wanted to save enviroment? Proceeding with overriding.");
        }

        if (!updateEnviroment)
        {
            Debug.Log("We didn't generate the enviroment, so we generate it now. We can't have a map without vegetation.");
            enviromentController.GenerateEnviroment();
        }

        JsonDataService.SaveData(filePath, GetMapProps());
    }

    public void LoadMap()
    {
        var mapData = JsonDataService.LoadData<MapPropsHolder>(filePath);
        mapSize = mapData.mapSize;
        screenWidth = mapData.screenWidth;
        screenHeight = mapData.screenHeight;
        Xposition = mapData.Xposition;
        Yposition = mapData.Yposition;
        Zposition = mapData.Zposition;
        noiseScale = mapData.noiseScale;
        octaves = mapData.octaves;
        persistance = mapData.persistance;
        lacunarity = mapData.lacunarity;
        seed = mapData.seed;
        heightMultiplicator = mapData.heightMultiplicator;
        hydraulicErosion = mapData.hydraulicErosion;
        erosionArgs = mapData.erosionArgs;
        haveFalloff = mapData.haveFalloff;
        resetFalloff = mapData.resetFalloff;
        waterLevel = mapData.waterLevel;
        sinkStrength = mapData.sinkStrength;
        domainWarpStrength = mapData.domainWarpStrength;
        XYwarp = mapData.XYwarp;
        noiseType = mapData.noiseType;

        updateEnviroment = false;

        GenerateMap();
        
        enviromentController.LoadEnviroment(mapData.enviromentData);
        
    }
/*
    IEnumerator MapVisualiseSeeds(float seconds)
    {
        Debug.Log("sTART CORUTINE");
        var initialDrops = erosionArgs.noDroplets;
        for (int drops = 0; drops < initialDrops; drops += 1000)
        {
            MapDisplay display;
            XYwarp += 1;
            //erosionArgs.noDroplets = drops;
            noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffRadius, resetFalloff, erosionArgs, waterLevel, sinkStrength, haveFalloff, hydraulicErosion);

            display = FindObjectOfType<MapDisplay>();

            display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

            yield return new WaitForSeconds(seconds);
        }
    }


    IEnumerator MapVisualiseErosion(float seconds)
    {
        Debug.Log("sTART CORUTINE");
        for (int iseed = seed; iseed < seed + 50; iseed ++)
        {
            MapDisplay display;

            noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, iseed, noiseScale, octaves, persistance, lacunarity, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffRadius, resetFalloff, erosionArgs, waterLevel, sinkStrength, haveFalloff, hydraulicErosion);

            display = FindObjectOfType<MapDisplay>();

            display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

            if (updateEnviroment)
            {
                enviromentController.GenerateEnviroment();
            }

            yield return new WaitForSeconds(seconds);
        }
    }
*/

    private void OnValidate()
    {
        if (mapSize < 1) mapSize = 1;
        if (mapSize > 1000) mapSize = 1000;
        if (octaves > 20) octaves = 20;
        if (noiseScale < 40) noiseScale = 40;
        if (persistance < 0.1f) persistance = 0.1f;
        if (persistance > 1) persistance = 1f;
        if (lacunarity < 1) lacunarity = 1;
        if (lacunarity > 3) lacunarity = 3;
    }

    internal MapPropsHolder GetMapProps()
    {
        var mapProps = new MapPropsHolder()
        {
            mapSize = mapSize,
            screenWidth = screenWidth,
            screenHeight = screenHeight,
            Xposition = Xposition,
            Yposition = Yposition,
            Zposition = Zposition,
            noiseScale = noiseScale,
            octaves = octaves,
            persistance = persistance,
            lacunarity = lacunarity,
            seed = seed,
            heightMultiplicator = heightMultiplicator,
            hydraulicErosion = hydraulicErosion,
            erosionArgs = erosionArgs,
            haveFalloff = haveFalloff,
            resetFalloff = resetFalloff,
            waterLevel = waterLevel,
            sinkStrength = sinkStrength,
            domainWarpStrength = domainWarpStrength,
            XYwarp = XYwarp,
            noiseType = noiseType,
            enviromentData = enviromentController.GetEnviromentData(),
        };

        return mapProps;
    }
}

public struct MapPropsHolder
{
    public int mapSize;
    public int screenWidth;
    public int screenHeight;
    public int Xposition;
    public int Yposition;
    public int Zposition;

    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;
    public int heightMultiplicator;

    public bool hydraulicErosion;
    public ErosionArguments erosionArgs;

    public bool haveFalloff;
    public float falloffRadius;
    public bool resetFalloff;

    public float waterLevel;
    public float sinkStrength;

    public float domainWarpStrength;
    public float XYwarp;

    public NoiseType noiseType;
    public EnviromentData enviromentData;
}
