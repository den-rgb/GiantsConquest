

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightFollowPath : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject mapGen;
    private GameObject knight;
    private NavMeshAgent agent;
    private NavMeshAgent mapAgentPos;
    public GameObject knightPrefab;

    private List<GameObject> pathList = new List<GameObject>();
    int count = 0;
    int i = 11;
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(count==0){
            mapGen = GameObject.Find("MapGenerator");
            pathList = mapGen.GetComponent<SingleTerrainGen>().InstantiatedPath;
            mapAgentPos = mapGen.GetComponent<SingleTerrainGen>().agentList[0];
            RaycastHit hit;
            if(Physics.Raycast(mapAgentPos.transform.position, Vector3.down, out hit)){
                knight = Instantiate(knightPrefab, mapAgentPos.transform.position, Quaternion.identity);
                agent = knight.GetComponent<NavMeshAgent>();
                agent.SetDestination(pathList[11].transform.position);
            }
            // print(pathList.Count);
            count++;
            
        }


        //print(Vector3.Distance(knight.transform.position, pathList[i].transform.position));
        if (Vector3.Distance(knight.transform.position, pathList[i].transform.position) < 20f && i < pathList.Count - 1)
        {
            i++;
            // print(i);
            agent.SetDestination(pathList[i].transform.position);
        }


    }
}
