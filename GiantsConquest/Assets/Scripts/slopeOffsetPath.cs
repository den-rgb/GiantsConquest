
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slopeOffsetPath : MonoBehaviour
{
    private GameObject mapGen;

    private List<GameObject> pathList = new List<GameObject>();

    private Vector3 placeHolder;

    private int count = 0;

    // Update is called once per frame
    void Update()
    {
        // if (count == 0)
        // {
        //     count++;
        //     mapGen = GameObject.Find("MapGenerator");
        //     pathList = mapGen.GetComponent<SingleTerrainGen>().InstantiatedPath;
        //     RaycastHit hit;
        //     for (int i = 0; i < pathList.Count; i++)
        //     {
        //         placeHolder = pathList[i].transform.position;
        //         placeHolder.y += 100f;
        //         if (Physics.Raycast(placeHolder, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        //         {
        //             Vector3 normal = hit.normal;
        //             Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        //             Vector3 euler = rotation.eulerAngles;
        //             //converting euler angles to degrees
        //             euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
        //             euler.z = (euler.z > 180) ? euler.z - 360 : euler.z;
        //             print(euler.z + " " + euler.x + "------" + pathList[i].name);
        //             if (euler.x < -15f || euler.x > 15f)
        //             {
        //                 Vector3 direction = Vector3.Cross(normal, Vector3.right);
        //                 RaycastHit hitX;
        //                 if (Physics.Raycast(hit.point, direction, out hitX, Mathf.Infinity, LayerMask.GetMask("Ground")))
        //                 {
        //                     pathList[i].transform.position = hitX.point;
        //                 }
        //             }
        //             if (euler.z < -15f || euler.z > 15f)
        //             {
        //                 Vector3 direction = Vector3.Cross(normal, Vector3.forward);
        //                 RaycastHit hitZ;
        //                 if (Physics.Raycast(hit.point, direction, out hitZ, Mathf.Infinity, LayerMask.GetMask("Ground")))
        //                 {
        //                     pathList[i].transform.position = hitZ.point;
        //                 }
        //             }
        //         }
        //     }
        // }

    }
}
