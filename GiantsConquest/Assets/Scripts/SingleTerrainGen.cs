

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;
//using System;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor;






public class SingleTerrainGen : MonoBehaviour
{
    public GameObject terrainToSpawn;
    public GameObject well;
    public GameObject giant;
    public GameObject villageHouse;
    private TerrainCollider terrainCollider;
    public float maxHeight;
    public float minHeight;
    public float maxSlope;
    public float minSlope;
    private RaycastHit hit;

    GameObject spawnedPlayer;

    //Colouring
    public Gradient gradient;
    private TerrainData terrainData;
    float[,] colourHeights;
    Texture2D texture;
    //////////////////////////////////////////
    /// Painting Forestry
    ///     
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    public GameObject[] grassPrefabs;

    public GameObject[] faunaPrefabs;

    public int numberOfTrees = 20;
    public int numberOfRocks = 100;
    public int numberOfFauna= 10000;
    public float treePlacementRadius = 5f;
    
    private List<SphereCollider> scList = new List<SphereCollider>();
    private List<List<Vector3>> pathList = new List<List<Vector3>>();
    public List<NavMeshAgent> agentList = new List<NavMeshAgent>();
    private List<Vector3> pathIteration;
    public GameObject[] rocks;
    private NavMeshSurface navMeshSurface;
    private NavMeshAgent pathAgent;
    private NavMeshPath calcPath;
    public GameObject cube;
    public GameObject agentPos;
    private GameObject agent;
    public GameObject red;
    public GameObject blue;
    public GameObject green;
    public LayerMask walkableLayer;
    public List<GameObject> cloudList = new List<GameObject>();
    private List<Vector3> path = new List<Vector3>();
    // Create a list to hold the results of each thread
    //private List<IAsyncResult> results = new List<IAsyncResult>();
    private List<bool> successList = new List<bool>();

// Path connecting villages
    public List<Vector3> InstantiatedPath = new List<Vector3>();
    public List<List<Vector3>> InstantiatedListOfPaths = new List<List<Vector3>>();

    public Dictionary<GameObject, List<GameObject>> villageDictionary = new Dictionary<GameObject, List<GameObject>>();


    // Occlusion Culling

    private GameObject occlusionCulling;
    public GameObject platform;
    // chunks
    Vector3 terrainSize;
    
