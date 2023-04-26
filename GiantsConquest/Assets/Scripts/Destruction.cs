using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    // The player object
    private GameObject player;

    public checkDestroyed checkDestroyed;
    public bool isDestroyed;

    // Start is called before the first frame update
    void Start()
    {
        isDestroyed = false;
        // Set the player object to the player variable
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider that touched this object is the player
        if (other.gameObject == player)
        {
            // Set the kinematic state of all children of this object to false
            foreach (Transform child in transform)
            {
                child.GetComponent<Rigidbody>().isKinematic = false;
                checkDestroyed.destroyed = true;
                PlayerPrefs.SetInt("wood", PlayerPrefs.GetInt("wood") + 1);
                PlayerPrefs.SetInt("stone", PlayerPrefs.GetInt("stone") + 1);
                PlayerPrefs.Save();
            }
        }
    }
}






