using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bridgeCreator : MonoBehaviour
{
    // // Start is called before the first frame update
    // public GameObject bridgePrefab;
    // private GameObject mapGen;

    // private List<Vector3> pathList = new List<Vector3>();

    // private int count = 0;

    // // Update is called once per frame
    // void Update()
    // {
    //     if (count == 0)
    //     {
    //         count++;
    //         mapGen = GameObject.Find("MapGenerator");
    //         pathList = mapGen.GetComponent<SingleTerrainGen>().InstantiatedPath;
    //         RaycastHit hit;
    //         for (int i = 0; i<pathList.Count; i++)
    //         {
    //             if (Physics.Raycast(pathList[i], Vector3.up, out hit, Mathf.Infinity, LayerMask.GetMask("Water"))){
    //                 GameObject bridge = Instantiate(bridgePrefab, hit.point, Quaternion.identity);
    //                 bridge.transform.parent.po = pathList[i];
    //             }
    //         }


    //     }
    // }
}
