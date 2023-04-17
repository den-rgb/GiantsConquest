
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class villagerSpawner : MonoBehaviour
{
    public GameObject villagerPrefab_f;
    public GameObject villagerPrefab_m;
    public int numberOfVillagers;
    public float spawnRadius;
    int i =0;

    private void Update()
    {
        if(i<=numberOfVillagers){
            SpawnVillager();
            print(i + "spawn count");
            i++;
        }
    }

    private void SpawnVillager()
    {
        
        Vector3 randomPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        randomPosition.y = transform.position.y;
        UnityEngine.AI.NavMeshHit navHit;

        if (UnityEngine.AI.NavMesh.SamplePosition(randomPosition, out navHit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            int randomVillager = Random.Range(0,1);
            if(randomVillager==1){
                GameObject v = Instantiate(villagerPrefab_f, navHit.position, Quaternion.identity);
                v.GetComponent<villager>().villageNum  = gameObject;
            }else{
                GameObject v = Instantiate(villagerPrefab_m, navHit.position, Quaternion.identity);
                v.GetComponent<villager>().villageNum  = gameObject;
            }
        }
        else
        {
            Debug.LogWarning("Failed to place villager on NavMesh. Try increasing the spawn radius or ensuring the NavMesh is baked correctly.");
        }
       
    }
}