    public void Start()
    {

        GameObject spawned = Instantiate(terrainToSpawn, new Vector3(0, 0, 0), Quaternion.identity);
        MapGenerator script = spawned.GetComponent<MapGenerator>();
        float height = UnityEngine.Random.Range(150, 300);
        float noise = UnityEngine.Random.Range(1700, 2100);
        float noise_2 = UnityEngine.Random.Range(3600, 4500);
        int rand = UnityEngine.Random.Range(1, 2);
        if (rand == 1)
        {
            script.noiseData.noiseScale = noise;
        }
        else
        {
            script.noiseData.noiseScale = noise_2;
        }
        int oct = 4;
        float lac = UnityEngine.Random.Range(3.0f, 3.5f);
        script.noiseData.meshHeightMultiplier = height;
        script.noiseData.octaves = oct;
        script.noiseData.lacunarity = lac;
        script.generateWater = true;
        script.GenerateRandomSeed();
        script.GenerateMap();
        
        Mesh m = spawned.GetComponent<MeshCollider>().sharedMesh;
        m.RecalculateBounds();

        GameObject mesh2Terrain = GameObject.Find("Mesh2Terrain"); 
        

        mesh2Terrain.GetComponent<Object2Terrain>().CreateTerrain(spawned);
        GameObject convertedTerrain = GameObject.Find("Terrain0");
        convertedTerrain.transform.position = new Vector3(1900, 0, 1500);
        gradient = GameObject.Find("Mesh2Terrain").GetComponent<TerrainColour>().gradient;
        float[,] noiseMap = spawned.GetComponent<MapGenerator>().noiseMap;
        convertedTerrain.AddComponent<TerrainColour>();
        terrainSize = convertedTerrain.GetComponent<Terrain>().terrainData.size;
        Texture2D mapTexture = convertedTerrain.GetComponent<TerrainColour>().DisplayTerrain(noiseMap, gradient, convertedTerrain.GetComponent<Terrain>());
        
        // float terrainSizeX = mapTexture.width;
        // float terrainSizeZ = mapTexture.height;
        mesh2Terrain.GetComponent<Object2Terrain>().CreateTerrainChunks(spawned, mapTexture);
        GameObject[] chunks = new GameObject[20];
        Vector3 firstChunkPos = new Vector3(0, 0, 0);

        int numberOfObjects = 10;
        float pos0x = firstChunkPos.x + terrainSize.x/2;
        float pos0z = firstChunkPos.z + terrainSize.z/2;
        
        
        for (int i = 0; i<20; i++){
            GameObject chunk = GameObject.Find("Chunk" + i);
            Terrain t = chunk.GetComponent<Terrain>();
            gradient = GameObject.Find("Mesh2Terrain").GetComponent<TerrainColour>().gradient;
            chunk.gameObject.layer = LayerMask.NameToLayer("Ground");
            chunk.isStatic = false;
            chunk.gameObject.tag = "Chunk";

            #if UNITY_EDITOR
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(chunk);
            flags |= StaticEditorFlags.NavigationStatic;
            GameObjectUtility.SetStaticEditorFlags(chunk, flags);
            var flags2 = StaticEditorFlags.OffMeshLinkGeneration;
            GameObjectUtility.SetStaticEditorFlags(chunk, flags2);
            #endif

            chunk.AddComponent<NavMeshSurface>();
            NavMeshSurface navMeshSurface = chunk.GetComponent<NavMeshSurface>();
            navMeshSurface.overrideTileSize = true;
            navMeshSurface.tileSize = 125;
            navMeshSurface.overrideVoxelSize = true;
            navMeshSurface.voxelSize = 5;
            NavMeshBuildSettings buildSettings = navMeshSurface.GetBuildSettings();
            buildSettings.agentSlope = 25;
            navMeshSurface.BuildNavMesh();

            TreePrototype[] treePrototypes = new TreePrototype[treePrefabs.Length];
            // Iterate over each tree prefab
            for (int k= 0; k < treePrefabs.Length; k++)
            {
                // Create a new TreePrototype object
                TreePrototype treePrototype = new TreePrototype();
                // Assign the tree prefab to the prefab property of the TreePrototype
                treePrototype.prefab = treePrefabs[k];
                // Assign the TreePrototype to an element of the treePrototypes array
                treePrototypes[k] = treePrototype;
            }
            // Assign the treePrototypes array to the terrainData's treePrototypes property
            t.terrainData.treePrototypes = treePrototypes;
            // Refresh the terrain's tree instances
            t.terrainData.RefreshPrototypes();

            int iterations = 1000;

            TreeInstance[] trees = new TreeInstance[numberOfTrees];
            for (int l = 0; l < numberOfTrees; l++)
            {
                // Generate a random normalized position for the tree instance
                float x = Random.Range(0f, 1f);
                float z = Random.Range(0f, 1f);
                // Convert the normalized position to a world space position
                Vector3 worldSpacePosition = new Vector3(x * t.terrainData.size.x, 0, z * t.terrainData.size.z) + t.transform.position;
                // Calculate the y position of the tree instance using the SampleHeight method
                float y = t.SampleHeight(worldSpacePosition);
                //rocks
                if(l<=3 && iterations > 0){
                    if (y >= 100 && y <= 400)
                    {
                            // Convert the world space position to a normalized position
                            Vector3 normalizedPosition = new Vector3(x, y / t.terrainData.size.y, z);
                            // Create a new TreeInstance object
                            TreeInstance treeInstance = new TreeInstance();
                            // Assign a random prototypeIndex to the tree instance
                            treeInstance.prototypeIndex = Random.Range(7,9);
                            // Assign the normalized position to the tree instance
                            treeInstance.position = normalizedPosition;
                            // Set the widthScale and heightScale of the tree instance
                            int random = Random.Range(3,5);
                            treeInstance.widthScale = random;
                            treeInstance.heightScale = random;
                            // Set the color and lightmapColor of the tree instance
                            treeInstance.color = Color.white;
                            treeInstance.lightmapColor = Color.white;
                            treeInstance.rotation = Random.Range(0f, 360f);
                            trees[l] = treeInstance;
                    }else {
                        l--;
                        iterations--;
                    }
                }
                //stones
                else if(l<10 && l>3 && iterations > 0){
                    
                    if (y >= 100 && y <= 200)
                    {
                            // Convert the world space position to a normalized position
                            Vector3 normalizedPosition = new Vector3(x, y / t.terrainData.size.y, z);
                            // Create a new TreeInstance object
                            TreeInstance treeInstance = new TreeInstance();
                            // Assign a random prototypeIndex to the tree instance
                            treeInstance.prototypeIndex = Random.Range(9,18);
                            // Assign the normalized position to the tree instance
                            treeInstance.position = normalizedPosition;
                            // Set the widthScale and heightScale of the tree instance
                            int random = Random.Range(5,10);
                            treeInstance.widthScale = random;
                            treeInstance.heightScale = random;
                            // Set the color and lightmapColor of the tree instance
                            treeInstance.color = Color.white;
                            treeInstance.lightmapColor = Color.white;
                            treeInstance.rotation = Random.Range(0f, 360f);
                            trees[l] = treeInstance;
                    }else {
                        l--;
                        iterations--;
                    }
                }
                else if(l<=numberOfTrees && l>10 && iterations > 0){
                    //trees
                    if (y >= 150 && y <= 400)
                    {
                            // Convert the world space position to a normalized position
                            Vector3 normalizedPosition = new Vector3(x, y / t.terrainData.size.y, z);
                            // Create a new TreeInstance object
                            TreeInstance treeInstance = new TreeInstance();
                            // Assign a random prototypeIndex to the tree instance
                            float randomTree = Random.Range(0f, 1f);
                            if(randomTree<=0.05f){
                                treeInstance.prototypeIndex = 6;
                            }else{
                                treeInstance.prototypeIndex = Random.Range(0, 5);
                            }
                            // Assign the normalized position to the tree instance
                            treeInstance.position = normalizedPosition;
                            // Set the widthScale and heightScale of the tree instance
                            int random = Random.Range(3,5);
                            treeInstance.widthScale = random;
                            treeInstance.heightScale = random;
                            // Set the color and lightmapColor of the tree instance
                            treeInstance.color = Color.white;
                            treeInstance.lightmapColor = Color.white;
                            treeInstance.rotation = Random.Range(0f, 360f);
                            trees[l] = treeInstance;
                    }else {
                        l--;
                        iterations--;
                    }
                }
            }
            t.terrainData.treeInstances = trees;


            // /////////////////////////////////////////////////////////////////////////////////////////
            /// Mass Place Grass
            /// /////////////////////////////////////////////////////////////////////////////////////////
            
            //Create an array to hold the DetailPrototype objects
            DetailPrototype[] detailPrototypes = new DetailPrototype[grassPrefabs.Length];

            // Iterate over each grass prefab
            for (int g = 0; g < grassPrefabs.Length; g++)
            {
                // Create a new DetailPrototype object
                DetailPrototype detailPrototype = new DetailPrototype();
                // Assign the grass prefab to the prototype property of the DetailPrototype
                detailPrototype.useInstancing = true;
                detailPrototype.prototype = grassPrefabs[g];
                detailPrototype.usePrototypeMesh = true;
                
                // Set other properties of the DetailPrototype (such as render mode, min/max width/height, etc.)
                detailPrototype.renderMode = DetailRenderMode.VertexLit;
                detailPrototype.minWidth = 20f;
                detailPrototype.maxWidth = 25f;
                detailPrototype.minHeight = 5f;
                detailPrototype.maxHeight = 10f;
                detailPrototype.noiseSpread = 0.5f;
                detailPrototype.noiseSeed = 69420;
                // Assign the DetailPrototype to an element of the detailPrototypes array
                detailPrototypes[g] = detailPrototype;
            }

            // Assign the detailPrototypes array to the terrainData's detailPrototypes property
            t.terrainData.detailPrototypes = detailPrototypes;
            t.terrainData.RefreshPrototypes();
            t.terrainData.SetDetailResolution(1024, 32);
            // Create a 2D array to hold the detail densities
            var map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);
            var map2 = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 1);

