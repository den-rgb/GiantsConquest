using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainColour : MonoBehaviour
{
    public Material material;
    public Gradient gradient;

    private TerrainLayer terrainLayer;
    private Texture2D texture;

    

    public Color[] GenerateColors(float[,] noiseMap, Gradient gradient)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        List<Color> colorMap = new List<Color>();
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Find the minimum and maximum values in the noise map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (noiseMap[x, y] < minValue)
                {
                    minValue = noiseMap[x, y];
                }
                if (noiseMap[x, y] > maxValue)
                {
                    maxValue = noiseMap[x, y];
                }
            }
        }

        // Generate colors for each point based on its height value
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float heightValue = noiseMap[x, y];
                colorMap.Add(gradient.Evaluate((heightValue - minValue) / (maxValue - minValue)));
            }
        }

        return colorMap.ToArray();
    }



   public void DisplayTerrain(float[,] noiseMap, Gradient gradient, Terrain _terrain)
    {
        Color[] colourMap = GenerateColors(noiseMap, gradient);

        // Create a texture from the color map
        Texture2D texture = new Texture2D(noiseMap.GetLength(0), noiseMap.GetLength(1));
        for (int y = 0; y < noiseMap.GetLength(1); y++)
        {
            for (int x = 0; x < noiseMap.GetLength(0); x++)
            {
                texture.SetPixel(x, y, colourMap[y * noiseMap.GetLength(0) + x]);
            }
        }
        texture.Apply();

        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Glossiness", 0.0f);
        _terrain.materialTemplate = mat;
        // Apply the texture to the terrain's material
        _terrain.materialTemplate.mainTexture = texture;
    }

}
