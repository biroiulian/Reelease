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
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

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

    public void Draw3D(MeshData meshData)
    {
        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshFilter.sharedMesh.RecalculateNormals();
        meshCollider.sharedMesh.Clear();
        meshCollider.sharedMesh = meshData.CreateMesh();
        meshCollider.sharedMesh.RecalculateNormals();

        // The part that handles scaling. We need this to keep our mesh inside the screen.
        var xRenderScale = meshRenderer.transform.localScale.x;
        meshRenderer.transform.localScale = new Vector3(xRenderScale, 1, xRenderScale);
        
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
}
