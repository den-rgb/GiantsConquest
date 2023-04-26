using UnityEngine;

public class LOD : MonoBehaviour {
    public GameObject playerCamera;
    public Renderer objectRenderer;
    public float lod0Distance = 10f;
    public float lod1Distance = 50f;

    private Material objectMaterial;

    void Start() {
        playerCamera = GameObject.FindWithTag("MainCamera");
        objectRenderer = GetComponent<Renderer>();
        objectMaterial = objectRenderer.material;
    }

    void Update() {
        if (objectMaterial.shader.name != "Custom/grassLOD") {
            Debug.LogError("The object must use the 'grassLOD' shader.");
            return;
        }

        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        objectMaterial.SetFloat("_LOD0Distance", lod0Distance);
        objectMaterial.SetFloat("_LOD1Distance", lod1Distance);
    }
}
