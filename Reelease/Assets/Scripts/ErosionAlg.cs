using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using static MapController;

public static class ErosionAlg
{
    public static float RiverStartMinHeight = 0.8f, RiverStartMaxHeight = 0.9f;
    public static float RiverDepth = 0.2f;
    public static float FillingCapacity = 0.1f;

    public static Collection<Collection<Point>> GetRivers(float[,] noiseMap, int riversCount, HeightInterval heightInterval)
    {
        if (riversCount == 0) { return null; }

        RiverStartMinHeight = heightInterval.lowerLimit;
        RiverStartMaxHeight = heightInterval.upperLimit;

        // Find possible starting point based on the height interval in which rivers start.
        var startPoints = getBetween(noiseMap, RiverStartMinHeight, RiverStartMaxHeight);

        // We will make the given number of rivers if the number of starting points allow it.
        // If not, then we make as many rivers as we can.
        var startPointsCount = startPoints.Count;
        var riversList = new Collection<Collection<Point>>();
        if (startPointsCount > 0)
        {

            for(int i = 0; i < Mathf.Min(riversCount, startPointsCount); i++) 
            {
                var chosenPointPos = UnityEngine.Random.Range(0, startPoints.Count);
                var chosenPoint = startPoints.ElementAt(chosenPointPos);
                startPoints.RemoveAt(chosenPointPos);

                // I'm curious if I write the below 3 lines in a single line, would it have the same effect?
                Collection<Point> riverPoints = new Collection<Point>();
                riverPoints = findRiverPath(noiseMap, chosenPoint, riverPoints);
                riversList.Add(riverPoints);
            }
        }

        return riversList;

    }


    private static Collection<Point> findRiverPath(float[,] noiseMap, Point currentPoint, Collection<Point> traveled)
    {
        while (true)
        {
            var nextPoint = getLowestNeighbour(noiseMap, currentPoint);
            if (nextPoint.x == currentPoint.x && nextPoint.y == currentPoint.y)
            {
                // Set a capacity for filling an area.
                var fillingCapacity = noiseMap[currentPoint.x, currentPoint.y] + FillingCapacity;
                
                // After finding a capacity for filling, starts filling up until that height.
                traveled = fill(noiseMap, currentPoint, fillingCapacity, traveled);
                return traveled;
            }
            else
            {
                traveled.Add(currentPoint);
            }
            currentPoint = nextPoint;
        }
    }

    // Only fill the points which would flow towards the same point. If you meet a point that flows towards
    // another point, then stop at that height.

    // Breath first search from the end of the river.
    private static Collection<Point> fill(float[,] noiseMap, Point point, float fillingCapacity, Collection<Point> traveled)
    {
        var toFillPoints = new Queue<Point>();
        toFillPoints.Enqueue(point);

        int pointsFilled = 0;

        while(toFillPoints.Count > 0)
        {
            var nextPoint = toFillPoints.Dequeue();
            if (!traveled.Contains(nextPoint))
            {
                traveled.Add(nextPoint);
                var fittingPoints = getUntraveledNeighboursLowerThan(noiseMap, nextPoint, fillingCapacity, traveled);
                
                foreach (var toEnqueuePoint in fittingPoints)
                {
                    // Add point to the river points collection.
                    toFillPoints.Enqueue(toEnqueuePoint);
                    pointsFilled++;
                }

            }
        }

        Debug.Log("Filled " + pointsFilled + " points.");
        return traveled;

    }

    private static Collection<Point> getUntraveledNeighboursLowerThan(float[,] noiseMap, Point point, float fillingCapacity, Collection<Point> traveled)
    {
        var fittingNeighbours = new Collection<Point>();

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        for (int x = Mathf.Max(point.x - 1, 0); x <= Mathf.Min(point.x + 1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (point.x==x && point.y ==y) continue;
                if (!traveled.Contains(new Point(x, y)) && noiseMap[x, y] < fillingCapacity) 
                    fittingNeighbours.Add(new Point(x, y));
            }
        }
        return fittingNeighbours;
    }

    // Does it have a neighbour which is smaller than him?
    // If so, then it can leak through there.
    private static bool isLeaking(float[,] noiseMap, Point currentPoint)
    {
        var foundPoint = getLowestNeighbour(noiseMap, currentPoint);
        if(foundPoint.x == currentPoint.x && foundPoint.y == currentPoint.y) { return true; }
        else { return false; }
    }

    // Returns the point with the lowest height from: 
    //  [x-1,y-1]   [x-1,y]   [x-1,y+1]
    //   [x,y-1]   { [x,y] }   [x,y+1]
    //  [x+1,y-1]   [x+1,y]   [x+1,y+1]
    private static Point getLowestNeighbour(float[,] noiseMap, Point point)
    {
        Point minPoint = new Point();
        float minValue = float.MaxValue;
        
        var width = noiseMap.GetLength(0)-1;
        var height = noiseMap.GetLength(1)-1;

        // Debug.Log("Looking for neighbour for point: " + point.x + ", " + point.y);
        // Debug.Log("Square corners: " + Mathf.Max(point.x - 1, 0) + ", " + Mathf.Min(point.x + 1, width) + ", " + Mathf.Max(point.y - 1, 0) + ", " + Mathf.Min(point.y + 1, height));

        for(int x = Mathf.Max(point.x-1, 0); x <= Mathf.Min(point.x+1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (point.x == x && point.y == y) continue;
                // Debug.Log("Trying to access: " + x + ", " + y);
                if (noiseMap[x, y]<minValue)
                {
                    minValue = noiseMap[x, y];
                    minPoint.x = x;
                    minPoint.y = y;
                }
            }
        }

        // Debug.Log("Found smaller neighbour with height: " + minValue + " and middle point's height is: " + noiseMap[point.x, point.y]);
        
        // check if the found minim is smaller than our point
        if (minValue < noiseMap[point.x, point.y])
        {
            return minPoint;
        }
        else
            return point;
    }

