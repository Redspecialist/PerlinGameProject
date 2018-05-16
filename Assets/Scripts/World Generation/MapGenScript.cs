using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenScript : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh, FalloffMap, SurroundingTiles}
    public DrawMode drawMode;



    [Range(0, MeshGenerator.numSupportedLODs - 1)]
    public int levelOfDetail;

    [Range(0, MeshGenerator.numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    public int mapChunkSize{
        get{
          return MeshGenerator.supportedChunkSizes[chunkSizeIndex]-1;
        }
    }

    public TerrainData terrainData;
    public NoiseData noiseData;
    public NoiseData falloffData;

    public BiomeData biomeData;
    public BiomeHub hub;
    public bool disabled;

    public bool autoUpdate;

    
    
    [Range(-180,180)]
    

    

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    Queue<MapThreadInfo<PreFabData>> prefabDataInfoQueue = new Queue<MapThreadInfo<PreFabData>>();

    public void Awake()
    {
        chunkSizeIndex = 0;
        for(int i = 0; i < hub.biomeSelection2D.Length; i++)
        {
            hub.biomeSelection2D[i].biome[0].biome.texture.ApplyToMaterial();
            hub.biomeSelection2D[i].biome[0].biome.texture.UpdateMeshHeights(terrainData.minHeight, terrainData.maxHeight);

        }

    }
    public void Start()
    {
        chunkSizeIndex = 0;
        for (int i = 0; i < hub.biomeSelection2D.Length; i++)
        {
            hub.biomeSelection2D[i].biome[0].biome.texture.ApplyToMaterial();
            hub.biomeSelection2D[i].biome[0].biome.texture.UpdateMeshHeights(terrainData.minHeight, terrainData.maxHeight);

        }

    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        
        biomeData.texture.ApplyToMaterial();
        biomeData.texture.UpdateMeshHeights(terrainData.minHeight, terrainData.maxHeight);
        DrawMapInEditor();
    }
    

    public void DrawMapInEditor()
    {
        biomeData.texture.UpdateMeshHeights(terrainData.minHeight, terrainData.maxHeight);

        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (disabled)
        {
            return;
        }
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        
        else if (drawMode == DrawMode.Mesh)
        {

            MeshData mesh = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.mapHeightMultiplier, terrainData.meshHeightCurve, 3, chunkSizeIndex);
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.mapHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail, chunkSizeIndex), biomeData.texture);
            display.DrawPrefabs(biomeData.generatePrefabMap(mesh.getPositions(), mapData, terrainData.maxHeight, terrainData.minHeight));

        }
        else if(drawMode == DrawMode.FalloffMap)
        {
            //display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenrateFalloffMap(mapChunkSize)));
        }
        else if(drawMode == DrawMode.SurroundingTiles)
        {
            Debug.Log(display);
            display.DestroyAllMeshData();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mapData = GenerateMapData(new Vector2((i-1) * mapChunkSize ,(j-1) * mapChunkSize));
                    
                    MeshData mesh = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.mapHeightMultiplier, terrainData.meshHeightCurve, 3, chunkSizeIndex);

                    display.DrawMeshUnderParentWithPrefabs(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.mapHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail, chunkSizeIndex), biomeData.generatePrefabMap(mesh.getPositions(), mapData, terrainData.maxHeight, terrainData.minHeight), biomeData.texture, new Vector2((i-1) * mapChunkSize, (j-1) * mapChunkSize));
                    
                }
            }

        }

    }

    public void SaveMesh()
    {
        print("Creating Mesh");
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        Mesh m = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.mapHeightMultiplier, terrainData.meshHeightCurve, levelOfDetail,chunkSizeIndex).CreateMesh();
        print("Saving mesh");
        display.MeshDump(m);
    }

    MapData GenerateMapData(Vector2 center)
    {
        
        float[,] noiseMap = Noise.GenerateNoiseMap(MeshGenerator.supportedChunkSizes[chunkSizeIndex] + 5, MeshGenerator.supportedChunkSizes[chunkSizeIndex] + 5, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistence, noiseData.lacurnarity, center + noiseData.offset, noiseData.turn, noiseData.adjustmentCurve);
        float[,] fallOffMap = Noise.GenerateNoiseMap(MeshGenerator.supportedChunkSizes[chunkSizeIndex] + 5, MeshGenerator.supportedChunkSizes[chunkSizeIndex] + 5, falloffData.seed, falloffData.noiseScale, falloffData.octaves, falloffData.persistence, falloffData.lacurnarity, center + falloffData.offset, falloffData.turn, falloffData.adjustmentCurve);

        for (int y = 0; y < MeshGenerator.supportedChunkSizes[chunkSizeIndex] + 5; y++)
        {
            for (int x = 0; x < MeshGenerator.supportedChunkSizes[chunkSizeIndex] + 5; x++)
            {
                if (terrainData.useFallOff)
                {

                    noiseMap[x, y] = Mathf.Clamp(noiseMap[x, y] - fallOffMap[x, y],0,1.5f);
                   
                }
                

            }
        }
        
        return new MapData(noiseMap,fallOffMap);

    }

    private void OnValidate()
    {
        if(terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;

        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;

        }

        if(biomeData.texture != null)
        {
            
            biomeData.texture.OnValuesUpdated -= OnTextureValuesUpdated;
            biomeData.texture.OnValuesUpdated += OnTextureValuesUpdated;
        }
        if (biomeData != null)
        {
            biomeData.OnValuesUpdated -= OnTextureValuesUpdated;
            biomeData.OnValuesUpdated += OnTextureValuesUpdated;
        }

    }

    public void RequestMapData(Vector2 center, BiomeData biome, Action<MapData> callback)
    {
        biome.texture.UpdateMeshHeights(terrainData.minHeight, terrainData.maxHeight);
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }
    private void Update()
    {

        if (mapDataThreadInfoQueue.Count > 0)
        {
            while (0 < mapDataThreadInfoQueue.Count)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {

            while (0 < meshDataThreadInfoQueue.Count)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (prefabDataInfoQueue.Count > 0)
        {
            while (0 < prefabDataInfoQueue.Count)
            {
                MapThreadInfo<PreFabData> threadInfo = prefabDataInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }

        }
    }
    public void RequestPrefabData(Vector3[] mesh,MapData map, BiomeData biome, Action<PreFabData> callback)
    {
        ThreadStart threadStart = delegate
        {
            PrefabDataThread(mesh,map, biome, callback);
        };
        new Thread(threadStart).Start();
    }

    public void PrefabDataThread(Vector3[] mesh, MapData map, BiomeData biome, Action<PreFabData> callback)
    {
        PreFabData data = biome.generatePrefabMap(mesh,map, terrainData.maxHeight, terrainData.minHeight);
        lock (prefabDataInfoQueue)
        {
            prefabDataInfoQueue.Enqueue(new MapThreadInfo<PreFabData>(callback, data));
        }
    }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }
    public void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.mapHeightMultiplier, terrainData.meshHeightCurve, lod, chunkSizeIndex);

        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }


    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;
        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly float[,] fallOffMap;
    public MapData(float[,] heightMap, float[,] fallOff)
    {
        this.heightMap = heightMap;
        this.fallOffMap = fallOff;
    }
}
