using UnityEngine;

public class ObjectVisibility : MonoBehaviour
{
    public float visibleRadius = 50f;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isVisible = distanceToPlayer <= visibleRadius;

        // If the object is within the visible radius, enable rendering, otherwise disable rendering
        GetComponent<Renderer>().enabled = isVisible;
    }
}