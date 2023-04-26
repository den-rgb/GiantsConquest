using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class villager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject villageNum;
    private Transform villageCenter;
    public float villageRadius = 500f;
    public float wanderRadius = 500f;
    public float timeBetweenMoves = 7f;

    public ParticleSystem fire;

    private GameObject player;

    public List<GameObject> villageHouseList = new List<GameObject>();

    private UnityEngine.AI.NavMeshAgent agent;
    private float timer;
    int count = 0;

    private void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = 5f;
        timer = timeBetweenMoves;
    }

    private void Update()
    {
        if (count == 0)
        {
            villageCenter = villageNum.transform;
            //print(villageNum.name + " name");
            count++;
        }
        timer += Time.deltaTime;

        // Find the relevant house
        GameObject relevantHouse = null;
        foreach (GameObject house in villageHouseList)
        {
            if (house.GetComponent<checkDestroyed>().destroyed)
            {
                relevantHouse = house;
                break;
            }
        }

        if (relevantHouse != null)
        {
            player = GameObject.FindWithTag("Player");
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Check if the player is within the wander radius
            if (distanceToPlayer <= wanderRadius)
            {
                if(fire!=null) fire.Play();
                
               // Debug.Log("Following player"  + gameObject.name);
                agent.SetDestination(player.transform.position);
                Vector3 targetPosition = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                gameObject.transform.LookAt(targetPosition);
            }else{
                if(fire!=null) fire.Stop();
                if (timer >= timeBetweenMoves)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, LayerMask.GetMask("Ground"), villageCenter.position, villageRadius);
                    agent.SetDestination(newPos);
                    gameObject.transform.LookAt(newPos);
                    //print(newPos + "new pos");
                    timer = 0;
                }
            }
        }
        else
        {
            if(fire!=null) fire.Stop();
            if (timer >= timeBetweenMoves)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, LayerMask.GetMask("Ground"), villageCenter.position, villageRadius);
                agent.SetDestination(newPos);
                gameObject.transform.LookAt(newPos);
                //print(newPos + "new pos");
                timer = 0;
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask, Vector3 villageCenter, float villageRadius)
    {
        Vector3 randomDirection;
        Vector3 potentialPosition;
        int maxAttempts = 2000;
        int currentAttempt = 0;

        do
        {
            randomDirection = Random.insideUnitSphere * distance;
            randomDirection.y = 0; // Ensure the movement is only in the XZ plane
            potentialPosition = origin + randomDirection;

            if (Vector3.Distance(potentialPosition, villageCenter) <= villageRadius)
            {
                UnityEngine.AI.NavMeshHit navHit;
                if (UnityEngine.AI.NavMesh.SamplePosition(potentialPosition, out navHit, villageRadius, UnityEngine.AI.NavMesh.AllAreas))
                {
                    //print("got position");
                    return navHit.position;
                }
            }
            currentAttempt++;
        } while (currentAttempt < maxAttempts);

        return origin; // Fallback to the origin if no valid position is found
    }

}
