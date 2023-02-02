
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleTerrainGen : MonoBehaviour
{   
    public GameObject terrainToSpawn;
    public GameObject well;
    public GameObject  villageHouse;
    private MeshCollider terrainCollider;
    public float maxHeight;
    public float minHeight;
    public float maxSlope;
    public float minSlope;
    private RaycastHit hit;
    private List<SphereCollider> scList = new List<SphereCollider>();
    private List<Vector3> pathList = new List<Vector3>();
    public GameObject[] path;


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
        /// center spawn 
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
                if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
                {
                    position.y = hit.point.y;
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    Vector3 euler = rotation.eulerAngles;
                    //converting euler angles to degrees
                    euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                    euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
                    if(euler.x > minSlope && euler.x < maxSlope && euler.z > minSlope && euler.z < maxSlope){
                        // Spawn the object at the hit point
                        GameObject villageCenter = Instantiate(well, position, rotation);
                        villageCenter.name = "Well" + scList.Count.ToString();
                        SphereCollider villageCenterCollider = villageCenter.AddComponent<SphereCollider>();
                        villageCenterCollider.radius = 200f;
                        villageCenterCollider.isTrigger = true;
                        scList.Add(villageCenterCollider);
                    }else{
                        numberOfObjects++;
                    }
                }else{
                    numberOfObjects++;
                }
        }

        // /////////////////////////////////////////////////////////////////////////////////////////
        // // surrounding houses spawn
        // /////////////////////////////////////////////////////////////////////////////////////////
        spawnSurroundingObjects(5, 50, 1, villageHouse);
        

        // /////////////////////////////////////////////////////////////////////////////////////////
        // // Generate path
        // /////////////////////////////////////////////////////////////////////////////////////////

        Vector3 startPos = new Vector3(scList[0].bounds.center.x, scList[0].bounds.center.y+100, scList[0].bounds.center.z);
        if (Physics.Raycast(startPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground"))){
            startPos.y = hit.point.y;
        
            float segmentLength = 7f; // the desired length of each segment
            Vector3 offset;
            for (int i = 0; i < pathList.Count; i++)
            {
                Vector3 endPos = pathList[i];
                float distance = Vector3.Distance(startPos, endPos);
                int numSegments = Mathf.FloorToInt(distance / segmentLength);
                float sectionLength = distance / numSegments;
                Vector3 difference = endPos - startPos;
                Vector3 direction = difference.normalized;

                for (int j = 0; j < numSegments-2; j++)
                {
                    Vector3 sectionPoint = startPos + direction * sectionLength * j;
                    if(distance < 100f) offset = Random.onUnitSphere * 2;
                    else offset = Random.onUnitSphere * 3;
                    sectionPoint += offset;
                    Vector3 sectionPointUp = sectionPoint + new Vector3(0, 50, 0);
                    if (Physics.Raycast(sectionPointUp, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground"))){
                        sectionPoint.y = hit.point.y;
                        Quaternion rotationCenter = Quaternion.LookRotation(direction, Vector3.up);
                        Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        Quaternion finalRotation = rotationNormal * rotationCenter;
                        GameObject chosenRock = path[Random.Range(0, path.Length)];
                        Instantiate(chosenRock, new Vector3(sectionPoint.x, sectionPoint.y, sectionPoint.z), finalRotation);
                    }
                }
            }
        }
    } // make sure stones arent touching (they have tag) + rotate rocks randomly + add more different rocks
    


    public void spawnSurroundingObjects(int count, float minDistance, float radiusChange, GameObject prefab){
            for(int j = 0; j < scList.Count; j++) {
                int spawned = 0;
                int iterations = 0;
                while (spawned < count && iterations < 1000) {
                    // Vector3 randomPos = scList[j].bounds.center + new Vector3(Random.Range(-scList[j].bounds.extents.x, scList[j].bounds.extents.x),
                    //                     1000, Random.Range(-scList[j].bounds.extents.z, scList[j].bounds.extents.z));
                    Vector3 randomPos = scList[j].bounds.center + Random.insideUnitSphere * (scList[j].radius/radiusChange);
                    randomPos.y = 1000f;
                    if (Physics.Raycast(randomPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight)){
                        randomPos.y = hit.point.y;
                        Vector3 center = scList[j].bounds.center;
                        Vector3 newCenter = new Vector3(center.x,randomPos.y,center.z);
                        Vector3 direction = newCenter - randomPos;
                        direction = -direction;
                        Quaternion rotationCenter = Quaternion.LookRotation(direction, Vector3.up);
                        Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        Quaternion finalRotation = rotationNormal * rotationCenter;
                        Vector3 euler = finalRotation.eulerAngles;
                        euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                        euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
                        if(euler.x > minSlope && euler.x < maxSlope && euler.z > minSlope && euler.z < maxSlope){
                            randomPos = new Vector3(randomPos.x,randomPos.y - 1.5f,randomPos.z);
                            Collider[] surroundingColliders = Physics.OverlapSphere(randomPos, 50f, LayerMask.GetMask("House"));
                            //print(scList[j]+" "+surroundingColliders.Length + surroundingColliders[0].name);
                            if(surroundingColliders.Length == 0){
                                float distance = Vector3.Distance(randomPos, scList[j].bounds.center);
                                if(distance >= minDistance) {
                                    
                                    GameObject spawnedObj = Instantiate(prefab, randomPos, finalRotation);
                                    spawnedObj.name = "House " + j.ToString() + spawned.ToString();
                                    spawnedObj.AddComponent<SphereCollider>();
                                    spawnedObj.GetComponent<SphereCollider>().radius = 50f;
                                    spawnedObj.GetComponent<SphereCollider>().isTrigger = true;
                                    if(pathList.Count < 5){
                                        Vector3 pathPos = new Vector3(randomPos.x,randomPos.y,randomPos.z);
                                        pathList.Add(randomPos);
                                    }
                                    
                                    spawned++;
                                    // surroundingItems++;
                                }
                            }
                        }
                        
                    }
                    iterations++;
                }
            }
        }
}