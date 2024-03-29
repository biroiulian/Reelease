using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Random = UnityEngine.Random;
using TreeEditor;

public static class Noise
{
    public static int mapSize = 300;
    public static int screenWidth = 20;
    public static int screenHeight;
    public static int Xposition;
    public static int Yposition;
    public static int Zposition;
    public static float seaLevel;
    public static float noiseScale = 2;
    public static int octaves = 1;
    public static float persistance;
    public static float lacunarity;
    public static int seed = 0;

    private static Point[] falloffPoints = new Point[5];
    private static double[,] falloffMapCached;
    private static double[,] noiseMapCached;
    public static double[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, NoiseType noiseType, float domainWarpStrength, float Xwarp, float Ywarp, float falloffRadius, bool resetFalloff, ErosionArguments erosionArgs, float waterLevel, float sinkStrength, AnimationCurve falloffCurve, bool haveFalloff = false, bool haveHydraErosion = false, bool smooth= true, bool useImprovedPerlin = true)
    {
        CacheParameters(mapWidth, seed, scale, octaves, persistance, lacunarity, waterLevel);

        if (noiseType == NoiseType.Falloff)
        {
            // falloffMapCached = GenerateFalloffMap(mapWidth, falloffRadius, resetFalloff);
            falloffMapCached = GenerateFalloffMap(mapWidth, falloffRadius);
            
            return falloffMapCached;
        }

        double[,] noiseMap = new double[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
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
                    noiseHeight = fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin);
                }
                else if (noiseType == NoiseType.WarpedPerlin)
                {
                    noiseHeight = domainWarpFbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, domainWarpStrength, y, x, Xwarp, Ywarp, useImprovedPerlin);
                }
                else if (noiseType == NoiseType.PartiallyWarpedPerlin)
                {
                    noiseHeight = (domainWarpFbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, domainWarpStrength, y, x, Xwarp, Ywarp, useImprovedPerlin) +
                        fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x, y, useImprovedPerlin)) / 2f;
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

        // try to smooth out the artifacts
        if (smooth) { 
            for (int y = 1; y < mapHeight - 1; y++)
            {
                for (int x = 1; x < mapWidth - 1; x++)
                {
                    double sum = 0.0d;
                    for (int nx = x - 1; nx <= x + 1; nx++)
                    {
                        for (int ny = y - 1; ny <= y + 1; ny++)
                        {
                            sum += noiseMap[nx, ny];
                        }
                    }
                    noiseMap[x, y] = sum / 9;
                }
            }
    }


        // falloff try
        if (haveFalloff)
        {
            falloffMapCached = GenerateFalloffMap(mapWidth, falloffRadius);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (falloffMapCached[x, y] == 0)
                        noiseMap[x, y] = 0;
                    else
                        noiseMap[x, y] = noiseMap[x, y] * falloffMapCached[x, y];
                }
            }
        }

        // Rain erosion
        if (haveHydraErosion)
        {
            Erosion.Erode(noiseMap, mapWidth, seed, erosionArgs);
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
                        noiseMap[x, y] = noiseMap[x, y] - sinkStrength;
                    }
                }
            }
        }

        noiseMapCached = noiseMap;
        return noiseMap;
    }



    private static void CacheParameters(int mapWidth, int seed, float scale, int octaves, float persistance, float lacunarity, float waterLevel)
    {
        mapSize = mapWidth;
        noiseScale = scale;
        Noise.octaves = octaves;
        Noise.persistance = persistance;
        Noise.lacunarity = lacunarity;
        Noise.seed = seed;
        Noise.seaLevel = waterLevel;
    }

    public static float[,] GenerateBinaryNoiseMap(float scale, int octaves, float persistance, float lacunarity, int seedOffset)
    {
        float[,] noiseMap = new float[mapSize, mapSize];

        System.Random prng = new System.Random(seed + seedOffset);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
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
                var height = fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfSize, halfSize, x, y, true);
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
    private static double fbm(float scale, int octaves, float persistance, float lacunarity, Vector2[] octaveOffsets, float halfWidth, float halfHeight, double y, double x, bool useImprovedPerlin)
    {
        float amplitude = 1;
        float frequency = 1;
        double noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            double sampleX = (x - halfHeight) / scale * frequency + octaveOffsets[i].x;
            double sampleY = (y - halfWidth) / scale * frequency + octaveOffsets[i].y;

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

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return noiseHeight;
    }

    static double domainWarpFbm(float scale, int octaves, float persistance, float lacunarity, Vector2[] octaveOffsets, float halfWidth, float halfHeight, double domainWarpStrength, double y, double x, double Xwarp, double Ywarp, bool useImprovedPerlin)
    {
        // f(p) = f(p + off(p))
        return fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight,
            x + fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x + Xwarp, y + Ywarp, useImprovedPerlin) * domainWarpStrength,
            y + fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x + Xwarp, y + Ywarp, useImprovedPerlin) * domainWarpStrength, useImprovedPerlin);
    }

    /// <summary>
    /// Create a falloff height map.
    /// </summary>
    /// <returns>Matrix of values between 0 and 1.</returns>
    public static double[,] GenerateFalloffMap(int size, float radius)
    {
        Random.InitState(seed);
        double[,] matrix = new double[size, size];

        falloffPoints = new Point[]
        {
            new Point(size/2, size/2),
            new Point(Random.Range(size / 8 * 3, size/8*4), Random.Range(size / 8 * 3, size/8*4)),
            new Point(Random.Range(size / 8 * 4, size/8*5), Random.Range(size / 8 * 3, size/8*4)),
            new Point(Random.Range(size / 8 * 3, size/8*4), Random.Range(size / 8 * 4, size/8*5)),
            new Point(Random.Range(size / 8 * 4, size/8*5), Random.Range(size / 8 * 4, size/8*5)),
        };

        float maxDistance = float.MinValue;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x, y, distance = float.MaxValue, tryDistance;
                x = i;
                y = j;
                foreach (Point point in falloffPoints)
                {
                    tryDistance = Mathf.Sqrt(Mathf.Pow((x - point.x), 2) + Mathf.Pow((y - point.y), 2));
                    if (distance > tryDistance)
                    {
                        distance = tryDistance;
                    }
                }

                if (distance > maxDistance)
                { maxDistance = distance; }

                // Normalize distance to [0, 1]
                distance = Mathf.InverseLerp(0, maxDistance, distance);
                distance = 1 - distance; // Invert to have highest values in the center

                distance *= distance;

                if (distance < radius - 0.06f) distance = 0;
                if (distance > radius + 0.06f) distance = 1;
                else distance = Mathf.InverseLerp(radius - 0.06f, radius + 0.06f, distance);
                matrix[i, j] = Mathf.InverseLerp(0, 1, distance);
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