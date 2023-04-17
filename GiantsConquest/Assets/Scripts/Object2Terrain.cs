using UnityEngine;
using System.Collections.Generic;

public class Object2Terrain : MonoBehaviour
{
    public int resolution = 2000;

	public Gradient gradient;
	
    public void CreateTerrainChunks(GameObject ground, Texture2D originalTexture)
	{
		int terrainCountX = 4;
		int terrainCountZ = 5;
		int totalTerrainCount = terrainCountX * terrainCountZ;

		TerrainData[] terrains = new TerrainData[totalTerrainCount];
		GameObject[] terrainObjects = new GameObject[totalTerrainCount];
		MeshCollider collider = ground.GetComponent<MeshCollider>();

		Texture2D[] chunkTextures = new Texture2D[totalTerrainCount];
		int chunkSizeX = originalTexture.width / terrainCountX;
		int chunkSizeZ = originalTexture.height / terrainCountZ;
			

		for (int i = 0; i < totalTerrainCount; i++)
		{
			
			int chunkOffsetX = (i % terrainCountX) * chunkSizeX;
			int chunkOffsetZ = (i / terrainCountX) * chunkSizeZ;
			chunkTextures[i] = new Texture2D(chunkSizeX, chunkSizeZ);
			chunkTextures[i].SetPixels(originalTexture.GetPixels(chunkOffsetX, chunkOffsetZ, chunkSizeX, chunkSizeZ));
			chunkTextures[i].Apply();
		}
		
		// Add a collider to our source object if it does not exist.
		// Otherwise raycasting doesn't work.
		if (!collider)
		{
			collider = ground.AddComponent<MeshCollider>();
		}

		for (int i = 0; i < totalTerrainCount; i++)
		{
			terrains[i] = new TerrainData();
			terrains[i].heightmapResolution = resolution;
			terrains[i].size = new Vector3(collider.bounds.size.x / terrainCountX, collider.bounds.size.y, collider.bounds.size.z / terrainCountZ);
			terrainObjects[i] = Terrain.CreateTerrainGameObject(terrains[i]);
			terrainObjects[i].name = "Chunk" + i.ToString();
			gradient = GameObject.Find("Mesh2Terrain").GetComponent<TerrainColour>().gradient;
			


			Vector3 position = new Vector3(
				((i % terrainCountX) * terrains[i].size.x + terrains[i].size.x / 2) ,
				0,
				Mathf.Floor((i / terrainCountX) * terrains[i].size.z + terrains[i].size.z / 2) 
			);
			terrainObjects[i].transform.position = position;

			Bounds bounds = new Bounds(position, new Vector3(terrains[i].size.x, terrains[i].size.y, terrains[i].size.z));
			float[,] heights = new float[terrains[i].heightmapResolution, terrains[i].heightmapResolution];
			float meshHeightInverse = 1f / bounds.size.y;

			int maxHeight = heights.GetLength(0);
			int maxLength = heights.GetLength(1);
			
			for (int z = 0; z < maxHeight; z++)
			{
				for (int x = 0; x < maxLength; x++)
				{
					Vector3 rayOrigin = new Vector3(
						bounds.min.x + x / (float)terrains[i].heightmapResolution * bounds.size.x,
						bounds.max.y + bounds.size.y,
						bounds.min.z + z / (float)terrains[i].heightmapResolution * bounds.size.z
					);
					Ray ray = new Ray(rayOrigin, -Vector3.up);
					RaycastHit hit;

					if (collider.Raycast(ray, out hit, bounds.size.y * 3))
					{
						float meshYOffset = terrainObjects[i].transform.position.y;
						heights[z, x] = (hit.point.y - meshYOffset) * meshHeightInverse;
						
					}

				}
			}
			terrains[i].SetHeights(0, 0, heights);
			terrains[i].alphamapResolution = 2000;
			Material mat = new Material(Shader.Find("Standard"));
			mat.SetFloat("_Glossiness", 0.0f);
			terrainObjects[i].GetComponent<Terrain>().materialTemplate = mat;
			// Apply the texture to the terrain's material
			terrainObjects[i].GetComponent<Terrain>().materialTemplate.mainTexture = chunkTextures[i];
			}

		if (collider)
		{
			DestroyImmediate(collider);
		}
		
	}


	public void CreateTerrain(GameObject ground)
    {
        TerrainData terrain = new TerrainData();
        terrain.heightmapResolution = resolution;
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrain);
		terrainObject.name = "Terrain0";
        MeshCollider collider =  ground.GetComponent<MeshCollider>();
        CleanUp cleanUp = null;

        //Add a collider to our source object if it does not exist.
        //Otherwise raycasting doesn't work.
        // if (!collider)
        // {
		// 	print("destroyed collider");
        //     collider = gameObject.AddComponent<MeshCollider>();
        //     cleanUp = () => DestroyImmediate(collider);
        // }

        Bounds bounds = collider.bounds;	
		float sizeFactor = collider.bounds.size.y / (collider.bounds.size.y);
		terrain.size = collider.bounds.size;
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
					//height += shiftHeight;
 
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