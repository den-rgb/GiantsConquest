using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainColour : MonoBehaviour
{
    public Material material;
    public Gradient gradient;

    private TerrainLayer terrainLayer;
    private Texture2D texture;

    

    public Color[] GenerateColors(float[,] noiseMap, Terrain _terrain)
    {
        gradient = GameObject.Find("Mesh2Terrain").GetComponent<TerrainColour>().gradient;
        if (_terrain == null)
        {
            _terrain = GetComponent<Terrain>();
        }

        int width = _terrain.terrainData.heightmapResolution;
        int height = _terrain.terrainData.heightmapResolution;

        List<Color> colorMap = new List<Color>();

        for (int y = 0; y < height ; y++)
        {
            for (int x = 0; x < width ; x++)
            {
                if (x == width - 1 || y == height - 1) continue;
                float heightValue = noiseMap[x, y];
                colorMap.Add(gradient.Evaluate(heightValue));
            }
        }

        return colorMap.ToArray();
    }


    public void DisplayTerrain(float[,] noiseMap, Terrain _terrain, Gradient gradient)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        // Generate the color map and apply it to the texture
        Color[] colorMap = GenerateColors(noiseMap, _terrain);
        if (texture == null)
        {
            texture = new Texture2D(width - 1, height - 1);
            texture.filterMode = FilterMode.Point;
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        // Apply the texture to the terrain
        if (terrainLayer == null)
        {
            terrainLayer = new TerrainLayer();
        }
        terrainLayer.diffuseTexture = texture;
        
        TerrainData terrainData = _terrain.terrainData;
        terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
        
        float[,,] splatmaps = new float[width, height, 1];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                splatmaps[x, y, 0] = 1.0f;
            }
        }
        
        // Normalize the splatmap values to ensure they add up to 1.0 for each point
        for (int i = 0; i < terrainData.alphamapLayers; i++)
        {
            float[,,] alphamap = terrainData.GetAlphamaps(0, 0, width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float total = alphamap[x, y, i] + splatmaps[x, y, 0];
                    if (total > 1.0f)
                    {
                        alphamap[x, y, i] /= total;
                        splatmaps[x, y, 0] /= total;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, alphamap);
        }
        terrainData.SetAlphamaps(0, 0, splatmaps);

        // Update the terrain object
        _terrain.Flush();
    }
}
