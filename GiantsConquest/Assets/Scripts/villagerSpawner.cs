
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class villagerSpawner : MonoBehaviour
{
    public int villageNum;
    public GameObject villagerPrefab_f;
    public GameObject villagerPrefab_m;

    public List<GameObject> houseList = new List<GameObject>();
    public int numberOfVillagers;
    public float spawnRadius;
    int i =0;

    private void Update()
    {
        if(i<=numberOfVillagers){
            SpawnVillager();
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
            int randomVillager = Random.Range(0,2);
            if(randomVillager==1){
                GameObject v = Instantiate(villagerPrefab_f, new Vector3(navHit.position.x, navHit.position.y+ 10, navHit.position.z), Quaternion.identity);
                v.GetComponent<villager>().villageNum  = gameObject;
                v.GetComponent<villager>().villageHouseList = houseList;
                v.name = v.name + i + "_" + villageNum;
            }else{
                GameObject v = Instantiate(villagerPrefab_m, new Vector3(navHit.position.x, navHit.position.y+ 10, navHit.position.z), Quaternion.identity);
                v.GetComponent<villager>().villageNum  = gameObject;
                v.GetComponent<villager>().villageHouseList = houseList; 
                v.name = v.name + i + "_" + villageNum;
            }
        }
        else
        {
            Debug.LogWarning("Failed to place villager on NavMesh. Try increasing the spawn radius or ensuring the NavMesh is baked correctly.");
        }
    }
}
