using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject knightspawner;
    private List<List<GameObject>> pathList = new List<List<GameObject>>();
    private List<GameObject> setPath = new List<GameObject>();
    
    private Animator anim;
    private NavMeshAgent agent;
    private GameObject mapGen;

    public bool takenDamage;
    private GameObject player;
    public SphereCollider spearCollider;

    public int pathNum = 0;
    int count = 0;
    bool combat = false;
    int i = 11;
    int sphereRadius = 50;
    float elapsedTime = 0;

    void Start(){
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(count==0){
            player = GameObject.FindGameObjectWithTag("Player");
            mapGen = GameObject.Find("MapGenerator");
            pathList = mapGen.GetComponent<SingleTerrainGen>().InstantiatedListOfPaths;
            print(pathNum+" pathnum" + "--------" + pathList.Count);
            setPath = pathList[pathNum];
            print(setPath.Count+"---Count");
            agent.SetDestination(setPath[i].transform.position);
            count++;
        }

        if (!combat)
        {
            if (Vector3.Distance(transform.position, setPath[i].transform.position) < 7f && i < setPath.Count - 1)
            {
                i++;
                agent.SetDestination(setPath[i].transform.position);
                agent.gameObject.transform.LookAt(setPath[i].transform.position);
            }
        }
        else
        {
            agent.SetDestination(player.transform.position);

            if (agent.remainingDistance < 7f)
            {
                agent.isStopped = true;
                anim.SetBool("attack", true);

                // Check if knight_step_back animation is playing
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Knight_step_back"))
                {
                    // Move the gameobject perpendicular to the path in the opposite direction
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, player.transform.position - transform.position).normalized;
                    Vector3 targetPosition = transform.position - perpendicular * 3f;
                    while (elapsedTime < 0.5f)
                    {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / 0.5f);
                    elapsedTime += Time.deltaTime;
                    }
                }


                // Check if knight_step animation is playing
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Knight_step"))
                {
                    // Move the gameobject perpendicular to the path
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, setPath[i].transform.position - transform.position).normalized;
                    Vector3 targetPosition = transform.position + perpendicular * 3f;
                    while (elapsedTime < 0.5f)
                    {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / 0.5f);
                    elapsedTime += Time.deltaTime;
                    }
                    
                }
                float rnd = Random.Range(0, 15);
                anim.SetFloat("step", rnd);
            }
            else
            {
                anim.SetBool("attack", false);
                agent.isStopped = false;
            }
        }


        playerFound();
    }

    void playerFound(){
        // Check for player within sphere collider
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereRadius);
        foreach (Collider hitCollider in hitColliders) {
            if (hitCollider.CompareTag("Player")) {
                combat = true;
                return;
            }
        }

        // Check for player with raycast
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 100f)) {
            if (hitInfo.collider.CompareTag("Player")) {
                combat = true;
                return;
            }
        }
        
    }
}
