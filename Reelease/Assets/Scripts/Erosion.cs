using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class Erosion
{

    public static float inertia = 0.001f;
    public static int radius = 1;
    public static float speed = 0.2f;

    public static float[,] erosionGradient1;
    public static float[,] erosionGradient2;

    public static float[,] Erode(float[,] heightMap, int mapSize, int noDroplets, int seed)
    {
        UnityEngine.Random.InitState(seed);
        erosionGradient1 = new float[3,3];
        erosionGradient1[0, 0] = 0.02f;
        erosionGradient1[0, 1] = 0.04f;
        erosionGradient1[0, 2] = 0.02f;
        erosionGradient1[1, 0] = 0.04f;
        erosionGradient1[1, 1] = 0.8f;
        erosionGradient1[1, 2] = 0.04f;
        erosionGradient1[2, 0] = 0.02f;
        erosionGradient1[2, 1] = 0.04f;
        erosionGradient1[2, 2] = 0.02f;

        erosionGradient2 = new float[3, 3];
        erosionGradient2[0, 0] = 0.05f;
        erosionGradient2[0, 1] = 0.1f;
        erosionGradient2[0, 2] = 0.05f;
        erosionGradient2[1, 0] = 0.1f;
        erosionGradient2[1, 1] = 0.4f;
        erosionGradient2[1, 2] = 0.1f;
        erosionGradient2[2, 0] = 0.05f;
        erosionGradient2[2, 1] = 0.1f;
        erosionGradient2[2, 2] = 0.05f;

        for (int i = 0; i < noDroplets; i++)
        {
            // Calculate starting position
            var xPos = UnityEngine.Random.Range(0, mapSize - 1);
            var yPos = UnityEngine.Random.Range(0, mapSize - 1);

            PlaceDroplet(ref heightMap, xPos, yPos);
        }

        return heightMap;

    }

    private static void PlaceDroplet(ref float[,] heightMap, int xPos, int yPos)
    {
        var currentX = xPos;
        var currentY = yPos;

        var lifetime = 25;
        var capacity = 0.1f;
        var sediment = 0f;
        var direction = 0;
        var erodedAmmount = 0f;
        var currentSpeed = speed;

        while (lifetime>0)
        {
            var nextPoint = GetLowestNeighbourAndDirection(heightMap, currentX, currentY, direction, currentSpeed);
            var nextX = nextPoint.x; var nextY = nextPoint.y; direction = nextPoint.dir;

            if (nextX == -1 && nextY == -1) return;

            // Update speed according to slope
            //currentSpeed = Mathf.Sqrt(currentSpeed * currentSpeed + (heightMap[currentX, currentY] - heightMap[nextX, nextY]) * 4f);

            // Calculate erosion
            erodedAmmount = Mathf.Min(capacity-sediment, Mathf.Abs(heightMap[currentX, currentY] - heightMap[nextX, nextY]));
            sediment += erodedAmmount;
            
            // Erode
            ErodeArea(heightMap, currentX, currentY, erodedAmmount);


            if (sediment < 0)
            {
                Debug.Log("Sediment negativ.");
            }

            // If he is his lowest neighbour, then he is a low minimum (lake bottom).
            if (nextX == currentX && nextY == currentY)
            {
                // Place sediment
                //heightMap[currentX, currentY] += capacity;
                PlaceSediment(heightMap, currentX, currentY, sediment);
                return;
            }
            else if (sediment > capacity)
            {
                // Place overflowing of sediment
                PlaceSediment(heightMap, currentX, currentY, capacity / 8);
                //PlaceSediment(heightMap, currentX, currentY, sediment-capacity/8);
                sediment -= capacity/8;
                return;
            }
            

            currentX = nextX;
            currentY = nextY;
            lifetime--;
            capacity -= capacity / lifetime;
        }

        PlaceSediment(heightMap, currentX, currentY, sediment);
    }

    private static void ErodeArea(float[,] noiseMap, int xPos, int yPos, float erodedAmmount)
    {
        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        int xBrush = 0, yBrush = 0;

        // We're creating a square around the point and try to spread the given sediment accordingly
        for (int x = Mathf.Max(xPos - 1, 0); x <= Mathf.Min(xPos + 1, width); x++)
        {
            yBrush = 0;
            for (int y = Mathf.Max(yPos - 1, 0); y <= Mathf.Min(yPos + 1, height); y++)
            {
                noiseMap[x, y] -= erodedAmmount * erosionGradient2[xBrush, yBrush];
                yBrush++;
            }
            xBrush++;
        }
    }

    private static void PlaceSediment(float[,] noiseMap, int xPos, int yPos, float sedimentAmmount)
    {
        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        int xBrush = 0, yBrush = 0;

        // We're creating a square around the point and try to spread the given sediment accordingly
        for (int x = Mathf.Max(xPos - 1, 0); x <= Mathf.Min(xPos + 1, width); x++)
        {
            yBrush = 0;
            for (int y = Mathf.Max(yPos - 1, 0); y <= Mathf.Min(yPos + 1, height); y++)
            {
                noiseMap[x, y] += (sedimentAmmount) * erosionGradient1[xBrush, yBrush] * noiseMap[x,y];
                yBrush++;
            }
            xBrush++;
        }
    }

    private static (int x, int y, int dir) GetLowestNeighbourAndDirection(float[,] noiseMap, int xPos, int yPos, int previousDirection, float speed)
    {
        
        if (previousDirection < 0 || previousDirection > 7) throw new ArgumentException(previousDirection.ToString());

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        // if out of the map, finish
        if (xPos - 1 < 0 || yPos - 1 < 0 || yPos + 1 >height || xPos + 1 > width)
        {
            return (-1, -1, -1);
        }

        float minValue = float.MaxValue;
        var nextDirection = -1;

        int minXPos = -1, minYPos = -1, localDirection = 0;

        for (int x = Mathf.Max(xPos - 1, 0); x <= Mathf.Min(xPos + 1, width); x++)
        {
            for (int y = Mathf.Max(yPos - 1, 0); y <= Mathf.Min(yPos + 1, height); y++)
            {
                if (y != yPos && x != xPos) // that means except for the center
                {
                    // the point on the direction that the droplet was already flowing towards
                    // has a little advantage to be picked.
                    if (previousDirection == localDirection)
                    {
                        // (weight * inertia) if weight is at full capacity then inertia acts
                        // to its maximum. if weight is zero, then inertia doesn't act at all.
                        if (noiseMap[x, y] - noiseMap[x, y] * inertia < minValue)
                        {
                            minValue = noiseMap[x, y];
                            minXPos = x;
                            minYPos = y;
                            // if this is the minimum point, this is the direction where it will flow
                            nextDirection = localDirection;
                        }
                    }
                    else
                    if (noiseMap[x, y] < minValue)
                    {
                        minValue = noiseMap[x, y];
                        minXPos = x;
                        minYPos = y;
                        // if this is the minimum point, this is the direction where it will flow
                        nextDirection = localDirection;
                    }

                    localDirection += 1;
                }
            }
        }

        return (minXPos, minYPos, nextDirection);
    }
}
