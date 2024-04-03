using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Threading;

public enum NoiseType { Perlin, WarpedPerlin, PartiallyWarpedPerlin, Falloff};

[System.Serializable]
public struct Interval
{
    public float lowerLimit;
    public float upperLimit;
}

public class MapController : MonoBehaviour
{
    #region Editor Visible Variables
    public int mapSize = 300;
    public int screenWidth= 20;
    public int screenHeight;
    public int Xposition;
    public int Yposition;
    public int Zposition;

    public NoiseArgs noiseArgs;
    public int heightMultiplicator = 2;
    public bool improvedPerlin = true;
    public bool useSmooth = true;

    public bool hydraulicErosion = true;
    public ErosionArguments erosionArgs;
    public FalloffArgs falloffArgs;

    public float waterLevel = 0.01f;
    public float sinkStrength = 0.01f;

    public float domainWarpStrength = 10f;
    public float XYwarp = 2f;

    public NoiseType noiseType = NoiseType.Perlin;
    public bool updateEnviroment = false;
    public EnviromentController enviromentController;
    public bool autoUpdate;

    public NoiseMapDisplay2D visualizer;
    #endregion

    private string filePath = "/mapData.json";

    private double[,] noiseMap;

    public bool MapComputationDone;

    public void ComputeMap(bool useErosion = true, bool useEnviroment = true)
    {
        new Thread(() => 
        {
            MapComputationDone = false;
            hydraulicErosion = useErosion;
            updateEnviroment = useEnviroment;
            noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, noiseArgs, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffArgs, erosionArgs, waterLevel, sinkStrength, hydraulicErosion, improvedPerlin).noiseMap;
            MapComputationDone = true;
        }
        ).Start();
    }

    public void DrawMap()
    {
        MapDisplay display;
        display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

        if (updateEnviroment)
        {
            enviromentController.GenerateEnviroment();
        }
        else
        {
            enviromentController.DeleteEnviroment();
        }
    }

    public void GenerateMap()
    { 
        noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, noiseArgs, noiseType, domainWarpStrength, XYwarp, XYwarp/2, falloffArgs, erosionArgs, waterLevel, sinkStrength, hydraulicErosion, improvedPerlin).noiseMap;

        MapDisplay display;
        display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

        if (updateEnviroment)
        {
            enviromentController.GenerateEnviroment();
        }
    }

    public void GenerateEditorMaps()
    {
        MapDisplay display;

        var result = Noise.GenerateNoiseMap(mapSize, mapSize, noiseArgs, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffArgs, erosionArgs, waterLevel, sinkStrength, hydraulicErosion, improvedPerlin);


        visualizer.DrawNoiseMaps(result.noiseMap, result.noErosionMap, result.soilMap);

        display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(result.noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));
    }

    public void GenerateMap(bool useErosion = true, bool useEnviroment = true)
    {
        hydraulicErosion = useErosion;
        updateEnviroment = useEnviroment;
        MapDisplay display;

        noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, noiseArgs, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffArgs, erosionArgs, waterLevel, sinkStrength, useErosion).noiseMap;

        display = FindObjectOfType<MapDisplay>();

        display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

        if (updateEnviroment)
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
        noiseArgs.scale = mapData.noiseScale;
        noiseArgs.octaves = mapData.octaves;
        noiseArgs.persistance = mapData.persistance;
        noiseArgs.lacunarity = mapData.lacunarity;
        noiseArgs.seed = mapData.seed;
        heightMultiplicator = mapData.heightMultiplicator;
        hydraulicErosion = mapData.hydraulicErosion;
        erosionArgs = mapData.erosionArgs;
        falloffArgs.haveFalloff = mapData.haveFalloff;
        waterLevel = mapData.waterLevel;
        sinkStrength = mapData.sinkStrength;
        domainWarpStrength = mapData.domainWarpStrength;
        XYwarp = mapData.XYwarp;
        noiseType = mapData.noiseType;

        updateEnviroment = false;

        GenerateMap();
        
        enviromentController.LoadEnviroment(mapData.enviromentData);
        
    }

    IEnumerator MapVisualiseSeeds(float seconds)
    {
        Debug.Log("sTART CORUTINE");
        var initialDrops = erosionArgs.noDroplets;
        NoiseArgs localArgs = noiseArgs;

        for (int iseed = noiseArgs.seed; iseed < noiseArgs.seed + 20; iseed++)
        {
            MapDisplay display;
            localArgs.seed = iseed;
            noiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, localArgs, noiseType, domainWarpStrength, XYwarp, XYwarp / 2, falloffArgs, erosionArgs, waterLevel, sinkStrength, hydraulicErosion).noiseMap;

            display = FindObjectOfType<MapDisplay>();

            display.Draw3D(MeshGenerator.GenerateTerrainMesh(noiseMap, screenWidth, screenHeight, Xposition, Zposition, Yposition, heightMultiplicator));

            yield return new WaitForSeconds(seconds);
        }
    }


/*    IEnumerator MapVisualiseErosion(float seconds)
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
    }*/


    private void OnValidate()
    {
        if (mapSize < 1) mapSize = 1;
        if (mapSize > 1000) mapSize = 1000;
        if (noiseArgs.octaves > 20) noiseArgs.octaves = 20;
        if (noiseArgs.scale < 40) noiseArgs.scale = 40;
        if (noiseArgs.persistance < 0.1f) noiseArgs.persistance = 0.1f;
        if (noiseArgs.persistance > 1) noiseArgs.persistance = 1f;
        if (noiseArgs.lacunarity < 1) noiseArgs.lacunarity = 1;
        if (noiseArgs.lacunarity > 3) noiseArgs.lacunarity = 3;
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
            noiseScale = noiseArgs.scale,
            octaves = noiseArgs.octaves,
            persistance = noiseArgs.persistance,
            lacunarity = noiseArgs.lacunarity,
            seed = noiseArgs.seed,
            heightMultiplicator = heightMultiplicator,
            hydraulicErosion = hydraulicErosion,
            erosionArgs = erosionArgs,
            haveFalloff = falloffArgs.haveFalloff,
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

    public float waterLevel;
    public float sinkStrength;

    public float domainWarpStrength;
    public float XYwarp;

    public NoiseType noiseType;
    public EnviromentData enviromentData;
}
