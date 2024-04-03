using UnityEngine;

public class NoiseMapDisplay2D : MonoBehaviour
{

    public Renderer erosionTextureRenderer;
    public Renderer noErosionTextureRenderer;
    public Renderer soilTextureRenderer;

    public void DrawNoiseMaps(double[,] erosionMap, double[,] noErosionMap, double[,] soilMap)
    {
        DrawNoiseMap(erosionMap, erosionTextureRenderer);

        if (noErosionMap != null)
        {
            DrawNoiseMap(noErosionMap, noErosionTextureRenderer);
        }

        if (soilMap != null)
        {
            DrawNoiseMap(soilMap, soilTextureRenderer);
        }
    }

    private void DrawNoiseMap(double[,] erosionMap, Renderer renderer)
    {
        int width = erosionMap.GetLength(0);
        int height = erosionMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, (float)erosionMap[x, y]);
            }
        }

        Color[] colourMapFlipped = new Color[width * height];
        int counter = 0;
        for (int i = width * height - 1; i >= 0; i--)
        {
            colourMapFlipped[counter] = colourMap[i];
            counter++;
        }

        texture.SetPixels(colourMapFlipped);
        texture.Apply();

        renderer.sharedMaterial.mainTexture = texture;
    }
}
