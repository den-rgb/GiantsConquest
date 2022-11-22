
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRandom : MonoBehaviour
{
    public Transform terrain;
    public BoxCollider bc;
    private Vector3 randPos;
    float x, z;
    int treeSize, rockSize, villageSize;
    public GameObject tree;
    public GameObject rock ;
    public GameObject village;
    float xDim;
    float zDim;
    // Start is called before the first frame update
    void Start()
    {
        treeSize = 101;
        rockSize = 51;
        villageSize = 11;
        xDim = bc.bounds.size.x;
        zDim = bc.bounds.size.z;
        
        
        for(int t=0; t<treeSize; t++){
            x = Random.Range(-xDim/2,xDim/2);
            z = Random.Range(-zDim/2,zDim/2);
            randPos = new Vector3(x,2,z);
            GameObject pref = Instantiate(tree, randPos, Quaternion.identity) as GameObject;
            pref.transform.localScale = new Vector3(1, 4, 1);
            Destroy(gameObject);
        }

        for(int r=0; r<rockSize; r++){
            x = Random.Range(-xDim/2,xDim/2);
            z = Random.Range(-zDim/2,zDim/2);
            randPos = new Vector3(x,1,z);
            GameObject pref = Instantiate(rock, randPos, Quaternion.identity) as GameObject;
            pref.transform.localScale = new Vector3(2, 1, 4);
            Destroy(gameObject);
        }

        for(int v=0; v<villageSize; v++){
            x = Random.Range(-xDim/2,xDim/2);
            z = Random.Range(-zDim/2,zDim/2);
            randPos = new Vector3(x,5,z);
            GameObject pref = Instantiate(village, randPos, Quaternion.identity) as GameObject;
            pref.transform.localScale = new Vector3(10, 10, 10);
            Destroy(gameObject);
        }
    } 
}


