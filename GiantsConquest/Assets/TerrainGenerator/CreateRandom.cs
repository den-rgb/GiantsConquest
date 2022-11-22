
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRandom : MonoBehaviour
{
    public Transform terrain;
    public BoxCollider bc;
    private Vector3 randPos;
    float x, z;
    public List<GameObject> toSpawn = new List<GameObject>();
    float xDim;
    float zDim;
    // Start is called before the first frame update
    void Start()
    {
        xDim = bc.bounds.size.x;
        zDim = bc.bounds.size.z;
        
        
        for(int i=0; i<toSpawn.Count; i++){
            x = Random.Range(-xDim/2,xDim/2);
            z = Random.Range(-zDim/2,zDim/2);
            randPos = new Vector3(x,1,z);
            GameObject pref = Instantiate(toSpawn[i], randPos, Quaternion.identity) as GameObject;
            pref.transform.localScale = new Vector3(1, 1, 1);
            Destroy(gameObject);
        }
    } 
}


