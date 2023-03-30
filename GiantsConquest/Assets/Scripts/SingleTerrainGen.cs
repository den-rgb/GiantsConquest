using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;
//using System;
using System.Threading.Tasks;
using System.Linq;






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

    public int numberOfTrees = 10000;
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
    private List<Vector3> path = new List<Vector3>();
    // Create a list to hold the results of each thread
    //private List<IAsyncResult> results = new List<IAsyncResult>();
    private List<bool> successList = new List<bool>();

// Path connecting villages
    public List<GameObject> InstantiatedPath = new List<GameObject>();
    public List<List<GameObject>> InstantiatedListOfPaths = new List<List<GameObject>>();

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
        GameObject convertedTerrain = GameObject.Find("Terrain");
        Terrain t = convertedTerrain.GetComponent<Terrain>();
        gradient = GameObject.Find("Mesh2Terrain").GetComponent<TerrainColour>().gradient;
        float[,] noiseMap = spawned.GetComponent<MapGenerator>().noiseMap;
        convertedTerrain.AddComponent<TerrainColour>();
        convertedTerrain.GetComponent<TerrainColour>().DisplayTerrain(noiseMap, gradient, convertedTerrain.GetComponent<Terrain>());
        Destroy(mesh2Terrain);
        convertedTerrain.gameObject.layer = LayerMask.NameToLayer("Ground");
    

        spawned.transform.DetachChildren();
        WaterGenerator waterScript = FindObjectOfType<WaterGenerator>();
        GameObject water = waterScript.gameObject;
        water.transform.position = new Vector3(0, -150, 0);
        Destroy(spawned);

        // /////////////////////////////////////////////////////////////////////////////////////////
        /// NavMesh
        //// /////////////////////////////////////////////////////////////////////////////////////////
        navMeshSurface = convertedTerrain.AddComponent<NavMeshSurface>();
        navMeshSurface.overrideVoxelSize = true;
        navMeshSurface.voxelSize = 1;
        NavMeshBuildSettings buildSettings = navMeshSurface.GetBuildSettings();
        buildSettings.agentSlope = 25;
        navMeshSurface.BuildNavMesh();
        
        /// /////////////////////////////////////////////////////////////////////////////////////////
        /// center spawn 
        ///////////////////////////////////////////////////////////////////////////////////////////// 

        terrainCollider = convertedTerrain.GetComponent<TerrainCollider>();
        Vector3 terrainSize = spawned.GetComponentInChildren<MeshRenderer>().bounds.size;
        int numberOfObjects = 10;
        

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Generate a UnityEngine.Random position within the terrain's bounds
            Vector3 position = new Vector3(UnityEngine.Random.Range(0, terrainSize.x), 1000, UnityEngine.Random.Range(0, terrainSize.z));
            if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
            {
                position.y = hit.point.y;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 euler = rotation.eulerAngles;
                //converting euler angles to degrees
                euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
                if (euler.x > minSlope && euler.x < maxSlope && euler.z > minSlope && euler.z < maxSlope)
                {
                    // Spawn the object at the hit point
                    GameObject villageCenter = Instantiate(well, position, rotation);
                    agentPos.transform.position = villageCenter.transform.position;
                    agent = Instantiate(agentPos, position, rotation);

                    villageCenter.name = "Well" + scList.Count.ToString();
                    SphereCollider villageCenterCollider = villageCenter.AddComponent<SphereCollider>();
                    villageCenterCollider.radius = 200f;
                    villageCenterCollider.isTrigger = true;
                    scList.Add(villageCenterCollider);

                    agentList.Add(agent.GetComponent<NavMeshAgent>());
                }
                else
                {
                    numberOfObjects++;
                }
            }
            else
            {
                numberOfObjects++;
            }
        }

        for(int i =1; i<agentList.Count; i++){
            generatePath(agentList[0], agentList[i].transform.position);
        }

        int giantCount = 1;
        for (int i = 0; i < giantCount; i++)
        {
            Vector3 giantSpawn = new Vector3(UnityEngine.Random.Range(0, terrainSize.x), 1000, UnityEngine.Random.Range(0, terrainSize.z));
            if (Physics.Raycast(giantSpawn, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
            {
                giantSpawn.y = hit.point.y + 10f;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 euler = rotation.eulerAngles;
                    //converting euler angles to degrees
                euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
                if (euler.x > minSlope && euler.x < maxSlope && euler.z > minSlope && euler.z < maxSlope)
                    {
                        Instantiate(giant, giantSpawn, rotation);
                    }else{
                        giantCount++;
                    }
            }else{
                giantCount++;
            }
        }
        

    //     // /////////////////////////////////////////////////////////////////////////////////////////
    //     // // surrounding houses spawn
    //     // /////////////////////////////////////////////////////////////////////////////////////////
            spawnSurroundingObjects(50, 1, villageHouse);

            // /////////////////////////////////////////////////////////////////////////////////////////
        /// Mass Place Trees
        /// /////////////////////////////////////////////////////////////////////////////////////////
        TreePrototype[] treePrototypes = new TreePrototype[treePrefabs.Length];
        // Iterate over each tree prefab
        for (int i = 0; i < treePrefabs.Length; i++)
        {
            // Create a new TreePrototype object
            TreePrototype treePrototype = new TreePrototype();
            // Assign the tree prefab to the prefab property of the TreePrototype
            treePrototype.prefab = treePrefabs[i];
            // Assign the TreePrototype to an element of the treePrototypes array
            treePrototypes[i] = treePrototype;
        }
        // Assign the treePrototypes array to the terrainData's treePrototypes property
        t.terrainData.treePrototypes = treePrototypes;
        // Refresh the terrain's tree instances
        t.terrainData.RefreshPrototypes();



        TreeInstance[] trees = new TreeInstance[numberOfTrees+ numberOfFauna];
        for (int i = 0; i < numberOfTrees + numberOfFauna; i++)
        {
            // Generate a random normalized position for the tree instance
            float x = Random.Range(0f, 1f);
            float z = Random.Range(0f, 1f);
            // Convert the normalized position to a world space position
            Vector3 worldSpacePosition = new Vector3(x * t.terrainData.size.x, 0, z * t.terrainData.size.z) + t.transform.position;
            // Calculate the y position of the tree instance using the SampleHeight method
            float y = t.SampleHeight(worldSpacePosition);
            //rocks
            if(i<=200){
                if (y >= minHeight && y <= maxHeight)
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
                        trees[i] = treeInstance;
                }else i--;
            }
            //stones
            else if(i<1200 && i>200){
                if (y >= minHeight && y <= maxHeight)
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
                        trees[i] = treeInstance;
                }else i--;
            }
            else if(i<=numberOfTrees && i>1200){
                //trees
                if (y >= minHeight && y <= maxHeight)
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
                        trees[i] = treeInstance;
                }else i--;
            }else{
                // if (y >= minHeight && y <= 700)
                // {
                //         // Convert the world space position to a normalized position
                //         Vector3 normalizedPosition = new Vector3(x, y / t.terrainData.size.y, z);
                //         // Create a new TreeInstance object
                //         TreeInstance treeInstance = new TreeInstance();
                //         // Assign a random prototypeIndex to the tree instance
                        
                //         treeInstance.prototypeIndex = 19;
                    
                //         // Assign the normalized position to the tree instance
                //         treeInstance.position = normalizedPosition;
                //         // Set the widthScale and heightScale of the tree instance
                //         int random = Random.Range(100,101);
                //         treeInstance.widthScale = 1;
                //         treeInstance.heightScale = 1;
                //         // Set the color and lightmapColor of the tree instance
                //         treeInstance.color = Color.white;
                //         treeInstance.lightmapColor = Color.white;
                //         treeInstance.rotation = Random.Range(0f, 360f);
                //         trees[i] = treeInstance;
                // }else i--;
            }
        }
        t.terrainData.treeInstances = trees;


        // /////////////////////////////////////////////////////////////////////////////////////////
        /// Mass Place Grass
        /// /////////////////////////////////////////////////////////////////////////////////////////
        
        // Create an array to hold the DetailPrototype objects
        DetailPrototype[] detailPrototypes = new DetailPrototype[grassPrefabs.Length];

        // Iterate over each grass prefab
        for (int i = 0; i < grassPrefabs.Length; i++)
        {
            // Create a new DetailPrototype object
            DetailPrototype detailPrototype = new DetailPrototype();
            // Assign the grass prefab to the prototype property of the DetailPrototype
            detailPrototype.useInstancing = true;
            detailPrototype.prototype = grassPrefabs[i];
            detailPrototype.usePrototypeMesh = true;
            
            // Set other properties of the DetailPrototype (such as render mode, min/max width/height, etc.)
            detailPrototype.renderMode = DetailRenderMode.VertexLit;
            detailPrototype.minWidth = 10f;
            detailPrototype.maxWidth = 20f;
            detailPrototype.minHeight = 10f;
            detailPrototype.maxHeight = 20f;
            detailPrototype.noiseSpread = 0.5f;
            detailPrototype.noiseSeed = 693192397;
            // Assign the DetailPrototype to an element of the detailPrototypes array
            detailPrototypes[i] = detailPrototype;
        }

        // Assign the detailPrototypes array to the terrainData's detailPrototypes property
        t.terrainData.detailPrototypes = detailPrototypes;
        t.terrainData.RefreshPrototypes();
        t.terrainData.SetDetailResolution(2048, 32);
        // Create a 2D array to hold the detail densities
        var map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);
        var map2 = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);

        // Set the density of the detail objects
        for (int y = 0; y < t.terrainData.detailHeight; y++)
        {
            for (int x = 0; x < t.terrainData.detailWidth; x++)
            {
                float h = t.terrainData.GetInterpolatedHeight((float)x / t.terrainData.detailWidth, (float)y / t.terrainData.detailHeight);

                if (h > minHeight && h < 700)
                {
                    map[y, x] = 105; 
                    map2[y, x] = 105;// second type of grass
                }
            }
        }

        // Assign the detail layer to the terrain data
        t.terrainData.SetDetailLayer(0, 0, 0, map);
        t.terrainData.SetDetailLayer(0, 0, 1, map2);


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



    public void spawnSurroundingObjects(float minDistance, float radiusChange, GameObject prefab)
    {
        for (int j = 0; j < scList.Count; j++)
        {
            int spawned = 0;
            int iterations = 0;
            int count = UnityEngine.Random.Range(3,7);
            pathIteration = new List<Vector3>();
            while (spawned < count && iterations < 1000)
            {
                Vector3 RandomPos = scList[j].bounds.center + UnityEngine.Random.insideUnitSphere * (scList[j].radius / radiusChange);
                RandomPos.y = 1000f;
                if (Physics.Raycast(RandomPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
                {
                    int raycastCount = 360;
                    float angleStep = 360f / raycastCount;
                    Vector3 centeObj = prefab.transform.localScale / 2f;
                    Vector3 instantiationPoint = RandomPos + centeObj;
                    instantiationPoint.y = hit.point.y;
                    Vector3 center = scList[j].bounds.center;
                    Vector3 newCenter = new Vector3(center.x, instantiationPoint.y, center.z);
                    Vector3 direction = newCenter - instantiationPoint;
                    direction = -direction;
                    Quaternion rotationCenter = Quaternion.LookRotation(direction, Vector3.up);
                    Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal); //possibly needed changing due to hit.normal not being the same hit point as instantiationPoint
                    Quaternion finalRotation = rotationNormal * rotationCenter;

                    //Check if too close too height change
                    Vector3 checkSurround = new Vector3(instantiationPoint.x, instantiationPoint.y + 6.5f, instantiationPoint.z);
                    for (int i = 0; i < raycastCount; i++)
                    {
                        float angle = i * angleStep;
                        Vector3 checkDirection = Quaternion.Euler(0, angle, 0) * Vector3.right;
                        if (Physics.Raycast(checkSurround, checkDirection, out hit, 20f, LayerMask.GetMask("Ground"))) continue;

                    }
                    
                    // Check if any houses are in the way
                    float distanceToWell = Vector3.Distance(center, checkSurround);
                    Vector3 difference = center - checkSurround;
                    Vector3 toCenter = difference.normalized;
                    if (Physics.Raycast(checkSurround, toCenter, out hit, distanceToWell - 5f, LayerMask.GetMask("House"))) continue;

                    Vector3 euler = finalRotation.eulerAngles;
                    euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                    euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
                    if (euler.x > minSlope && euler.x < maxSlope && euler.z > minSlope && euler.z < maxSlope)
                    {
                        instantiationPoint = new Vector3(instantiationPoint.x, instantiationPoint.y - 1.5f, instantiationPoint.z);
                        Collider[] surroundingColliders = Physics.OverlapSphere(instantiationPoint, 50f, LayerMask.GetMask("House"));
                        //print(scList[j]+" "+surroundingColliders.Length + surroundingColliders[0].name);
                        if (surroundingColliders.Length == 0)
                        {
                            float distance = Vector3.Distance(instantiationPoint, center);
                            if (distance >= minDistance)
                            {
                                //print("---------" + instantiationPoint + " " + UnityEngine.RandomPos);
                                GameObject spawnedObj = Instantiate(prefab, instantiationPoint, finalRotation);
                                spawnedObj.name = "House " + j.ToString() + spawned.ToString();
                                spawnedObj.AddComponent<SphereCollider>();
                                spawnedObj.GetComponent<SphereCollider>().radius = 50f;
                                spawnedObj.GetComponent<SphereCollider>().isTrigger = true;
                                pathIteration.Add(instantiationPoint);
                                spawned++;
                                if (spawned == count) pathList.Add(pathIteration);
                            }
                        }
                    }
                }
                iterations++;
            }
        }
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
        startAgent.CalculatePath(end, calcPath);
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


        //InstantiatedListOfPaths.Add(InstantiatedPath);

        // Create a new line renderer component
    
    lineRenderer.startWidth = 5f;
    lineRenderer.endWidth = 5f;
    lineRenderer.positionCount = steps1 + 1;


    for (int j = 0; j <= steps1; j++)
    {
        float t = (float)j / (steps1 - 1);
        Vector3 point = CalculateBezierPoint(t, corners);

        RaycastHit hit;
        if (Physics.Raycast(point + Vector3.up * 1000f, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            point.y = hit.point.y;
            lineRenderer.SetPosition(j, point);

            // Set the rotation of the line renderer to match the ground normal
            Vector3 up = hit.normal;
            Vector3 right = Vector3.Cross(point, up);
            lineRenderer.transform.rotation = Quaternion.LookRotation(point, up) * Quaternion.FromToRotation(Vector3.up, right);
        }
    }


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

}





