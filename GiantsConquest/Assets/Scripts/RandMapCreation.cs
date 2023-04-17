using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandMapCreation : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject terrainToSpawn;
    public GameObject Grid;
    public WaterGenerator waterGenScript;

    [System.Obsolete]
    void Start()
    {
        Vector3 gridSize = Grid.GetComponent<MeshRenderer>().bounds.size;
        print(gridSize);
        Vector3 position;
        int mountainAmount = 20;
        int flatsAmount = 20;
        int waterCount = 0;
        for (int i = 0; i <= mountainAmount; i++)
        {
            do
            {
                position = new Vector3(
                Random.Range((-Grid.transform.position.x - gridSize.x) / 2, (Grid.transform.position.x + gridSize.x) / 2),
                0.5f,
                Random.Range((-Grid.transform.position.z - gridSize.z) / 2, (Grid.transform.position.z + gridSize.z) / 2));
                print("Finding position:" + position);
            } while (Physics.CheckSphere(position, 1.0f, LayerMask.GetMask("SpawnedTerrain")));

            GameObject spawned = Instantiate(terrainToSpawn, position, Quaternion.identity);
            MapGenerator script = spawned.GetComponent<MapGenerator>();
            if (waterCount == 0) { 
                script.generateWater = true;
            } else { 
                script.generateWater = false; 
            }
            float height = Random.Range(100,250);
            float noise = Random.Range(4000, 8000);
            int oct = Random.Range(4, 5);
            float lac = Random.Range(3.0f, 3.5f);
            script.noiseData.meshHeightMultiplier = height;
            script.noiseData.noiseScale = noise;
            script.noiseData.octaves = oct;
            script.noiseData.lacunarity = lac;
            script.GenerateRandomSeed();
            script.GenerateMap();
            if (waterCount == 0)
            {
                waterCount++;
                //GameObject water = spawned.transform.GetChild(0).gameObject;
                spawned.transform.DetachChildren();
                WaterGenerator waterScript = FindObjectOfType<WaterGenerator>();
                GameObject water = waterScript.gameObject;
                water.transform.localScale = new Vector3(50, 1, 50);
                Vector4 pos = Grid.transform.localToWorldMatrix.GetColumn(3);
                water.transform.position = new Vector3(pos.x -12000, pos.y, pos.z -12000);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(water.transform);
                Renderer waterRenderer = water.GetComponent<Renderer>();
                Bounds waterBounds = waterRenderer.bounds;
                Vector3 waterCenter = waterBounds.center;
                cube.transform.position = new Vector3(waterCenter.x, waterCenter.y - 1, waterCenter.z);
                cube.transform.localScale = new Vector3(500, 1, 500);

            }
            else
            {
                Destroy(spawned.transform.GetChild(0).gameObject);
            }
        }

        for (int i = 0; i <= flatsAmount; i++)
        {
            do
            {
                position = new Vector3(
                Random.Range((-Grid.transform.position.x - gridSize.x) / 2, (Grid.transform.position.x + gridSize.x) / 2),
                0.5f,
                Random.Range((-Grid.transform.position.z - gridSize.z) / 2, (Grid.transform.position.z + gridSize.z) / 2));
                print("Finding position:" + position);
            } while (Physics.CheckSphere(position, 1.0f, LayerMask.GetMask("SpawnedTerrain")));

            GameObject spawned = Instantiate(terrainToSpawn, position, Quaternion.identity);
            MapGenerator script = spawned.GetComponent<MapGenerator>();
            float height = Random.Range(50, 100);
            float noise = Random.Range(2000, 3000);
            int oct = Random.Range(4, 5);
            float lac = Random.Range(3.0f, 3.5f);
            script.noiseData.meshHeightMultiplier = height;
            script.noiseData.noiseScale = noise;
            script.noiseData.octaves = oct;
            script.noiseData.lacunarity = lac;
            script.GenerateRandomSeed();
            script.GenerateMap();
            Destroy(spawned.transform.GetChild(0).gameObject);
        }


        Destroy(Grid.gameObject);
    }
}
