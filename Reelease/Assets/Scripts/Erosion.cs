using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using UnityEngine;

[Serializable]
public struct ErosionArguments
{
    public int noDroplets;
    public float inertia;
    public int radius;
    public float speed;
    public int lifetime;
    public float capacity;
}

public static class Erosion
{

    public static int radius = 1;

    public static float[,] sedimentPlacementMask;
    public static float[,] erosionMask;

    public static float[,] Erode(float[,] heightMap, int mapSize, int seed, ErosionArguments args)
    {
        InitalizeValues(args);

        UnityEngine.Random.InitState(seed);

        sedimentPlacementMask = GenerateMatrix(args.radius, 1);
        erosionMask = GenerateMatrix(1, 3f);

        for (int i = 0; i < args.noDroplets; i++)
        {
            // Calculate starting position
            var xPos = UnityEngine.Random.Range(0, mapSize - 1);
            var yPos = UnityEngine.Random.Range(0, mapSize - 1);

            PlaceDroplet(ref heightMap, xPos, yPos, args);
        }

        return heightMap;

    }

    private static void InitalizeValues(ErosionArguments args)
    {
        radius = args.radius;
    }

    private static void PlaceDroplet(ref float[,] heightMap, int xPos, int yPos, ErosionArguments args)
    {
        var currentX = xPos;
        var currentY = yPos;
        var lifetime = args.lifetime;
        var capacity = args.capacity;
        var inertia = args.inertia;
        var sediment = 0f;
        var direction = -1;
        var speed = args.speed;

        while (lifetime > 0)
        {
            var nextPoint = GetLowestNeighbourAndDirection(heightMap, currentX, currentY, direction, speed, inertia);
            var nextX = nextPoint.x; var nextY = nextPoint.y; direction = nextPoint.dir;

            if (nextX == -1 && nextY == -1) 
            {
                return;
            }
            else // If he is his lowest neighbour, then he is a low minimum (lake bottom).
            if (nextX == currentX && nextY == currentY)
            {
                // Place sediment
                PlaceSediment(heightMap, currentX, currentY, sediment); 
                return;
            }

            // Update speed according to slope
            speed = speed * heightMap[currentX, currentY] - heightMap[nextX, nextY];

            // Calculate erosion
            float erodedAmmount = Mathf.Min(capacity - sediment, Mathf.Abs(heightMap[currentX, currentY] - heightMap[nextX, nextY])) * (UnityEngine.Random.Range(1,11)/10f);
            sediment += erodedAmmount;

            // Erode
            ErodeArea(heightMap, currentX, currentY, erodedAmmount);

            if (sediment > capacity)
            {
                // Place overflowing of sediment
                //heightMap[currentX, currentY] -= sediment - capacity;
                PlaceSediment(heightMap, currentX, currentY, sediment - capacity);
                sediment -= (sediment - capacity);
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

        int xBrush = 0, yBrush;

        // We're creating a square around the point and try to spread the given sediment accordingly
        for (int x = Mathf.Max(xPos - 1, 0); x <= Mathf.Min(xPos + 1, width); x++)
        {
            yBrush = 0;
            for (int y = Mathf.Max(yPos - 1, 0); y <= Mathf.Min(yPos + 1, height); y++)
            {
                noiseMap[x, y] -= erodedAmmount * erosionMask[xBrush, yBrush];
                yBrush++;
            }
            xBrush++;
        }
    }

    private static void PlaceSediment(float[,] noiseMap, int xPos, int yPos, float sedimentAmmount)
    {
        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        int xBrush = 0, yBrush;

        // We're creating a square around the point and try to spread the given sediment accordingly
        for (int x = Mathf.Max(xPos - radius, 0); x <= Mathf.Min(xPos + radius, width); x++)
        {
            yBrush = 0;
            for (int y = Mathf.Max(yPos - radius, 0); y <= Mathf.Min(yPos + radius, height); y++)
            {
                noiseMap[x, y] += sedimentAmmount * sedimentPlacementMask[xBrush, yBrush];
                yBrush++;
            }
            xBrush++;
        }
    }

    private static (int x, int y, int dir) GetLowestNeighbourAndDirection(float[,] noiseMap, int xPos, int yPos, int previousDirection, float speed, float inertia)
    {
        
        if (previousDirection < -1 || previousDirection > 7) throw new ArgumentException(previousDirection.ToString());

        // it's the first time the droplets start to row, it should not have a predetermined direction. We should assign it a random one.
        if(previousDirection == -1) 
        {
            previousDirection = UnityEngine.Random.Range(0,8);
        }

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
                if (!(xPos==x && yPos==y)) // that means except for the center
                {
                    // the point on the direction that the droplet was already flowing towards
                    // has a little advantage to be picked.
                    if (previousDirection == localDirection)
                    {
                        // (weight * inertia) if weight is at full capacity then inertia acts
                        // to its maximum. if weight is zero, then inertia doesn't act at all.
                        if (noiseMap[x, y] - noiseMap[x, y] * inertia * speed < minValue)
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
                else
                {
                    
                }
            }
        }

        return (minXPos, minYPos, nextDirection);
    }

    static float[,] GenerateMatrix(int size, float centerHardness)
    {

        size = size * 2 + 1;

        float[,] matrix = new float[size, size];

        // Calculate the middle value
        int mid = size / 2;
        float distance;
        // Assign the values to the matrix
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {

                if (i == mid && j == mid) 
                { 
                    distance = Mathf.Sqrt(Mathf.Pow((mid - i), 2) + Mathf.Pow((mid - j+1), 2))/ centerHardness; 
                }
                else
                {
                    distance = Mathf.Sqrt(Mathf.Pow((mid - i), 2) + Mathf.Pow((mid - j), 2));
                }
                    matrix[i, j] = 1 / distance; // reverse de result
            }
        }

        // Normalize the matrix so the sum of all elements equals 1
        float sum = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                sum += matrix[i, j];
            }
        }

        // Normalize the matrix
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                matrix[i, j] /= sum;
            }
        }

        string str = "";
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                str += matrix[i, j] + " ";
            }
            str += "\n";
        }

        Debug.Log("matr\n" + str);

        return matrix;
    }

}
