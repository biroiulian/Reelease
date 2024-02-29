using UnityEngine;
using System.Collections;
using System;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using System.Drawing;
using System.Linq;
using Random = UnityEngine.Random;

public static class Noise
{
    private static Point[] falloffPoints = new Point[5];
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, NoiseType noiseType , float domainWarpStrength, float Xwarp, float Ywarp, float falloffRadius, bool resetFalloff, int terracesEasing, bool haveTerraces = false, bool haveFalloff = false)
    {
        if (noiseType == NoiseType.Falloff) return GenerateFalloffMap(mapWidth, falloffRadius, resetFalloff);

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float noiseHeight = 0;
                if (noiseType == NoiseType.Perlin)
                {
                    noiseHeight = fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x, y);
                }
                else if (noiseType == NoiseType.WarpedPerlin)
                {
                    noiseHeight = domainWarpFbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, domainWarpStrength, y, x, Xwarp, Ywarp);
                }
                else if (noiseType == NoiseType.PartiallyWarpedPerlin) 
                {
                    noiseHeight = (domainWarpFbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, domainWarpStrength, y, x, Xwarp, Ywarp) +
                        fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x, y))/2f;
                    
                }
                noiseMap[x, y] += noiseHeight;
            }
        }

        // find min and max heights
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (noiseMap[x,y] > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseMap[x, y];
                }
                else if (noiseMap[x, y] < minNoiseHeight)
                {
                    minNoiseHeight = noiseMap[x, y];
                }
            }
        }
        Debug.Log("Max: " + maxNoiseHeight + " Min: " + minNoiseHeight);


        // make the values be [0,1]
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        // falloff try
        float[,] falloff = null;
        if (haveFalloff)
        {
            falloff = GenerateFalloffMap(mapWidth, falloffRadius, resetFalloff);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (falloff[x ,y] == 0 ) noiseMap[x,y] = falloff[x ,y];
                    noiseMap[x, y] = noiseMap[x, y] * falloff[x, y];
                }
            }
        }

        // terrace try -> this needs the values to be normalised
        if (haveTerraces)
        {
            float[] terracesLevel = new float[] { 0.1f, 0.15f, 0.5f};
            noiseMap = makeIntoTerracesLight(noiseMap, terracesLevel, terracesEasing);
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = noiseMap[x, y] * noiseMap[x, y];
            }
        }

        return noiseMap;
    }

    private static float fbm(float scale, int octaves, float persistance, float lacunarity, Vector2[] octaveOffsets, float halfWidth, float halfHeight, float y, float x)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
            float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            //float perlinValue = DoWeirdStuff(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return noiseHeight;
    }

    public static float[,] makeIntoTerracesLight(float[,] noiseMap, float[] levels, int terracesEasing)
    { 
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        bool setLevel = false;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                foreach(float level in levels)
                {
                    if (noiseMap[i, j] < level) 
                    {
                        setLevel = true;
                        noiseMap[i, j] = (noiseMap[i, j] * terracesEasing + level)/(terracesEasing+1);
                        break;
                    }
                }
                // edge case for the last level
                setLevel = false;
            }
        }

        return noiseMap;
    }

    static float domainWarpFbm(float scale, int octaves, float persistance, float lacunarity, Vector2[] octaveOffsets, float halfWidth, float halfHeight, float domainWarpStrength, int y, int x, float Xwarp, float Ywarp)
    {
        // f(p) = f(p + off(p))
        return fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight,
            x+fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x + Xwarp, y + Ywarp)* domainWarpStrength,
            y+fbm(scale, octaves, persistance, lacunarity, octaveOffsets, halfWidth, halfHeight, x + Xwarp, y + Ywarp) * domainWarpStrength);
    }

    /// <summary>
    /// Create a falloff height map.
    /// </summary>
    /// <returns>Matrix of values between 0 and 1.</returns>
    public static float[,] GenerateFalloffMap(int size, float radius, bool resetFalloffPoints)
    {
        float[,] matrix = new float[size, size];
        if (resetFalloffPoints)
        {
            falloffPoints = new Point[]
            {
            new Point(size/2, size/2),
            new Point(Random.Range(size / 8 * 3, size/8*4), Random.Range(size / 8 * 3, size/8*4)),
            new Point(Random.Range(size / 8 * 4, size/8*5), Random.Range(size / 8 * 3, size/8*4)),
            new Point(Random.Range(size / 8 * 3, size/8*4), Random.Range(size / 8 * 4, size/8*5)),
            new Point(Random.Range(size / 8 * 4, size/8*5), Random.Range(size / 8 * 4, size/8*5)),
            };
        }
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

                if(distance > maxDistance)
                {  maxDistance = distance; }

                // Normalize distance to [0, 1]
                distance = Mathf.InverseLerp(0, maxDistance, distance);
                distance = 1 - distance; // Invert to have highest values in the center
                
                distance = distance * distance;

                if (distance < radius) distance = radius;
                matrix[i, j] = Mathf.InverseLerp(radius, 1, distance);
            }
        }

        return matrix;
    }

    public static float DoWeirdStuff(float x, float y)
    {
        //vec2 q = vec2(fbm(p + vec2(0.0, 0.0)), fbm(p + vec2(5.2, 1.3)));
        var newX = Mathf.PerlinNoise(x + 0, y+0);
        var newY = Mathf.PerlinNoise(x +  5.2f, y + 1.3f);

        return Mathf.PerlinNoise(x + 4 * newX, y + 4 * newY);

    }

}