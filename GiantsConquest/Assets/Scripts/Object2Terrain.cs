using UnityEngine;

public class Object2Terrain : MonoBehaviour
{
    public int resolution = 512;
    public Vector3 addTerrain;
    public int bottomTopRadioSelected = 0;
    public static string[] bottomTopRadio = new string[] { "Bottom Up", "Top Down" };
    public float shiftHeight = 0f;

    public void CreateTerrain(GameObject ground)
    {
        TerrainData terrain = new TerrainData();
        terrain.heightmapResolution = resolution;
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrain);

        MeshCollider collider =  ground.GetComponent<MeshCollider>();
        CleanUp cleanUp = null;

        //Add a collider to our source object if it does not exist.
        //Otherwise raycasting doesn't work.
        if (!collider)
        {
			print("destroyed collider");
            collider = gameObject.AddComponent<MeshCollider>();
            cleanUp = () => DestroyImmediate(collider);
        }

        Bounds bounds = collider.bounds;	
		float sizeFactor = collider.bounds.size.y / (collider.bounds.size.y + addTerrain.y);
		terrain.size = collider.bounds.size + addTerrain;
		bounds.size = new Vector3(terrain.size.x, collider.bounds.size.y, terrain.size.z);
 
		// Do raycasting samples over the object to see what terrain heights should be
		float[,] heights = new float[terrain.heightmapResolution, terrain.heightmapResolution];	
		Ray ray = new Ray(new Vector3(bounds.min.x, bounds.max.y + bounds.size.y, bounds.min.z), -Vector3.up);
		RaycastHit hit = new RaycastHit();
		float meshHeightInverse = 1 / bounds.size.y;
		Vector3 rayOrigin = ray.origin;
 
		int maxHeight = heights.GetLength(0);
		int maxLength = heights.GetLength(1);
 
		Vector2 stepXZ = new Vector2(bounds.size.x / maxLength, bounds.size.z / maxHeight);
 
		for(int zCount = 0; zCount < maxHeight; zCount++){
 
			for(int xCount = 0; xCount < maxLength; xCount++){
 
				float height = 0.0f;
 
				if(collider.Raycast(ray, out hit, bounds.size.y * 3)){
 
					height = (hit.point.y - bounds.min.y) * meshHeightInverse;
					height += shiftHeight;
 
					//bottom up
	                height *= sizeFactor;
					
 
					//clamp
					if(height < 0){
 
						height = 0;
					}
				}
 
				heights[zCount, xCount] = height;
           		rayOrigin.x += stepXZ[0];
           		ray.origin = rayOrigin;
			}
 
			rayOrigin.z += stepXZ[1];
      		rayOrigin.x = bounds.min.x;
      		ray.origin = rayOrigin;
		}
 
		terrain.SetHeights(0, 0, heights);
 
		if(cleanUp != null){
 
			cleanUp();    
		}
    }

    delegate void CleanUp();
}