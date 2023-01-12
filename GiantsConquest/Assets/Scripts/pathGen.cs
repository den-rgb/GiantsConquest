using UnityEngine;
using System.Collections.Generic;
public class pathGen : MonoBehaviour
{
    // Generates a list of slightly bendy paths on top of a given surface, visiting all of the specified endpoints
    public static List<Vector3[]> GeneratePaths(Vector3[] vertices, int numPaths, float pathLength, float bendiness, List<Transform> endpoints, float stepSize)
    {
        // Initialize an empty list to store the generated paths
        var paths = new List<Vector3[]>();

        // Generate numPaths paths
        for (int i = 0; i < numPaths; i++)
        {
            // Choose a random starting point for the path
            var startIndex = Random.Range(0, vertices.Length);
            var startPos = vertices[startIndex];

            // Initialize a list to store the points in the path
            var points = new List<Vector3>();

            // Generate the points for the path
            for (int j = 0; j < endpoints.Count; j++)
            {
                // Choose the current endpoint
                var endPos = endpoints[j].position;

                // Calculate the direction to the endpoint
                var direction = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x);

                // Generate points for the path to the endpoint
                while (Vector3.Distance(startPos, endPos) > 0.1f)
                {
                    // Bend the path slightly by adding some noise to the direction
                    direction += Random.Range(-bendiness, bendiness);

                    // Calculate the next point in the path using the current direction and the specified step size
                    var step = new Vector3(Mathf.Cos(direction), Mathf.Sin(direction), 0) * stepSize;
                    var nextPos = startPos + step;

                    // Add the point to the list of points
                    points.Add(nextPos);

                    // Update the starting position for the next point
                    startPos = nextPos;
                }
            }

            // Add the generated path to the list of paths
            paths.Add(points.ToArray());
        }

        return paths;
    }

}
