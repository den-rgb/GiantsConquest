



using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




public class SingleTerrainGen : MonoBehaviour
{
    public GameObject terrainToSpawn;
    public GameObject well;
    public GameObject villageHouse;
    private MeshCollider terrainCollider;
    public float maxHeight;
    public float minHeight;
    public float maxSlope;
    public float minSlope;
    private RaycastHit hit;
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

    public List<GameObject> InstantiatedPath = new List<GameObject>();

    public void Start()
    {

        GameObject spawned = Instantiate(terrainToSpawn, new Vector3(0, 0, 0), Quaternion.identity);
        MapGenerator script = spawned.GetComponent<MapGenerator>();
        float height = Random.Range(150, 300);
        float noise = Random.Range(1700, 2100);
        float noise_2 = Random.Range(3600, 4500);
        int rand = Random.Range(1, 2);
        if (rand == 1)
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
        Mesh m = spawned.GetComponent<MeshCollider>().sharedMesh;
        m.RecalculateBounds();
        spawned.transform.DetachChildren();
        WaterGenerator waterScript = FindObjectOfType<WaterGenerator>();
        GameObject water = waterScript.gameObject;
        water.transform.position = new Vector3(0, -150, 0);


        // /////////////////////////////////////////////////////////////////////////////////////////
        /// NavMesh
        //// /////////////////////////////////////////////////////////////////////////////////////////
        navMeshSurface = spawned.AddComponent<NavMeshSurface>();
        navMeshSurface.overrideVoxelSize = true;
        navMeshSurface.voxelSize = 1;
        NavMeshBuildSettings buildSettings = navMeshSurface.GetBuildSettings();
        buildSettings.agentSlope = 25;
        navMeshSurface.BuildNavMesh();

        /// /////////////////////////////////////////////////////////////////////////////////////////
        /// center spawn 
        ///////////////////////////////////////////////////////////////////////////////////////////// 

        terrainCollider = spawned.GetComponent<MeshCollider>();
        int numberOfObjects = 3;
        Vector3 terrainSize = spawned.GetComponentInChildren<MeshRenderer>().bounds.size;

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Generate a random position within the terrain's bounds
            Vector3 position = new Vector3(Random.Range(0, terrainSize.x), 1000, Random.Range(0, terrainSize.z));
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

        generatePath(agentList[0], agentList[1].transform.position);






        // List<Vector3> corners = new List<Vector3>();
        // for (int i = 0; i < calcPath.corners.Length - 1; i++)
        // {
        //    Vector3 start = calcPath.corners[i];
        //    Vector3 end = calcPath.corners[i + 1];
        //    float distance = Vector3.Distance(start, end);

        //    // Calculate intermediate control points
        //    Vector3 dir = (end - start).normalized;
        //    Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
        //    Vector3 up = Vector3.up;
        //    // Calculate intermediate control points
        //    int numControlPoints = 4; // Increase the number of control points
        //    Vector3[] controlPoints = new Vector3[numControlPoints];
        //    controlPoints[0] = start;
        //    controlPoints[numControlPoints - 1] = end;
        //    for (int j = 1; j < numControlPoints - 1; j++)
        //    {
        //        float t = (float)j / (numControlPoints - 1);
        //        Vector3 prevDir = (controlPoints[j - 1] - start).normalized;
        //        Vector3 prevRight = Vector3.Cross(prevDir, Vector3.up).normalized;
        //        Vector3 currRight = right;
        //        if (j == numControlPoints - 2)
        //        {
        //            currRight = Vector3.Cross(dir, prevRight).normalized;
        //        }
        //        // Add some randomness to the control point positions
        //        float randomScale = 0.5f;
        //        controlPoints[j] = start + dir * distance * t
        //            + currRight * distance * (Random.Range(-1f, 1f) * randomScale + j * randomScale)
        //            + up * distance * (Random.Range(-1f, 1f) * randomScale + j * randomScale);
        //    }
        //     for( int s =0; s<controlPoints.Length; s++){
        //     corners.Add(controlPoints[s]);
        //     }
        //    // Generate points along Bezier curve with random offsets
        //    int steps = Mathf.RoundToInt(distance / 7f);
        //    for (int j = 0; j <= steps; j++)
        //    {

        //        float t = (float)j / steps;
        //        Vector3 point = CalculateBezierPoint(t, corners);
        //        point.y = 1000f;



        //     //    // Add a small random offset to each point in a direction perpendicular to the tangent
        //     //    float offsetScale = 0.5f;
        //     //    Vector3 tangent = CalculateBezierPoint(t, corners);
        //     //    Vector3 offset = Vector3.Cross(tangent, Vector3.up).normalized * (Random.Range(-1f, 1f) * offsetScale);
        //     //    point += offset;

        //        if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        //        {
        //            point.y = hit.point.y;
        //            Instantiate(mark, point, Quaternion.identity);
        //        }
        //    }
        // }







        // /////////////////////////////////////////////////////////////////////////////////////////
        // // surrounding houses spawn
        // /////////////////////////////////////////////////////////////////////////////////////////
        spawnSurroundingObjects(5, 50, 1, villageHouse);


        // /////////////////////////////////////////////////////////////////////////////////////////
        // // Generate path
        // /////////////////////////////////////////////////////////////////////////////////////////
        for (int s = 0; s < pathList.Count; s++)
        {
            Vector3 startPos = new Vector3(scList[s].bounds.center.x, scList[s].bounds.center.y + 100, scList[s].bounds.center.z);
            if (Physics.Raycast(startPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                startPos.y = hit.point.y;

                float segmentLength = 7f; // the desired length of each segment
                Vector3 offset;
                for (int i = 0; i < pathIteration.Count; i++)
                {
                    Vector3 endPos = pathList[s][i];
                    float distance = Vector3.Distance(startPos, endPos);
                    int numSegments = Mathf.FloorToInt(distance / segmentLength);
                    float sectionLength = distance / numSegments;
                    Vector3 difference = endPos - startPos;
                    Vector3 direction = difference.normalized;

                    for (int j = 0; j < numSegments - 2; j++)
                    {
                        Vector3 sectionPoint = startPos + direction * sectionLength * j;
                        if (distance < 100f) offset = Random.onUnitSphere * 2;
                        else offset = Random.onUnitSphere * 3;
                        sectionPoint += offset;
                        GameObject chosenRock = rocks[Random.Range(0, rocks.Length)];
                        Vector3 centerObj = chosenRock.transform.localScale / 2f;
                        Vector3 instantiationPoint = sectionPoint + centerObj;
                        Vector3 sectionPointUp = instantiationPoint + new Vector3(0, 50, 0);
                        if (Physics.Raycast(sectionPointUp, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                        {
                            instantiationPoint.y = hit.point.y;
                            Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            Collider[] stoneColliders = Physics.OverlapSphere(instantiationPoint, 2f, LayerMask.GetMask("stone"));
                            Collider[] wellColliders = Physics.OverlapSphere(instantiationPoint, 2f, LayerMask.GetMask("well"));
                            Collider[] houseColliders = Physics.OverlapSphere(instantiationPoint, 2f, LayerMask.GetMask("House"));
                            if (stoneColliders.Length == 0 && wellColliders.Length == 0 && houseColliders.Length == 0)
                            {
                                Instantiate(chosenRock, instantiationPoint, rotationNormal);
                            }
                        }
                        else numSegments++;
                    }
                }
            }
        }
    }



    public void spawnSurroundingObjects(int count, float minDistance, float radiusChange, GameObject prefab)
    {
        for (int j = 0; j < scList.Count; j++)
        {
            int spawned = 0;
            int iterations = 0;
            pathIteration = new List<Vector3>();
            while (spawned < count && iterations < 1000)
            {
                Vector3 randomPos = scList[j].bounds.center + Random.insideUnitSphere * (scList[j].radius / radiusChange);
                randomPos.y = 1000f;
                if (Physics.Raycast(randomPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")) && (hit.point.y < maxHeight && hit.point.y > minHeight))
                {
                    int raycastCount = 360;
                    float angleStep = 360f / raycastCount;
                    Vector3 centeObj = prefab.transform.localScale / 2f;
                    Vector3 instantiationPoint = randomPos + centeObj;
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
                                //print("---------" + instantiationPoint + " " + randomPos);
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
        calcPath = new NavMeshPath();
        startAgent.CalculatePath(end, calcPath);
        Vector3 start = startAgent.gameObject.transform.position;

        int numCorners = calcPath.corners.Length;

        List<Vector3> corners = new List<Vector3>();
        float dist1 = Vector3.Distance(start, end);
        int steps1 = Mathf.RoundToInt(dist1 / 7f);
        int steps = 0;
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
        for (int j = 0; j <= steps1; j++)
        { //change to steps
            float t = (float)j / (steps1 - 1);
            Vector3 point = CalculateBezierPoint(t, corners);
            point.y = 1000f;

            if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                point = hit.point;
                Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
                GameObject c = Instantiate(red, point, rotationNormal);
                c.name = red.name + j.ToString();
                InstantiatedPath.Add(c);

            }
        }

        for (int j = 0; j <= steps1; j++)
        {
            float t = (float)j / (steps1 - 1);
            Vector3 point = Vector3.Lerp(calcPath.corners[0], calcPath.corners[calcPath.corners.Length - 1], t);
            point.y = 1000f;

            if (Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                point = hit.point;
                Quaternion rotationNormal = Quaternion.FromToRotation(Vector3.up, hit.normal);
                GameObject c = Instantiate(blue, point, rotationNormal);

            }
        }
    }

}





