using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    // The player object
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player object to the player variable
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player object is colliding with this object
        if (player.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
        {
            // Set the kinematic state of all children of this object to false
            foreach (Transform child in transform)
            {
                child.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}






