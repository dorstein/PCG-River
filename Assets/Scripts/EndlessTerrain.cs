using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    public const float maxViewDst = 300;
    public Transform viewer;
    public Transform parent;
	public Material mapMaterial;

    public static Vector2 viewPosition;
    int chunkSize, chunksVisivleInViewDst;
	static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
		mapGenerator = FindObjectOfType<MapGenerator> ();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisivleInViewDst = Mathf.RoundToInt( maxViewDst / chunkSize);
        
    }

    private void Update()
    {
        viewPosition = new Vector2(viewer.position.x, viewer.position.y);
        updateVisibleChunks();
    }

    void updateVisibleChunks()
    {

        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].setVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewPosition.y / chunkSize);


        for (int yOffset = -chunksVisivleInViewDst; yOffset <= chunksVisivleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisivleInViewDst; xOffset <= chunksVisivleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].updateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, parent, mapMaterial));
                }
            }

        }
    }


    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            setVisible(false);

			mapGenerator.requestMapData(onMapDataReceived);
        }

		void onMapDataReceived(mapData mapData)
		{
			mapGenerator.requestMeshData (mapData, onMeshDataReceived);
		}

		void onMeshDataReceived(MeshData meshData)
		{
			meshFilter.mesh = meshData.createMesh ();
		}

        public void updateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            setVisible(visible);
        }

        public void setVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }

    }


}




