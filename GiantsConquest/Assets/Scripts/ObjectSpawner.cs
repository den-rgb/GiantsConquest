using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectsToSpawn;
    private MeshCollider terrainCollider;
    public int numberOfObjects = 100;
    private GameObject terrain;
    private SingleTerrainGen terrGen;

    void Start()
    {
        terrGen = GetComponent<SingleTerrainGen>();
        print(terrGen);
        terrain = terrGen.terrainToSpawn;
        print(terrain);
        terrainCollider = terrain.GetComponent<MeshCollider>();
        print(terrainCollider);
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Generate a random position within the terrain's bounds
            Vector3 randomPos = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));

            // Raycast to get the point where the ray hit the terrain
            Ray ray = new Ray(randomPos + Vector3.up * 50, Vector3.down);
            RaycastHit hit;
            if (terrainCollider.Raycast(ray, out hit, 100))
            {
                // Get the terrain's normal at the hit point
                Vector3 normal = hit.normal;
                // Choose an object to spawn based on the terrain's slope
                GameObject objToSpawn = objectsToSpawn[0]; // set default object
                if (normal.y >= 0.5f)
                    objToSpawn = objectsToSpawn[1];
                else if (normal.y >= 0.3f)
                    objToSpawn = objectsToSpawn[2];
                else if (normal.y >= 0.1f)
                    objToSpawn = objectsToSpawn[3];

                // Spawn the object at the hit point
                Instantiate(objToSpawn, hit.point, Quaternion.identity);
            }
        }
    }
}


