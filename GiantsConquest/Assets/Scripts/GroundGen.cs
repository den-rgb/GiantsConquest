using UnityEngine;
using UnityEditor;

public class GroundGen : MonoBehaviour
{

    private Terrain terrain;
    public float frequency = 10f;
    public float octaves = 5f;
    public float amplitude = 5f;
    public float pixelError = 8f;
    public float baseMapDis = 1000f;
    void Start()
    {
        terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("LowPolyTerrain script must be attached to a terrain object");
            return;
        }

        // Set terrain size and resolution
        terrain.terrainData.size = new Vector3(1000, 600, 1000);
        terrain.terrainData.heightmapResolution = 129;

        // Generate random terrain heights
        float[,] heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
        for (int i = 0; i < terrain.terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < terrain.terrainData.heightmapResolution; j++)
            {
                float x = (float)i / terrain.terrainData.heightmapResolution * frequency;
                float y = (float)j / terrain.terrainData.heightmapResolution * frequency;
                heights[i, j] = Mathf.PerlinNoise(x, y) * octaves * amplitude;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);

        // Reduce the number of terrain polygons
        terrain.heightmapPixelError = pixelError;
        terrain.basemapDistance = baseMapDis;
    }
}