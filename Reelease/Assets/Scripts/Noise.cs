using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using System;

public static class Noise
{
    public static int mapSize = 300;
    public static int screenWidth = 20;
    public static int screenHeight;
    public static int Xposition;
    public static int Yposition;
    public static int Zposition;
    public static float seaLevel;
    private static NoiseArgs noiseArgs;

    private static Point[] falloffPoints = new Point[5];
    private static double[,] falloffMapCached;
    private static double[,] noiseMapCached;
    private static double[,] noErosionMap;
    private static double[,] soilMap;

    public static (double[,] noiseMap, double[,] noErosionMap, double[,] soilMap) GenerateNoiseMap(
        int mapWidth, int mapHeight, NoiseArgs noiseArgs, 
        NoiseType noiseType, float domainWarpStrength, float Xwarp, float Ywarp, FalloffArgs falloffArgs, 
        ErosionArguments erosionArgs, float waterLevel, float sinkStrength, bool haveHydraErosion = false,
        bool useImprovedPerlin = true)
    {
        CacheParameters(mapWidth, noiseArgs, waterLevel);

        if (noiseType == NoiseType.Falloff)
        {
            // falloffMapCached = GenerateFalloffMap(mapWidth, falloffRadius, resetFalloff);
            falloffMapCached = GenerateFalloffMap(mapWidth, falloffArgs);
            
            return (falloffMapCached, null, null);
        }

        double[,] noiseMap = new double[mapWidth, mapHeight];
        soilMap = new double[mapWidth, mapHeight];


        System.Random prng = new System.Random(noiseArgs.seed);
        Vector2[] octaveOffsets = new Vector2[noiseArgs.octaves];
        for (int i = 0; i < noiseArgs.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                double noiseHeight = 0;
                if (noiseType == NoiseType.Perlin)
                {
                    noiseHeight = fbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin);
                    soilMap[x, y] = fbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin);
                }
                else if (noiseType == NoiseType.WarpedPerlin)
                {
                    noiseHeight = domainWarpFbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, domainWarpStrength, y, x, Xwarp, Ywarp, useImprovedPerlin);
                    soilMap[x, y] = fbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin);
                }
                else if (noiseType == NoiseType.PartiallyWarpedPerlin)
                {
                    noiseHeight = (domainWarpFbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, domainWarpStrength, y, x, Xwarp, Ywarp, useImprovedPerlin) +
                        fbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin)) / 2f;
                    soilMap[x, y] = fbm(noiseArgs, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin);
                }

                noiseMap[x, y] += noiseHeight;
            }
        }

        // find min and max heights
        double maxNoiseHeight = double.MinValue;
        double minNoiseHeight = double.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (noiseMap[x, y] > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseMap[x, y];
                }
                else if (noiseMap[x, y] < minNoiseHeight)
                {
                    minNoiseHeight = noiseMap[x, y];
                }
            }
        }

        // make the values be [0,1]
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        // falloff try
        if (falloffArgs.haveFalloff)
        {
            falloffMapCached = GenerateFalloffMap(mapWidth, falloffArgs);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = noiseMap[x, y] * falloffMapCached[x, y];
                }
            }
        }

        noErosionMap = new double[mapHeight, mapWidth];
        Array.Copy(noiseMap, noErosionMap, noiseMap.Length);

        // Rain erosion
        if (haveHydraErosion)
        {
            Erosion.Erode(noiseMap, mapWidth, noiseArgs.seed, erosionArgs);
        }

        // Apply sinking of values smaller than water level.
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (noiseMap[x, y] < waterLevel) noiseMap[x, y] = -sinkStrength;
                else
                {
                    var suroundedBy = 0; // how many of the surounding points are below sea level

                    for (int nx = x - 1; nx <= x + 1 && nx < mapHeight; nx++)
                    {
                        if (nx < 0) nx = 0;
                        for (int ny = y-1 ; ny <= y + 1 && ny < mapWidth; ny++)
                        {
                            if (ny < 0) ny = 0;
                            if (noiseMap[nx, ny] < waterLevel) suroundedBy++;
                        }
                    }
                    if (suroundedBy >= 6)
                    {
                        Debug.Log("Sinked this.");
                        noiseMap[x, y] = noiseMap[x, y] - sinkStrength;
                    }
                }
            }
        }

        noiseMapCached = noiseMap;
        
        return (noiseMap, noErosionMap, soilMap);
    }



    private static void CacheParameters(int mapWidth, NoiseArgs noiseArgs, float waterLevel)
    {
        mapSize = mapWidth;
        Noise.noiseArgs = noiseArgs;
        seaLevel = waterLevel;
    }

    public static float[,] GenerateBinaryNoiseMap(NoiseArgs args, int seedOffset)
    {
        float[,] noiseMap = new float[mapSize, mapSize];

        System.Random prng = new System.Random(noiseArgs.seed + seedOffset);
        Vector2[] octaveOffsets = new Vector2[args.octaves];
        for (int i = 0; i < args.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float halfSize = mapSize / 2f;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                var height = fbm(args, octaveOffsets, halfSize, halfSize, x, y, true);
                if (height > -0.2f && noiseMapCached[x, y] > seaLevel)
                    noiseMap[x, y] = 1;
                else
                    noiseMap[x, y] = 0;
            }
        }

        return noiseMap;
    }

    /// <summary>
    /// My noise method that uses PerlinNoise.
    /// </summary>
    /// <returns> A value between -1 and 1. </returns>
    private static double fbm(NoiseArgs args, Vector2[] octaveOffsets, float halfWidth, float halfHeight, double y, double x, bool useImprovedPerlin)
    {
        float amplitude = 1;
        float frequency = 1;
        double noiseHeight = 0;

        for (int i = 0; i < args.octaves; i++)
        {
            double sampleX = (x - halfHeight) / args.scale * frequency + octaveOffsets[i].x;
            double sampleY = (y - halfWidth) / args.scale * frequency + octaveOffsets[i].y;

            if (useImprovedPerlin)
            {
                double perlinValue = ImprovedNoise.Noise(sampleX, sampleY, 0) * 2 - 1;
                noiseHeight += perlinValue * amplitude;
            }
            else
            {
                float perlinValue = Mathf.PerlinNoise((float)sampleX, (float)sampleY);
                noiseHeight += perlinValue * amplitude;
            }

            amplitude *= args.persistance;
            frequency *= args.lacunarity;
        }

        return noiseHeight;
    }

    static double domainWarpFbm(NoiseArgs args, Vector2[] octaveOffsets, float halfWidth, float halfHeight, double domainWarpStrength, double y, double x, double Xwarp, double Ywarp, bool useImprovedPerlin)
    {
        // f(p) = f(p + off(p))
        return fbm(args, octaveOffsets, halfWidth, halfHeight,
            x + fbm(args, octaveOffsets, halfWidth, halfHeight, x + Xwarp, y + Ywarp, useImprovedPerlin) * domainWarpStrength,
            y + fbm(args, octaveOffsets, halfWidth, halfHeight, x + Xwarp, y + Ywarp, useImprovedPerlin) * domainWarpStrength, useImprovedPerlin);
    }

    /// <summary>
    /// Create a falloff height map.
    /// </summary>
    /// <returns>Matrix of values between 0 and 1.</returns>
    public static double[,] GenerateFalloffMap(int size, FalloffArgs falloffArgs)
    {
        // Generate random points on a square-border. The points are generated inside cells that have a fixed size.
        // These points are used to calculate the distance
        var rand = new System.Random(noiseArgs.seed);
        double[,] matrix = new double[size, size];

        int cellSize = falloffArgs.cellSize;
        int borderSize = falloffArgs.borderSize;
        int noCells = (size / cellSize - borderSize * 2 - 1) * 4; 

        falloffPoints = new Point[noCells];
        int cellNo = 0;
        for(int i = cellSize*borderSize; i < size-cellSize*borderSize; i += cellSize)
        {
            for (int j = cellSize * borderSize; j < size - cellSize * borderSize; j += cellSize)
            {
                if (i == cellSize * borderSize || j == cellSize * borderSize || 
                    i == size - cellSize * (borderSize+1) || j == size - cellSize * (borderSize + 1))
                {
                    falloffPoints[cellNo] = new Point(rand.Next(i, i + cellSize), rand.Next(j, j + cellSize));
                    cellNo++;
                }
            }
        }


        float maxDistance = float.MinValue;

        // Calculate distance of each point from all the points on the border.
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x, y, distance = float.MaxValue, tryDistance;
                x = i;
                y = j;
                foreach (Point point in falloffPoints)
                {
                    tryDistance = Mathf.Sqrt(Mathf.Pow(x - point.x, 2) + Mathf.Pow(y - point.y, 2));
                    if (distance > tryDistance)
                    {
                        distance = tryDistance;
                    }
                }

                if (distance > maxDistance)
                { maxDistance = distance; }

                // Normalize distance to [0, 1]
                distance = Mathf.InverseLerp(0, maxDistance, distance);
                distance = 1 - distance;
                if (distance < falloffArgs.radius - falloffArgs.radiusSize) distance = 0;
                if (distance > falloffArgs.radius + falloffArgs.radiusSize) distance = 1;
                else distance = falloffArgs.heightCurve.Evaluate(Mathf.InverseLerp(falloffArgs.radius - falloffArgs.radiusSize, falloffArgs.radius + falloffArgs.radiusSize, distance));

                matrix[i, j] = distance;
            }
        }

        // Fill the middle
        for (int i = cellSize * (borderSize); i < size - cellSize * (borderSize + 1); i++)
        {
            for (int j = cellSize * (borderSize + 1); j < size - cellSize * (borderSize + 1); j++)
            {
                matrix[i, j] = 1;
            }
        }

        return matrix;
    }

    // Interpolates between /a/ and /b/ by /t/. /t/ is clamped between 0 and 1.
    public static double Lerp(double a, double b, double t)
    {
        if (t < 0.0d) t = 0.0d;
        if (t > 1.0d) t = 1.0d;
        return a + (b - a) * t;
    }

    public static double InverseLerp(double a, double b, double value)
    {
        if (a != b)
        {
            double res = (value - a) / (b - a);
            if (res < 0.0d) res = 0.0d; 
            if (res > 1.0d) res = 1.0d;
            return res;
        }

        return 0f;
    }
}

[System.Serializable]
public struct NoiseArgs
{
    public int seed;
    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;
}

[System.Serializable]
public struct FalloffArgs
{
    public bool haveFalloff;
    public int borderSize;
    public int cellSize;
    public float radius;
    public float radiusSize;
    public AnimationCurve heightCurve;
}