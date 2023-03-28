using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject mapGen;
    private GameObject knight;
    private UnityEngine.AI.NavMeshAgent agent;
    private UnityEngine.AI.NavMeshAgent mapAgentPos;
    public GameObject knightPrefab;

    // private Animator anim;

    // public GameObject player;

    //private List<GameObject> pathList = new List<GameObject>();
    //public List<GameObject> knightList = new List<GameObject>();
    private int count = 0;
    // bool combat ;
    // int i = 11;
    // int sphereRadius = 50;
    // void Start()
    // {
    //     anim = GetComponent<Animator>();
    //     combat = false;
    // }

    // Update is called once per frame
    void Update()
    {
        
        
            if(count==0){
                mapGen = GameObject.Find("MapGenerator");
                //pathList = mapGen.GetComponent<SingleTerrainGen>().InstantiatedListOfPaths;
                for(int i=0; i<mapGen.GetComponent<SingleTerrainGen>().agentList.Count-1; i++){
                    mapAgentPos = mapGen.GetComponent<SingleTerrainGen>().agentList[i];
                    RaycastHit hit;
                    if(Physics.Raycast(mapAgentPos.transform.position, Vector3.down, out hit)){
                        knight = Instantiate(knightPrefab, mapAgentPos.transform.position, Quaternion.identity);
                        knight.GetComponent<KnightBehaviour>().pathNum = i;
                        //knightList.Add(knight);
                    }
                }
                
                
                //     agent.SetDestination(pathList[11].transform.position);
                // }
                // print(pathList.Count);
                count++;
                
            }

            //print(Vector3.Distance(knight.transform.position, pathList[i].transform.position));
            // if (Vector3.Distance(knight.transform.position, pathList[i].transform.position) < 20f && i < pathList.Count - 1)
            // {
            //     i++;
            //     // print(i);
            //     agent.SetDestination(pathList[i].transform.position);
            //     agent.gameObject.transform.LookAt(pathList[i].transform.position);
            // }
        // }else{
        //     agent.SetDestination(player.transform.position);

        //     if(agent.remainingDistance < 5f){
        //         agent.isStopped = true;
        //         anim.SetBool("Attack",true);
        //         float rnd = Random.Range(0, 15);
        //         anim.SetFloat("step", rnd);
        //     }else{
        //         anim.SetBool("Attack",false);
        //         agent.isStopped = false;
        //     }

        


    }


    // void playerFound(){
    //     // Check for player within sphere collider
    //     Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereRadius);
    //     foreach (Collider hitCollider in hitColliders) {
    //         if (hitCollider.CompareTag("Player")) {
    //             Debug.Log("Player detected using sphere collider!");
    //             combat = true;
    //             return;
    //         }
    //     }

    //     // Check for player with raycast
    //     RaycastHit hitInfo;
    //     if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 100f)) {
    //         if (hitInfo.collider.CompareTag("Player")) {
    //             Debug.Log("Player detected using raycast!");
    //             combat = true;
    //             return;
    //         }
    //     }
        
    // }

}
