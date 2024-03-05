using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public static class MeshGenerator
{

    public static MeshData GenerateTerrainMesh(float[,] heightMap, float fixedWidth, float fixedHeight, int topLeftX, int topLeftZ, int distanceY, int heightMultiplicator)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        if (width != height) throw new ArgumentException("Height should be equal with width."); 


        float widthDistance = fixedWidth/(width-1);
        float heightDistance = fixedHeight/(height-1);
    
        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;
        Debug.Log("width mesh: " + width+ " height mesh " + height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(x * widthDistance == fixedWidth) { Debug.Log("It is w."); }
                if (y * heightDistance == fixedHeight) { Debug.Log("It is h."); }
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x * widthDistance, heightMap[x, y]*heightMultiplicator+ distanceY, topLeftZ - y*heightDistance);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;

    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        Debug.Log("Mesh: " + mesh.vertices.Count() + " tri: " + mesh.triangles.Count());
        return mesh;
    }

}