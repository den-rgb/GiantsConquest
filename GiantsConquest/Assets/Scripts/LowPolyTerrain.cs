
using UnityEngine;

public class LowPolyTerrain : MonoBehaviour
{
    public int width = 10;
    public int length = 10;
    public float aplification = 1f;
    public float frequency = 0.3f;
    public Material material;

    void Start()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

        // Generate terrain mesh
        Mesh mesh = new Mesh();
        mesh.name = "Terrain Mesh";

        Vector3[] vertices = new Vector3[(width + 1) * (length + 1)];
        for (int i = 0, z = 0; z <= length; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float y = Mathf.PerlinNoise(x * frequency, z * frequency) * aplification;
                float y2 = 0.1f;
                y = (y + y2) / 2f;
                vertices[i] = new Vector3(x, y, z);
            }
        }

        int[] triangles = new int[width * length * 6];
        for (int ti = 0, vi = 0, z = 0; z < length; z++, vi++)
        {
            for (int x = 0; x < width; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;


        // Add low-poly terrain material
        meshRenderer.material = material;

        meshCollider.sharedMesh = mesh;
    }
}

