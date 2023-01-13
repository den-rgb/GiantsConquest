using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pathRenderer : MonoBehaviour
{
    public Material material;
    public float width = 0.1f;
    Vector3[] vertices = { new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(0, 10, 0) };
    public int stepSize = 5;
    public List<Transform> endpoints;

    private void Start()
    {
        // Generate some test paths
        var paths = pathGen.GeneratePaths(vertices, 5, 100, 0.1f, endpoints,stepSize);

        // Render the paths
        foreach (var path in paths)
        {
            // Create a mesh to represent the path
            var mesh = new Mesh();

            // Create an array of vertices for the mesh
            var meshVertices = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                meshVertices[i] = path[i];
            }

            // Set the vertices of the mesh
            mesh.vertices = meshVertices;

            // Create an array of triangles for the mesh
            var meshTriangles = new int[(path.Length - 1) * 6];
            for (int i = 0; i < path.Length - 1; i++)
            {
                meshTriangles[i * 6 + 0] = i;
                meshTriangles[i * 6 + 1] = i + 1;
                meshTriangles[i * 6 + 2] = i + 2;
                meshTriangles[i * 6 + 3] = i + 2;
                meshTriangles[i * 6 + 4] = i + 1;
                meshTriangles[i * 6 + 5] = i + 3;
            }

            // Set the triangles of the mesh
            mesh.triangles = meshTriangles;

            // Create an array of normals for the mesh
            var meshNormals = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                meshNormals[i] = Vector3.up;
            }

            // Set the normals of the mesh
            mesh.normals = meshNormals;

            // Create a game object to hold the mesh
            var line = new GameObject("Line");
            line.transform.parent = transform;

            // Add a mesh filter and mesh renderer to the game object
            line.AddComponent<MeshFilter>().mesh = mesh;
            line.AddComponent<MeshRenderer>().material = material;
        }
    }
}
