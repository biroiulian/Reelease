using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public struct WaterPlane
{
    public int Size;
    public int xPos;
    public int yPos;
    public int zPos;
}

public class WaterPlaneGenerator : MonoBehaviour
{
    [SerializeField]
    public WaterPlane[] WaterPlanes;
    [SerializeField]
    public int ScreenWidth;
    [SerializeField]
    public int ScreenHeight;

    public void GenerateWater()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();

        foreach (WaterPlane waterPlane in WaterPlanes)
        {
            display.Draw3D(MeshGenerator.GenerateWaterMesh(waterPlane.Size, waterPlane.Size, ScreenWidth*4, ScreenHeight*4, waterPlane.xPos, waterPlane.zPos, waterPlane.yPos));
        }

    }
}
