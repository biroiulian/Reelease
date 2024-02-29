using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static MapController;

public static class RiverLakeAlg
{
    public static float RiverStartMinHeight = 0.8f, RiverStartMaxHeight = 0.9f;
    public static float RiverDepth = 0.2f;
    public static float FillingCapacity = 0.2f;

    public static Collection<Point> GetRivers(float[,] noiseMap, int riversCount, HeightInterval heightInterval)
    {
        if (riversCount == 0) { return null; }

        RiverStartMinHeight = heightInterval.lowerLimit;
        RiverStartMaxHeight = heightInterval.upperLimit;

        // Find possible starting point based on the height interval in which rivers start.
        var startPoints = getBetween(noiseMap, RiverStartMinHeight, RiverStartMaxHeight);

        // We will make the given number of rivers if the number of starting points allow it.
        // If not, then we make as many rivers as we can.
        var startPointsCount = startPoints.Count;
        var riversList = new Collection<Point>();
        if (startPointsCount > 0)
        {

            for(int i = 0; i < Mathf.Min(riversCount, startPointsCount); i++) 
            {
                var chosenPointPos = UnityEngine.Random.Range(0, startPoints.Count);
                var chosenPoint = startPoints.ElementAt(chosenPointPos);
                startPoints.RemoveAt(chosenPointPos);

                // I'm curious if I write the below 3 lines in a single line, would it have the same effect?
                Collection<Point> riverPoints = new Collection<Point>();
                var globallyFilledPoints = new Collection<Point>();
                riverPoints = findRiverPath(noiseMap, chosenPoint, riverPoints, globallyFilledPoints);
                riversList.AddRange(riverPoints);
            }
        }
        
        return riversList;

    }

    public static Collection<Point> GetRivers(float[,] noiseMap, int riversCount, HeightInterval heightInterval, Collection<Point> startPoints)
    {
        if (riversCount == 0) { return null; }

        RiverStartMinHeight = heightInterval.lowerLimit;
        RiverStartMaxHeight = heightInterval.upperLimit;
        
        var riversList = new Collection<Point>();

        foreach(var p in startPoints)
        { 
            // I'm curious if I write the below 3 lines in a single line, would it have the same effect?
            Collection<Point> riverPoints = new Collection<Point>();
            var globallyFilledPoints = new Collection<Point>();
            riversList.AddRange(findRiverPath(noiseMap, p, riverPoints, globallyFilledPoints));
        }
        return riversList;

    }

