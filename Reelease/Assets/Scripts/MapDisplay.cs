using UnityEngine;
using System.Collections;
using System.Linq;
using static MapController;
using Unity.VisualScripting;
using System.Collections.ObjectModel;
using System;
using static UnityEngine.Mesh;
using System.Collections.Generic;
using System.Xml;

public class MapDisplay : MonoBehaviour
{
    private ColorFloatPair[] ColorsWithRatios;
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public Color[] GetColorMap(float[,] noiseMap, ColorFloatPair[] colorsWithRatios, ColorMode drawMode, Dictionary<Color, Collection<Point>> additionalPoints = null)
    {
        ColorsWithRatios = colorsWithRatios;

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                switch (drawMode)
                {
                    case ColorMode.NoiseMap:
                        colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                        break;
                    case ColorMode.LerpedColorMap:
                        colorMap[y * width + x] = getLerpedColorForHeight(noiseMap[x, y]);
                        break;
                    case ColorMode.SharpColorMap:
                        colorMap[y * width + x] = getColorForHeight(noiseMap[x, y]);
                        break;
                }
            }
        }

        if(additionalPoints != null)
        {
            foreach (KeyValuePair<Color, Collection<Point>> entry in additionalPoints)
            {
                var color = entry.Key;
                foreach (var point in entry.Value)
                {
                    colorMap[point.y * width + point.x] = color;
                }
            }
        }


        return colorMap;
    }

    public void Draw2D(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colourMap);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;

        // The part that handles scaling. We need this to keep our texture inside the screen.
        var xRenderScale = textureRender.transform.localScale.x;
        textureRender.transform.localScale = new Vector3(xRenderScale, 1, xRenderScale);
    }

    public void Draw2D(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;

        // The part that handles scaling. We need this to keep our texture inside the screen.
        var xRenderScale = textureRender.transform.localScale.x;
        textureRender.transform.localScale = new Vector3(xRenderScale, 1, xRenderScale);
    }

    public void Draw3D(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;

        // The part that handles scaling. We need this to keep our mesh inside the screen.
        var xRenderScale = meshRenderer.transform.localScale.x;
        meshRenderer.transform.localScale = new Vector3(xRenderScale, 1, xRenderScale);

        //meshObject.GetComponent<MeshCollider>().transform.localScale = new Vector3(xRenderScale, 1, xRenderScale);
        // Draw2D(texture);
    }

    public Texture2D GetTexture(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    private Color getLerpedColorForHeight(float point)
    {
        var ratiosSum = ColorsWithRatios.Sum(x => x.ratio);
        var counterRatio = 0f;
        for (int i = 0; i < ColorsWithRatios.Length - 1; i++)
        {
            var currentColorsRatio = ColorsWithRatios[i].ratio / ratiosSum;
            counterRatio += currentColorsRatio;

            if (point < counterRatio)
            {
                var lerpedPoint = Mathf.InverseLerp(counterRatio - currentColorsRatio, counterRatio, point);
                return Color.Lerp(ColorsWithRatios[i].color, ColorsWithRatios[i + 1].color, lerpedPoint);
            }
        }

        return ColorsWithRatios.Last().color;
    }

    private Color getColorForHeight(float point)
    {
        var ratiosSum = ColorsWithRatios.Sum(x => x.ratio);
        var counterRatio = 0f;
        for (int i = 0; i < ColorsWithRatios.Length - 1; i++)
        {
            var currentColorsRatio = ColorsWithRatios[i].ratio / ratiosSum;
            counterRatio += currentColorsRatio;

            if (point < counterRatio)
            {
                return ColorsWithRatios[i].color;
            }
        }

        return ColorsWithRatios.Last().color;
    }
}
