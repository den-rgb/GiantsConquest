using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGen : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> objectsToSpawn;
    public List<GameObject> villagesToSpawn;
    public GameObject Grid;

    void Start()
    {
        Vector3 gridSize = Grid.GetComponentInChildren<MeshRenderer>().bounds.size;
        Vector3 position;
        int resourceAmount = 100;
        int villageAmount = 10;

        for (int i = 0; i <= villageAmount; i++)
        {
            do
            {

                position = new Vector3(
                Random.Range((-Grid.transform.position.x - gridSize.x) / 2, (Grid.transform.position.x + gridSize.x) / 2),
                0,
                Random.Range((-Grid.transform.position.z - gridSize.z) / 2, (Grid.transform.position.z + gridSize.z) / 2)
                );
                print("Finding position");
            } while (Physics.CheckSphere(position, 1.0f, LayerMask.GetMask("Spawned")));

            // Choose a random object to spawn from the list
            int randomIndex = Random.Range(0, villagesToSpawn.Count);
            GameObject objectToSpawn = villagesToSpawn[randomIndex];

            Instantiate(objectToSpawn, position, Quaternion.identity);

        }

        for (int i = 0; i <= resourceAmount; i++)
        {
            do
            {

                position = new Vector3(
                Random.Range((-Grid.transform.position.x - gridSize.x) / 2, (Grid.transform.position.x + gridSize.x) / 2),
                0,
                Random.Range((-Grid.transform.position.z - gridSize.z) / 2, (Grid.transform.position.z + gridSize.z) / 2)
                );
                print("Finding position");
            } while (Physics.CheckSphere(position, 1.0f, LayerMask.GetMask("Spawned")));

            // Choose a random object to spawn from the list
            int randomIndex = Random.Range(0, objectsToSpawn.Count);
            GameObject objectToSpawn = objectsToSpawn[randomIndex];

            Instantiate(objectToSpawn, position, Quaternion.identity);
            
        }


    }
}