    private static Collection<Point> findRiverPath(float[,] noiseMap, Point currentPoint, Collection<Point> traveled, Collection<Point> globallyFilledPoints)
    {
        Debug.Log("Find River path from " + currentPoint.x  + " " + currentPoint.y);
        while (true)
        {
            var nextPoint = getLowestNeighbour(noiseMap, currentPoint);
            // If he is his lowest neighbour, then he is a low minimum (lake bottom).
            if (nextPoint.x == currentPoint.x && nextPoint.y == currentPoint.y)
            {
                // Set a capacity for filling an area.
                var fillingCap = noiseMap[currentPoint.x, currentPoint.y] + FillingCapacity;
                
                // After finding a capacity for filling, starts filling up until that height.
                return fill(noiseMap, currentPoint, fillingCap, traveled, globallyFilledPoints);
            }
            else
            {
                traveled.Add(currentPoint);
            }
            currentPoint = nextPoint;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="noiseMap"></param>
    /// <param name="currentPoint"> The point to check.</param>
    /// <param name="lakeBottom"> The point to flow towards.</param>
    /// <returns>
    /// <see cref="true"/> if the <see cref="tryingPoint"/> flows towards <see cref="lakeBottom"/>.
    /// <see cref="false"/> otherwise.
    /// </returns>
    private static bool PointFlowsTowards(float[,] noiseMap, Point currentPoint, Point lakeBottom, Collection<Point> globallyFilledPoints)
    {
        if (currentPoint.x == lakeBottom.x && currentPoint.y == lakeBottom.y) 
        {
            return true;
        }

        while (true)
        {
            var nextPoint = getLowestNeighbour(noiseMap, currentPoint);
            // If the lowest neighbour is the lake bottom, its clear it flows there.
            if(nextPoint.x==currentPoint.x && nextPoint.y == currentPoint.y)
            {
                if (nextPoint.x == lakeBottom.x && nextPoint.y == lakeBottom.y)
                {
                    return true;
                }
                else if(globallyFilledPoints.Where(p => p.x == nextPoint.x && p.y == nextPoint.y).Count()>0)
                {
                    return true;
                }
                else return false;
            }

            currentPoint = nextPoint;
        }
    }

    private static Collection<Point> fill(float[,] noiseMap, Point start, float fillingTreshold, Collection<Point> traveled, Collection<Point> globallyFilledPoints)
    {
        Debug.Log("Fill from " + start.x + " " + start.y);
        var lakeBottom = start;
        var travellingQueue = new Queue<Point>();
        travellingQueue.Enqueue(start);

        var localTraveled = new Collection<Point>();
        var localOutflowPoints = new Collection<Point>();

        while (travellingQueue.Count > 0)
        {
            var nextPoint = travellingQueue.Dequeue();

            var neighboursToAdd = getFillableNeighbours(noiseMap, nextPoint, lakeBottom, fillingTreshold, localTraveled, globallyFilledPoints);

            // This should only return outflow points. I should check this if it actually does that.
            var neighboursThatDontFlowToTheSameLake = getUnfillableNeighbours(noiseMap, nextPoint, lakeBottom, fillingTreshold, localTraveled, globallyFilledPoints);
            
            // We got the point's good neighbours (that belong to this lake)
            // We added them to the travellig queue so they will be travelled next
            // We added them to the traveled points so that they don't get
            foreach ( var p in neighboursToAdd)
            {
                travellingQueue.Enqueue(p);
                localTraveled.Add(p);
            }

            localOutflowPoints.AddRange(neighboursThatDontFlowToTheSameLake);
        }

        float min = float.MaxValue;
        var pointToRunNextRiverFrom = new Point(-1,-1);
        foreach( var p in localOutflowPoints)
        {
            if (noiseMap[p.x, p.y] < min)
            {
                min = noiseMap[p.x, p.y];
                pointToRunNextRiverFrom.x = p.x;
                pointToRunNextRiverFrom.y = p.y;
            }
        }

        // This is where we come back to check if we put water in any point that might be below our flowout point.
        var localTraveledUnderLeakPoint =  localTraveled.Where(p => noiseMap[p.x, p.y] < min);

        // Add the local points that should consist of the lake to the global river-lake constuct (traveled).
        traveled.AddRange(localTraveledUnderLeakPoint);

        globallyFilledPoints.Add(start);

        if (pointToRunNextRiverFrom.x != -1)
        {
            return findRiverPath(noiseMap, pointToRunNextRiverFrom, traveled, globallyFilledPoints);
        }

        return traveled;
        
    }


    private static Collection<Point> getUnfillableNeighbours(float[,] noiseMap, Point point, Point lakeBottom, float fillingCapacity, Collection<Point> traveled, Collection<Point> globallyFilledPoints)
    {
        var unfittingNeighbours = new Collection<Point>();

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        for (int x = Mathf.Max(point.x - 1, 0); x <= Mathf.Min(point.x + 1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (point.x == x && point.y == y) continue;
                var tryPoint = new Point(x, y);
                if (!traveled.Contains(tryPoint) &&
                    noiseMap[x, y] <= fillingCapacity &&
                    !PointFlowsTowards(noiseMap, tryPoint, lakeBottom, globallyFilledPoints)
                    )
                    unfittingNeighbours.Add(tryPoint);
            }
        }
        return unfittingNeighbours;
    }

    /// <summary>
    /// Returns the neighbours of this point that:
    /// 1. Have a height lower than <see cref="fillingCapacity"/>
    /// 2. When applied the river flowing alghoritm, the final point
    /// towards which they flow to, is the same as <see cref="lakeBottom"/>
    /// </summary>
    private static Collection<Point> getFillableNeighbours(float[,] noiseMap, Point point, Point lakeBottom, float fillingCapacity, Collection<Point> traveled, Collection<Point> globallyFilledPoints)
    {
        // just some extra validation
        if (fillingCapacity <= noiseMap[lakeBottom.x, lakeBottom.y])
        {
            Debug.LogException(new Exception("Invalid value for filling capacity: " + 
                fillingCapacity + " when the bottom of the lake is: " + noiseMap[lakeBottom.x, lakeBottom.y]));
        }

        var fittingNeighbours = new Collection<Point>();

        var width = noiseMap.GetLength(0) - 1;
        var height = noiseMap.GetLength(1) - 1;

        for (int x = Mathf.Max(point.x - 1, 0); x <= Mathf.Min(point.x + 1, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                // if it's the source, skip
                if (point.x==x && point.y == y) continue;
                var tryPoint = new Point(x, y);
                if (!traveled.Contains(tryPoint) && 
                    noiseMap[tryPoint.x, tryPoint.y] <= fillingCapacity &&
                    PointFlowsTowards(noiseMap, tryPoint, lakeBottom, globallyFilledPoints)
                    ) 
                    fittingNeighbours.Add(tryPoint);
            }
        }

        return fittingNeighbours;
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

        for(int x = Mathf.Max(point.x-1, 0); x <= Mathf.Min(point.x, width); x++)
        {
            for (int y = Mathf.Max(point.y - 1, 0); y <= Mathf.Min(point.y + 1, height); y++)
            {
                if (noiseMap[x, y]<minValue)
                {
                    minValue = noiseMap[x, y];
                    minPoint.x = x;
                    minPoint.y = y;
                }
            }
        }
        return minPoint;
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

        return signChanges > 3;

    }
}

[Serializable]
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
