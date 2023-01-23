
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTerrainGen : MonoBehaviour
{   
    public GameObject terrainToSpawn;
    public GameObject[] objectsToSpawn;
    private MeshCollider terrainCollider;
    public float maxHeight;
    public float minHeight;
    private RaycastHit hit;
    public void Start()
    {
        GameObject spawned = Instantiate(terrainToSpawn,new Vector3(0,0,0), Quaternion.identity);
        MapGenerator script = spawned.GetComponent<MapGenerator>();
        float height = Random.Range(150, 300);
        float noise = Random.Range(1700, 2100);
        float noise_2 = Random.Range(3600, 4500);
        int rand = Random.Range(1, 2);
        if(rand == 1)
        {
            script.noiseData.noiseScale = noise;
        }
        else
        {
            script.noiseData.noiseScale = noise_2;
        }
        int oct = 4;
        float lac = Random.Range(3.0f, 3.5f);
        script.noiseData.meshHeightMultiplier = height;
        script.noiseData.octaves = oct;
        script.noiseData.lacunarity = lac;
        script.generateWater = true;
        script.GenerateRandomSeed();
        script.GenerateMap();
        spawned.transform.DetachChildren();
        WaterGenerator waterScript = FindObjectOfType<WaterGenerator>();
        GameObject water = waterScript.gameObject;
        water.transform.position = new Vector3(0, -150, 0);
        
        /// /////////////////////////////////////////////////////////////////////////////////////////
        /// object spawn 
        ///////////////////////////////////////////////////////////////////////////////////////////// 
        
        terrainCollider = spawned.GetComponent<MeshCollider>();
        int numberOfObjects = objectsToSpawn.Length;
        Vector3 terrainSize = spawned.GetComponentInChildren<MeshRenderer>().bounds.size;
        Vector3 position;
        Mesh terrainMesh = spawned.GetComponent<MeshCollider>().sharedMesh;
        
        
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Generate a random position within the terrain's bounds
            
            do
            {
                position = new Vector3(Random.Range(0,terrainSize.x), 1000, Random.Range(0,terrainSize.z));
                print("Finding position" + position);

                if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                {
                    position.y = hit.point.y;
                    int randomIndex = Random.Range(0, objectsToSpawn.Length); // set default object
                    GameObject objToSpawn = objectsToSpawn[randomIndex];
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    print(rotation);
                    print("Spawning object at " + position);
                    // Spawn the object at the hit point
                    Instantiate(objToSpawn, new Vector3(position.x,position.y - 1.5f,position.z), rotation);
                }
            } while (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && hit.point.y > minHeight && hit.point.y < maxHeight);
        }
    }
}