    private static Point getHighestNeighbour(float[,] noiseMap, Point point)
    {
        Point maxPoint = new Point();
        float maxValue = float.MinValue;

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        for (int x = Mathf.Max(point.x - 1, 0); x <= Mathf.Min(point.x + 1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (point.x == x && point.y == y) continue;
                // Debug.Log("Trying to access: " + x + ", " + y);
                if (noiseMap[x, y] > maxValue)
                {
                    maxValue = noiseMap[x, y];
                    maxPoint.x = x;
                    maxPoint.y = y;
                }
            }
        }

        if (maxValue < noiseMap[point.x, point.y])
        {
            return maxPoint;
        }
        else
            return point;
    }

    private static Collection<Point> getNeighbours(float[,] noiseMap, Point point)
    {
        var neighbours = new Collection<Point>();

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        for (int x = Mathf.Max(point.x - 1, 0); x <= Mathf.Min(point.x + 1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (point.x == x && point.y == y) continue;
                neighbours.Add(new Point(x, y));
            }
        }

        return neighbours;
    }

    private static Collection<Point> arrangeNeighboursClockwise(Collection<Point> neighbours)
    {
        if (neighbours.Count != 8) throw new ArgumentException("Should have exaclty 8 points.");

        var result = new Collection<Point>
        {
            neighbours.ElementAt(0),
            neighbours.ElementAt(1),
            neighbours.ElementAt(2),
            neighbours.ElementAt(4),
            neighbours.ElementAt(7),
            neighbours.ElementAt(6),
            neighbours.ElementAt(5),
            neighbours.ElementAt(3)
        };

        return result;
    }

    private static Collection<Point> getLowerNeighbours(float[,] noiseMap, Point point)
    {
        var lowerPoints = new Collection<Point>();
        float currentHeightValue = noiseMap[point.x, point.y];

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        for (int x = Mathf.Max(point.x - 1, 0); x <= Mathf.Min(point.x + 1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (point.x == x && point.y == y) continue;

                if (noiseMap[x, y] < currentHeightValue)
                {
                    lowerPoints.Add(new Point(x, y));
                }
            }
        }

        return lowerPoints;
    }

    private static Collection<Point> getBetween(float[,] noiseMap, float low, float high)
    {
        var result = new Collection<Point>();
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                if (noiseMap[x, y] >= low && noiseMap[x, y] <= high)

                    result.Add(new Point() { x = x, y = y });
            }
        }

        return result;
    }

    internal static Collection<Point> GetLocalMinPoints(float[,] noiseMap)
    {
        var result = new Collection<Point>();
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                if(getLowerNeighbours(noiseMap, new Point(x, y)).Count==0)

                    result.Add(new Point() { x = x, y = y });
            }
        }
        return result;
    }

    internal static Collection<Point> GetPassagePoints(float[,] noiseMap)
    {
        var result = new Collection<Point>();
        for (int x = 1; x < noiseMap.GetLength(0)-1; x++)
        {
            for (int y = 1; y < noiseMap.GetLength(1)-1; y++)
            {
                var arrangedNeighbours = arrangeNeighboursClockwise(getNeighbours(noiseMap, new Point(x, y)));

                if (isPassage(noiseMap, arrangedNeighbours, noiseMap[x, y]))
                {
                    Debug.Log("Found a passage.");
                    result.Add(new Point(x, y));
                }
            }
        }
        return result;
    }

    private static bool isPassage(float[,] noiseMap, Collection<Point> border, float center)
    {
        var signChanges = 0;
        if ((noiseMap[border.First().x, border.First().y] > center &&
            noiseMap[border.Last().x, border.Last().y] < center) ||
            (noiseMap[border.First().x, border.First().y] < center &&
            noiseMap[border.Last().x, border.Last().y] > center)) { signChanges++; }

        for(int i=1; i<border.Count; i++)
        {
            if ((noiseMap[border.ElementAt(i).x, border.ElementAt(i).y] > center &&
            noiseMap[border.ElementAt(i-1).x, border.ElementAt(i-1).y] < center) ||
            (noiseMap[border.ElementAt(i).x, border.ElementAt(i).y] < center &&
            noiseMap[border.ElementAt(i-1).x, border.ElementAt(i-1).y] > center)) 
            {
                signChanges++; 
            }
        }

        return signChanges >= 4;

    }
}
public class Point
{
    public Point() { }
    public Point(int x, int y) { this.x = x; this.y = y; }

    public int x;
    public int y;

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Point otherPoint = (Point)obj;
        return (x == otherPoint.x) && (y == otherPoint.y);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            return hash;
        }
    }
}