            // Set the density of the detail objects
            for (int y = 0; y < t.terrainData.detailHeight; y++)
            {
                for (int x = 0; x < t.terrainData.detailWidth; x++)
                {
                    float h = t.terrainData.GetInterpolatedHeight((float)x / t.terrainData.detailWidth, (float)y / t.terrainData.detailHeight);

                    if (h > minHeight && h < maxHeight)
                    {
                        map[y, x] = 4; 
                        map2[y, x] = 4;// second type of grass
                    }
                }
            }

            // Assign the detail layer to the terrain data
            t.terrainData.SetDetailLayer(0, 0, 0, map);
            t.terrainData.SetDetailLayer(0, 0, 1, map2);
            t.treeDistance = 1000;
            t.detailObjectDistance = 500;

            OcclusionArea occlusionArea = chunk.AddComponent<OcclusionArea>();
            Vector3 terrainS = t.terrainData.bounds.size;
            occlusionArea.size = new Vector3(terrainS.x, 1000, terrainS.z);
            occlusionArea.center = new Vector3(terrainS.x/2, 0, terrainS.z/2);
            t.Flush();

            if(i==0){
                firstChunkPos = chunk.transform.position;
            }
            
            chunks.Append(chunk);
        }


        print(chunks.Length + " chunks");
        Destroy(convertedTerrain);
        Destroy(mesh2Terrain);

        spawned.transform.DetachChildren();
        WaterGenerator waterScript = FindObjectOfType<WaterGenerator>();
        GameObject water = waterScript.gameObject;
        water.transform.position = new Vector3(1900, -50, 1500);
        Destroy(spawned);

        #if UNITY_EDITOR
        Lightmapping.Bake();
        StaticOcclusionCulling.Compute();
        #endif
        
        /// /////////////////////////////////////////////////////////////////////////////////////////
        /// center spawn 
        ///////////////////////////////////////////////////////////////////////////////////////////// 

        // /// /////////////////////////////////////////////////////////////////////////////////////////
        // /// spawn Giant
        // /// /////////////////////////////////////////////////////////////////////////////////////////   
        

        int giantCount = 1;
        for (int i = 0; i < giantCount; i++)
        {
            spawnedPlayer = Instantiate(giant, new Vector3(Random.Range(firstChunkPos.x, firstChunkPos.x+ terrainSize.x), 1000 , Random.Range(firstChunkPos.x, firstChunkPos.x+ terrainSize.x)), Quaternion.identity);
        }

        
        
        
        int iter = 1000;
        Vector3 chunkSize = new Vector3(terrainSize.x / 10, terrainSize.y, terrainSize.z / 10);
        for (int i = 0; i < numberOfObjects; i++)
        {
            int random = Random.Range(0, 20);
            GameObject chunkPos = GameObject.Find("Chunk" + random);

            Vector3 position = new Vector3(
                Random.Range(chunkPos.transform.position.x, chunkPos.transform.position.x + chunkSize.x),
                2000,
                Random.Range(chunkPos.transform.position.z, chunkPos.transform.position.z + chunkSize.z)
            );
            position.y = GetTerrainHeight(position);

            if (position.y > minHeight && position.y < maxHeight)
            {
                Quaternion rotation = GetFinalRotation(position, position, false);

                if (IsValidSlope(rotation, minSlope, maxSlope))
                {
                    if (IsPositionFarFromCenters(position, scList))
                    {
                        Vector3 instantiationPoint = GetInstantiationPoint(position, well);
                        GameObject villageCenter = Instantiate(well, instantiationPoint, rotation);
                        agentPos.transform.position = villageCenter.transform.position;
                        agent = Instantiate(agentPos, instantiationPoint, rotation);
                        villageCenter.name = "Well" + scList.Count.ToString();
                        SphereCollider villageCenterCollider = villageCenter.AddComponent<SphereCollider>();
                        villageCenterCollider.radius = 500f;
                        villageCenterCollider.isTrigger = true;

                        // Find the nearest valid position on the NavMesh and place the agent there
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(agent.transform.position, out hit, 20f, NavMesh.AllAreas))
                        {
                            agent.transform.position = hit.position;
                            scList.Add(villageCenterCollider);
                        }
                        else
                        {
                            // Handle the case when no valid position is found within the specified range
                            Destroy(agent);
                            Destroy(villageCenter);
                            i--;
                            continue;
                        }

                        agentList.Add(agent.GetComponent<NavMeshAgent>());
                    }
                    else
                    {
                        i--;
                    }
                }
                else i--;
            }
            else i--;
        }



        for(int i =1; i<agentList.Count; i++){
            generatePath(agentList[0], agentList[i].transform.position);
        }

        int r = Random.Range(0,agentList.Count);
        spawnedPlayer.transform.position = new Vector3(agentList[r].transform.position.x + 50, agentList[r].transform.position.y + 100 , agentList[r].transform.position.z + 50);

    // //     // /////////////////////////////////////////////////////////////////////////////////////////
    // //     // // surrounding houses spawn
    // //     // /////////////////////////////////////////////////////////////////////////////////////////
        SpawnSurroundingObjects(100, 1, villageHouse);

        
        iter = 1000;
        for (int i = 0; i < 500; i++)
        {
            int random = Random.Range(0, 20);
            GameObject chunkPos = GameObject.Find("Chunk" + random);

            Vector3 position = new Vector3(
                Random.Range(chunkPos.transform.position.x, chunkPos.transform.position.x + chunkSize.x),
                2000,
                Random.Range(chunkPos.transform.position.z, chunkPos.transform.position.z + chunkSize.z)
            );
            position.y = 2000f;
            Instantiate(cloudList[Random.Range(0,cloudList.Count)], position, Quaternion.identity);
        }
        
        


    // //     // /////////////////////////////////////////////////////////////////////////////////////////
    // //     // // Generate path
    // //     // /////////////////////////////////////////////////////////////////////////////////////////
    //     for (int s = 0; s < pathList.Count; s++)
    //     {
    //         Vector3 startPos = new Vector3(scList[s].bounds.center.x, scList[s].bounds.center.y + 100, scList[s].bounds.center.z);
    //         if (Physics.Raycast(startPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
    //         {
    //             startPos.y = hit.point.y;

    //             float segmentLength = 7f; // the desired length of each segment
    //             Vector3 offset;
    //             for (int i = 0; i < pathIteration.Count; i++)
    //             {
    //                 Vector3 endPos = pathList[s][i];
    //                 float distance = Vector3.Distance(startPos, endPos);
    //                 int numSegments = Mathf.FloorToInt(distance / segmentLength);
    //                 float sectionLength = distance / numSegments;
    //                 Vector3 difference = endPos - startPos;
    //                 Vector3 direction = difference.normalized;

    //                 for (int j = 0; j < numSegments - 2; j++)
    //                 {
    //                     Vector3 sectionPoint = startPos + direction * sectionLength * j;
    //                     if (distance < 100f) offset = UnityEngine.Random.onUnitSphere * 2;
    //                     else offset = UnityEngine.Random.onUnitSphere * 3;
    //                     sectionPoint += offset;
    //                     GameObject chosenRock = rocks[UnityEngine.Random.Range(0, rocks.Length)];
    //                     Vector3 centerObj = chosenRock.transform.localScale / 2f;
    //                     Vector3 instantiationPoint = sectionPoint + centerObj;
    //                     Vector3 sectionPointUp = instantiationPoint + new Vector3(0, 50, 0);
    //                     if (Physics.Raycast(sectionPointUp, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
    //                     {
    //                         instantiationPoint.y = hit.point.y;
    //                         Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
    //                         Collider[] stoneColliders = Physics.OverlapSphere(instantiationPoint, 2f, LayerMask.GetMask("stone"));
    //                         Collider[] wellColliders = Physics.OverlapSphere(instantiationPoint, 2f, LayerMask.GetMask("well"));
    //                         Collider[] houseColliders = Physics.OverlapSphere(instantiationPoint, 2f, LayerMask.GetMask("House"));
    //                         if (stoneColliders.Length == 0 && wellColliders.Length == 0 && houseColliders.Length == 0)
    //                         {
    //                             Instantiate(chosenRock, instantiationPoint, rotationNormal);
    //                         }
    //                     }
    //                     else numSegments++;
    //                 }
    //             }
    //         }
    //     }
    }
    
    bool IsPositionFarFromCenters(Vector3 position, List<SphereCollider> villageCenters)
    {
        float minDistanceBetweenCenters = 750f;
        if(villageCenters.Count != 0){
            foreach (SphereCollider center in villageCenters)
            {
                if (Vector3.Distance(position, center.transform.position) < minDistanceBetweenCenters)
                {
                    return false;
                }
            }
        } else return true;
        return true;
    }


    public void SpawnSurroundingObjects(float minDistance, float radiusChange, GameObject prefab)
    {
        for (int j = 0; j < scList.Count; j++)
        {
            int spawned = 0;
            int iterations = 0;
            int count = UnityEngine.Random.Range(3, 7);
            List<Vector3> pathIteration = new List<Vector3>();
            scList[j].gameObject.GetComponent<villagerSpawner>().villageNum = j;

            GameObject wellObject = scList[j].gameObject;

            if (!villageDictionary.ContainsKey(wellObject))
            {
                villageDictionary[wellObject] = new List<GameObject>();
            }

            // Keep trying to spawn objects until the desired count is reached or the iteration limit is hit.
            while (spawned < count && iterations < 1000)
            {
                iterations++;

                // Generate a random position and find the corresponding terrain height.
                Vector3 randomPos = GetRandomPosition(scList[j], radiusChange, minDistance);
                float terrainHeight = GetTerrainHeight(randomPos);

                // Check if the position is within the desired height range.
                if (terrainHeight < maxHeight && terrainHeight > minHeight)
                {
                    // Calculate the position and rotation for the object to be spawned.
                    Vector3 instantiationPoint = GetInstantiationPoint(randomPos, prefab);
                    Quaternion finalRotation = GetFinalRotation(instantiationPoint, scList[j].bounds.center, true);

                    

                    // Check if the object would be placed on a slope that's too steep or blocked by other houses.
                    if (IsValidSlope(finalRotation, minSlope, maxSlope) &&
                        !IsBlockedByHeightChange(instantiationPoint) &&
                        !IsBlockedByHouses(instantiationPoint, scList[j].bounds.center, minDistance))
                    {
                        // Spawn the object and configure its components.
                        GameObject spawnedObj = Instantiate(prefab, instantiationPoint, finalRotation * prefab.transform.rotation);

                        spawnedObj.name = $"House {j}{spawned}";
                        AddHouseCollider(spawnedObj, 50f);
                        scList[j].gameObject.GetComponent<villagerSpawner>().houseList.Add(spawnedObj);
                        // Record the spawned object's position and increment the spawn counter.
                        pathIteration.Add(instantiationPoint);
                        spawned++;
                        villageDictionary[wellObject].Add(spawnedObj);

                        if (spawned == count)
                        {
                            pathList.Add(pathIteration);
                        }
                    }
                }
            }
        }
    }

    // Helper functions go here.
    private Vector3 GetRandomPosition(Collider sphereCollider, float radiusChange, float minimumDistance)
    {
        Vector3 randomPos;
        float distance;

        do
        {
            // Get a random direction
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
            randomDirection.y = 0; // Ensure the movement is only in the XZ plane

            // Calculate a random distance within the given range
            float randomDistance = UnityEngine.Random.Range(minimumDistance, sphereCollider.bounds.extents.magnitude / radiusChange);

            // Compute the new position
            randomPos = sphereCollider.bounds.center + randomDirection.normalized * randomDistance;
            randomPos.y = 1000f;
            distance = Vector3.Distance(randomPos, sphereCollider.bounds.center);
        } while (distance < minimumDistance);

        return randomPos;
    }



    private float GetTerrainHeight(Vector3 position)
    {
        GameObject chunkPoint = GetChunkAtPoint(position);
        return chunkPoint.GetComponent<Terrain>().SampleHeight(position);
    }

    private Vector3 GetInstantiationPoint(Vector3 position, GameObject prefab)
    {
        Vector3 centerOffset = prefab.transform.localScale / 2f;
        Vector3 instantiationPoint = position + centerOffset;
        instantiationPoint.y = GetTerrainHeight(position);
        return instantiationPoint;
    }

    private Quaternion GetFinalRotation(Vector3 instantiationPoint, Vector3 targetPoint, bool look)
    {
        // Get the terrain normal at the instantiation point
        GameObject chunkPoint = GetChunkAtPoint(instantiationPoint);
        Terrain terrain = chunkPoint.GetComponent<Terrain>();
        // Calculate the terrain coordinates (ranging from 0 to 1) based on the instantiation point
        Vector3 terrainPosition = (instantiationPoint - terrain.transform.position);
        Vector3 terrainCoordinates = new Vector3(terrainPosition.x / terrain.terrainData.size.x, 0, terrainPosition.z / terrain.terrainData.size.z);
        
        // Get the terrain normal at the instantiation point
        Vector3 terrainNormal = terrain.terrainData.GetInterpolatedNormal(terrainCoordinates.x, terrainCoordinates.z);
        Debug.DrawRay(instantiationPoint, terrainNormal * 50f, Color.green, 50f);
        
        // Calculate the rotation based on the terrain normal
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);
        rotation *= Quaternion.Euler(0, 180, 0);

        if(look){
                Vector3 forwardVector = (targetPoint - instantiationPoint).normalized;
                forwardVector -= Vector3.Dot(forwardVector, terrainNormal) * terrainNormal;
                forwardVector.Normalize();

                // Calculate the rotation to make the object face the target point
                Quaternion lookRotation = Quaternion.LookRotation(forwardVector, terrainNormal);

                // Combine the terrain alignment and the look rotation
                rotation = lookRotation * rotation;
        }
        
        return rotation;
    }



    private bool IsValidSlope(Quaternion rotation, float minSlope, float maxSlope)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
        euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;

        return euler.x > minSlope && euler.x < maxSlope && euler.z > minSlope && euler.z < maxSlope;
    }

    private bool IsBlockedByHeightChange(Vector3 instantiationPoint)
    {
        int raycastCount = 360;
        float angleStep = 360f / raycastCount;
        Vector3 checkSurround = new Vector3(instantiationPoint.x, instantiationPoint.y + 6.5f, instantiationPoint.z);

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = i * angleStep;
            Vector3 checkDirection = Quaternion.Euler(0, angle, 0) * Vector3.right;
            if (Physics.Raycast(checkSurround, checkDirection, out RaycastHit hit, 20f, LayerMask.GetMask("Ground")))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockedByHouses(Vector3 instantiationPoint, Vector3 center, float minDistance)
    {
        float distanceToCenter = Vector3.Distance(center, instantiationPoint);
        Vector3 difference = center - instantiationPoint;
        Vector3 toCenter = difference.normalized;
        if (Physics.Raycast(instantiationPoint, toCenter, out RaycastHit hit, distanceToCenter - 5f, LayerMask.GetMask("House")))
        {
            return true;
        }

        Collider[] surroundingColliders = Physics.OverlapSphere(instantiationPoint, 50f, LayerMask.GetMask("House"));
        if (surroundingColliders.Length > 0)
        {
            return true;
        }

        return false;
    }

private void AddHouseCollider(GameObject house, float radius)
{
    SphereCollider sphereCollider = house.AddComponent<SphereCollider>();
    sphereCollider.radius = radius;
    sphereCollider.isTrigger = true;
}


    int GetRandomIndex(GameObject[] array, int index)
    {
        float value = Random.value;
        if(index == 0){
        if (value < 0.666f)
            return Random.Range(0,1);
        else
            return Random.Range(2, array.Length);
        }
        else if(index == 1){
            if (value < 0.666f)
            return Random.Range(7,9);
        else
            return Random.Range(9, array.Length + 7);
        }
        return 0;
    }

    private Vector3 CalculateBezierPoint(float t, List<Vector3> points)
    {
        if (points.Count == 1)
        {
            return points[0];
        }

        List<Vector3> newPoints = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 newPoint = Vector3.Lerp(points[i], points[i + 1], t);

            newPoints.Add(newPoint);
        }

        return CalculateBezierPoint(t, newPoints);
    }

    public void generatePath(NavMeshAgent startAgent, Vector3 end)
    {
        LineRenderer lineRenderer;
        calcPath = new NavMeshPath();
        InstantiatedPath.Clear();
        NavMeshHit hit;
        if (NavMesh.SamplePosition(startAgent.transform.position, out hit, 100.0f, NavMesh.AllAreas))
        {
            startAgent.transform.position = hit.position;
        }
        if (NavMesh.SamplePosition(end, out hit, 100.0f, NavMesh.AllAreas))
        {
            end = hit.position;
        }
        startAgent.CalculatePath(end, calcPath);
        if (calcPath.status == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("Path Partial");
            Debug.Log(calcPath.corners.Length +" - corner count");
        }
        if (calcPath.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("Path Invalid");
        }
        Vector3 start = startAgent.gameObject.transform.position;

        int numCorners = calcPath.corners.Length;

        List<Vector3> corners = new List<Vector3>();
        float dist1 = Vector3.Distance(start, end);
        int steps1 = Mathf.RoundToInt(dist1 / 7f);
        int steps = 0;
        GameObject s = Instantiate(red,calcPath.corners[0],Quaternion.identity);
        lineRenderer = s.GetComponent<LineRenderer>();
        
        for (int i = 0; i < calcPath.corners.Length - 1; i++)
        {
            Vector3 startPoint = calcPath.corners[i];
            corners.Add(startPoint);
            Vector3 endPoint = calcPath.corners[i + 1];
            Vector3 controlPoint = (startPoint + endPoint) / 2f;
            float dist = Vector3.Distance(startPoint, endPoint);
            steps = Mathf.RoundToInt(dist / 7f);
            corners.Add(controlPoint);
            corners.Add(endPoint);
        }



        // Create a new line renderer component
    
    lineRenderer.startWidth = 5f;
    lineRenderer.endWidth = 5f;
    lineRenderer.positionCount = steps1 + 1;


    int validPointCount = 0;
    for (int j = 0; j <= steps1; j++)
    {
        float t = (float)j / (steps1 - 1);
        Vector3 point = CalculateBezierPoint(t, corners);

        // Clamp the point.x and point.z values to be within the terrain bounds
        point.x = Mathf.Clamp(point.x, 0, terrainSize.x);
        point.z = Mathf.Clamp(point.z, 0, terrainSize.z);

        GameObject chunkPoint = GetChunkAtPoint(point);
        if (chunkPoint != null)
        {
            point.y = chunkPoint.GetComponent<Terrain>().SampleHeight(point);

            // Check if the point is not beneath the terrain
            if (point.y >= minHeight)
            {
                InstantiatedPath.Add(point);
                lineRenderer.SetPosition(validPointCount, point);
                validPointCount++;

                // Set the rotation of the line renderer to match the ground normal
                Vector3 up = hit.normal;
                Vector3 right = Vector3.Cross(point, up);
                lineRenderer.transform.rotation = Quaternion.LookRotation(point, up) * Quaternion.FromToRotation(Vector3.up, right);
            }
        }
    }
    InstantiatedListOfPaths.Add(InstantiatedPath);

    // Set the number of positions in the line renderer to match the valid points count
    lineRenderer.positionCount = validPointCount;


    }

    void SpawnBreakableObjects(string type, int numObjects, List<GameObject> objectList, float minSlope, float maxSlope, Vector3 terrainSize, float maxHeight, float minHeight)
    {
        // Set the number of clusters and the cluster radius
        int numClusters = 1000;
        float clusterRadius = 200f;

        // Generate cluster centers
        Vector3[] clusterCenters = new Vector3[numClusters];
        for (int i = 0; i < numClusters; i++)
        {
            Vector3 clusterPos = new Vector3(UnityEngine.Random.Range(0, terrainSize.x), 1000, UnityEngine.Random.Range(0, terrainSize.z));
            if (Physics.Raycast(clusterPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
            {
                clusterPos.y = hit.point.y;
                clusterCenters[i] = clusterPos;
            }else i--;
        }

        int j = 0;
        int maxIterations = 1000;
        while (j < numObjects && maxIterations > 0)
        {
            // Choose a random cluster center
            Vector3 clusterCenter = clusterCenters[UnityEngine.Random.Range(0, numClusters)];

            // Generate a random position within the cluster
            Vector3 position = new Vector3(UnityEngine.Random.Range(clusterCenter.x - clusterRadius, clusterCenter.x + clusterRadius), 1000, UnityEngine.Random.Range(clusterCenter.z - clusterRadius, clusterCenter.z + clusterRadius));

            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
            {
                position.y = hit.point.y;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 euler = rotation.eulerAngles;
                euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
                if (euler.x > -35 && euler.x < 35 && euler.z > -35 && euler.z < 35)
                {
                    int RandomIndex = UnityEngine.Random.Range(0, objectList.Count);
                    GameObject obj = Instantiate(objectList[RandomIndex], position, rotation);
                    maxIterations = 1000;
                    if (type == "Wood")
                    {
                        float RandomScale = UnityEngine.Random.Range(2f, 3f);
                        obj.transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);
                    }
                    else if (type == "Stone")
                    {
                        float RandomScale = UnityEngine.Random.Range(0.5f, 2f);
                        obj.transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);
                    }
                    j++;
                }
            }
            maxIterations--;
        }

        
    }

    public GameObject GetChunkAtPoint(Vector3 point)
{
    // Replace "terrainChunks" with the list or array that holds your terrain chunk GameObjects.
    GameObject[] terrainChunks = GameObject.FindGameObjectsWithTag("Chunk");
    print(terrainChunks.Length + " :terrainChunks.Length");

    foreach (GameObject chunk in terrainChunks)
    {
        // Check if the point is within the chunk's bounds.
        Bounds bounds = chunk.GetComponent<Terrain>().terrainData.bounds;
        Vector3 chunkWorldPos = chunk.transform.position;
        bounds.center += chunkWorldPos;

        // Expand the bounds slightly to account for floating-point precision issues.
        bounds.Expand(0.1f);

        // Create a new Vector3 that ignores the Y component of the point when checking if it is within the chunk's bounds.
        Vector3 pointXZ = new Vector3(point.x, bounds.center.y, point.z);

        if (bounds.Contains(pointXZ))
        {
            return chunk;
        }
    }

    return null;
}



}





