
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleTerrainGen : MonoBehaviour
{   
    public GameObject terrainToSpawn;
    public GameObject  villageHouse;
    private MeshCollider terrainCollider;
    public float maxHeight;
    public float minHeight;
    public float maxSlope;
    public float minSlope;
    private RaycastHit hit;
    private List<SphereCollider> scList = new List<SphereCollider>();


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
        int numberOfObjects = 3;
        Vector3 terrainSize = spawned.GetComponentInChildren<MeshRenderer>().bounds.size;
        Vector3 position;
        Mesh terrainMesh = spawned.GetComponent<MeshCollider>().sharedMesh;
        
        
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Generate a random position within the terrain's bounds
                position = new Vector3(Random.Range(0,terrainSize.x), 1000, Random.Range(0,terrainSize.z));
                print("Finding position" + position);

                if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
                {
                    position.y = hit.point.y;
                    // int randomIndex = Random.Range(0, objectsToSpawn.Length); // set default object
                    // GameObject objToSpawn = objectsToSpawn[randomIndex];
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    if((Mathf.Abs(hit.normal.x) > minSlope && Mathf.Abs(hit.normal.x) < maxSlope) && (Mathf.Abs(hit.normal.z) > minSlope && Mathf.Abs(hit.normal.z) < maxSlope)){
                        print(rotation);
                        print("Spawning object at " + position);
                        // Spawn the object at the hit point
                        GameObject villageCenter = Instantiate(villageHouse, new Vector3(position.x,position.y - 1.5f,position.z), rotation);
                        SphereCollider villageCenterCollider = villageCenter.AddComponent<SphereCollider>();
                        villageCenterCollider.radius = 150f;
                        scList.Add(villageCenterCollider);
                    }else{
                        numberOfObjects++;
                    }
                }else{
                    numberOfObjects++;
                }
        }

        // /////////////////////////////////////////////////////////////////////////////////////////
        // // surrounding object spawn
        // /////////////////////////////////////////////////////////////////////////////////////////
        int surroundingBuildings = 5;
        for(int j = 0; j<scList.Count;j++){
            for(int i =0 ;i<=surroundingBuildings;i++){
                Vector3 randomPos = scList[j].bounds.center + new Vector3(Random.Range(-scList[j].bounds.extents.x, scList[j].bounds.extents.x), 
                                                            1000, Random.Range(-scList[j].bounds.extents.z, scList[j].bounds.extents.z));
                if (Physics.Raycast(randomPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight)){
                    randomPos.y = hit.point.y;
                    Vector3 center = scList[j].bounds.center;
                    Vector3 direction = center - randomPos;
                    Quaternion rotationCenter = Quaternion.LookRotation(direction);
                    Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    Quaternion finalRotation = rotationNormal * rotationCenter;
                    if((Mathf.Abs(hit.normal.x) > minSlope && Mathf.Abs(hit.normal.x) < maxSlope) && (Mathf.Abs(hit.normal.z) > minSlope && Mathf.Abs(hit.normal.z) < maxSlope)){
                        randomPos = new Vector3(randomPos.x,randomPos.y - 1.5f,randomPos.z);
                        GameObject surroundingHouses = Instantiate(villageHouse, randomPos, finalRotation);
                        SphereCollider houseCollider = surroundingHouses.AddComponent<SphereCollider>();
                        houseCollider.radius = 10f;
                        Collider[] surroundingColliders = Physics.OverlapSphere(randomPos, houseCollider.radius, LayerMask.GetMask("House"));
                        print(scList[j] +"--"+i+"--"+surroundingColliders[0].gameObject.name+" "+surroundingColliders.Length);
                    // if(surroundingColliders.Length > 1){
                    //     Destroy(surroundingHouses);
                    //     surroundingBuildings++;
                    //     print("Destroying a house :|");
                    // }
                    }else{
                        surroundingBuildings++;
                    }
                }else{
                    surroundingBuildings++;
                }
            }
        }

    }
}