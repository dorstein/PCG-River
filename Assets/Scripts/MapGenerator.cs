using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour {

    public enum DrawMode
    {
        NoiseMap, ColorMap, Terrain
    };

    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;
   

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public float meshHeightMultiplier;
    public AnimationCurve heightCurve;

    public int seed;
    public Vector2 offset;

    public TerrainType[] regions;


    public bool autoupdate;


	private Queue<mapThreadInfo<mapData>> mapDataThreadInfoQueue = new Queue<mapThreadInfo<mapData>>();
	private Queue<mapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<mapThreadInfo<MeshData>>();


    public void drawMapInEditor()
    {
        mapData mapData = generateMapData();

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.drawTexture(TextureGenerator.textureFromHeightMap(mapData.heightmap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.drawTexture(TextureGenerator.textureFromColorMap(mapData.colormap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Terrain)
        {
            display.genMesh(MeshGenerator.generateTerrainMesh(mapData.heightmap, meshHeightMultiplier, heightCurve, levelOfDetail),
				TextureGenerator.textureFromColorMap(mapData.colormap, mapChunkSize, mapChunkSize));
        }
    }


    public void requestMapData(Action<mapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            mapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    void mapDataThread(Action<mapData> callback)
    {
        mapData mapData = generateMapData();
		lock (mapDataThreadInfoQueue) 
		{
			mapDataThreadInfoQueue.Enqueue (new mapThreadInfo<mapData> (callback, mapData));
		}
    }

	public void requestMeshData(mapData mapdata, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate {
			meshDataThread (mapdata, callback);
		};
		new Thread (threadStart).Start ();
	}

	void meshDataThread(mapData mapdata, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.generateTerrainMesh (mapdata.heightmap, meshHeightMultiplier, heightCurve, levelOfDetail);
		lock(meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue (new mapThreadInfo<MeshData> (callback, meshData));
		}
	}

	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				mapThreadInfo<mapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}

		if(meshDataThreadInfoQueue.Count > 0)
		{
			for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				mapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}

	}


    private mapData generateMapData()
    {
        float[,] noiseMap = Noise.generateNoiseMap(mapChunkSize, mapChunkSize,seed, noiseScale, octaves,persistance,lacunarity,offset);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];


        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i =0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }

            }
        }

        return new mapData(noiseMap, colorMap);
        
    }






    private void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;
		if (noiseScale < 0)
			noiseScale = 0;
		

    }



    struct mapThreadInfo<T>
    {
		public readonly Action<T> callback;
		public readonly T parameter;

        public mapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter; 
        }
    }


}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color colour;
}



public struct mapData
{
	public readonly float[,] heightmap;
	public readonly Color[] colormap;

	public mapData (float[,] heightmap, Color[] colormap)
	{
		this.colormap = colormap;
		this.heightmap = heightmap;
	}

}
