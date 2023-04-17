using System.Data.Common;

using UnityEngine;
using System.Collections.Generic;
public class occlusionCulling : MonoBehaviour
{

    public Transform player; // Reference to the player's transform

    private List<GameObject> activeObjects = new List<GameObject>(); // List of currently active chunks
    int count;

    public GameObject[] chunks;



    void Start()
    {
        count = 0;
    }

    private void Update()
    {
        if(count==0){
            chunks  = GameObject.FindGameObjectsWithTag("Chunk");
            player = GameObject.FindGameObjectWithTag("Player").transform;
            count++;
        }
        // Loop through all chunks
        foreach (GameObject chunk in chunks)
        {
            // If the chunk is within the player's view distance
            if (Vector3.Distance(player.position, chunk.transform.position) < 1000)
            {
                // If the chunk is not already active
                if (!chunk.activeSelf)
                {
                    // Activate the chunk
                    chunk.SetActive(true);
                    // Add the chunk to the list of active chunks
                    activeObjects.Add(chunk);
                }
            }
            // If the chunk is outside the player's view distance
            else
            {
                // If the chunk is active
                if (chunk.activeSelf)
                {
                    // Deactivate the chunk
                    chunk.SetActive(false);
                    // Remove the chunk from the list of active chunks
                    activeObjects.Remove(chunk);
                }
            }
        }
    }
}