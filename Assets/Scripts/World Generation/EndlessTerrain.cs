using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    const float viewerMoveThresholdForChunUpdate = 25f;
    const float sqrViewerMoveThresholdForChunUpdate = viewerMoveThresholdForChunUpdate * viewerMoveThresholdForChunUpdate;
    const float colliderGenerationDistanceThreshold = 5;
    Vector2 oldViewerPosition;

    public int colliderLODIndex;



    [Range(1,4)]
    public int PrefabLODIndex;
    public float PrefabRenderDistance;


    public static float maxViewDist;


    public LODInfo[] detailLevels;
    public BiomeHub biomes;
    public NoiseData biomeSelection;
    public NoiseData secondBiomeSelector;
    

    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDist;
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    static MapGenScript mapGenerator;


    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenScript>();
        chunkSize = mapGenerator.mapChunkSize - 1;
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
        terrainChunkDictionary.Clear();
        visibleTerrainChunks.Clear();
        UpdateVisibleChunks();
    }

    private void Update()
    {

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPosition != oldViewerPosition)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.updateCollisionMesh();
            }
        }

        if((oldViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunUpdate)
        {
            oldViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
        
    }

    public static BiomeData getBiome(float x, float y, BiomeHub biomes, NoiseData selector, NoiseData secondarySelector)
    {
        float biomeTypeNum = selector.SamplePosition(x, y);
        float diversityNum = secondarySelector.SamplePosition(x, y);
        //BiomeData ret = biomes.biomes[biomes.biomes.Length-1].biome;

        GroupBiomeDataWeight data = biomes.biomeSelection2D[0];

        for (int i = biomes.biomeSelection2D.Length-1; i >= 0; i--)
        {
            if(biomeTypeNum >= biomes.biomeSelection2D[i].weight)
            {
                data = biomes.biomeSelection2D[i];
                break;
            }
        }
        if(data == null)
        {
            data = biomes.biomeSelection2D[0];
        }
        for (int i = data.biome.Length - 1; i >= 0; i--)
        {
            if (biomeTypeNum >= data.biome[i].weight)
            {
                return data.biome[i].biome;
                
            }
        }
        return data.biome[0].biome;
    }

    void UpdateVisibleChunks()
    {
        
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        
        for(int i = visibleTerrainChunks.Count-1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }
    
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for(int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, PrefabLODIndex, PrefabRenderDistance, transform, getBiome(viewedChunkCoord.x,viewedChunkCoord.y, biomes, biomeSelection,secondBiomeSelector)));
                    }
                }
                
                
            }

        }
    }

    public class TerrainChunk
    {
        public Vector2 coord;

        Vector2 position;
        GameObject meshObject;
        GameObject prefabParent;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        BiomeData biome;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;
        int prefabLODIndex;

        bool hasSetCollider;
        bool hasPrefabs;

        public float PrefabRenderDistance;

        MapData mapData;
        bool mapDataReceived;
        int previousLODindex = -1;

        public TerrainChunk(Vector2 coord, int size,LODInfo[] detailLevels, int colliderLODIndex, int prefabLODIndex, float prefabRenderDistance, Transform parent, BiomeData biome)
        {
            this.biome = biome;
            this.PrefabRenderDistance = prefabRenderDistance;
            this.prefabLODIndex = prefabLODIndex;
            this.coord = coord;
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);


            this.colliderLODIndex = colliderLODIndex;
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            

            meshRenderer.material = biome.texture.material;
            meshFilter = meshObject.AddComponent<MeshFilter>();
            

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            prefabParent = new GameObject("Prefab Parent");
            prefabParent.transform.parent = meshObject.transform;
            prefabParent.transform.localPosition = Vector3.zero;

            

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {


                lodMeshes[i] = new LODMesh(detailLevels[i].lod, i == prefabLODIndex);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if(i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += updateCollisionMesh;
                }
                
            }

            mapGenerator.RequestMapData(position, biome, OnMapDataRecieved);
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
           
            meshFilter.mesh = meshData.CreateMesh();
            
        }

        void OnMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        void OnPrefabDataRecieved(PreFabData prefabData)
        {
            
            this.hasPrefabs = true;
            
            foreach (GameObjectData data in prefabData.prefabs)
            {
                GameObject myNewPrefab = Instantiate(data.gamePiece, prefabParent.transform);
                myNewPrefab.transform.localPosition = data.position;
                myNewPrefab.transform.localRotation = data.rotation;
                myNewPrefab.transform.localScale = new Vector3(myNewPrefab.transform.localScale.x * data.scale.x, myNewPrefab.transform.localScale.y * data.scale.y, myNewPrefab.transform.localScale.z * data.scale.z);
                
            }

            return;


        }


        public void UpdateTerrainChunk()
        {
            
            if (!mapDataReceived)
            {
                return;
            }
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDistFromNearestEdge <= maxViewDist;
            if (visible)
            {
                int lodIndex = 0;
                for(int i = 0; i < detailLevels.Length-1; i++)
                {
                    if(viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                if(lodIndex != previousLODindex)
                {
                    
                    LODMesh mesh = lodMeshes[lodIndex];
                    if (mesh.hasMesh)
                    {
                        previousLODindex = lodIndex;
                        meshFilter.mesh = mesh.mesh;
                        meshCollider.sharedMesh = mesh.mesh;

                    } else if (!mesh.hasRequestedMesh)
                    {
                        mesh.RequestMesh(mapData);
                    }
                }
                if(viewerDistFromNearestEdge < PrefabRenderDistance)
                {
                    if (!hasPrefabs)
                    {
                        LODMesh mesh = lodMeshes[prefabLODIndex];
                        if (!mesh.hasMesh)
                        {
                            mesh.RequestMesh(mapData);
                        }
                        else
                        {
                            mapGenerator.RequestPrefabData(mesh.meshCoords, mapData, biome,OnPrefabDataRecieved);
                        }

                    }
                    
                    prefabParent.SetActive(true);
                    
                }
                else
                {
                    prefabParent.SetActive(false);
                }
                
            }

            if(wasVisible != visible)
            {
                if (visible)
                {
                    
                    visibleTerrainChunks.Add(this);
                }
                else
                {
                    visibleTerrainChunks.Remove(this);
                }
            }
            SetVisible(visible);
        }

        public void updateCollisionMesh()
        {
            if (!hasSetCollider)
            {
                float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
                {
                    if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                    {
                        lodMeshes[colliderLODIndex].RequestMesh(mapData);
                    }
                }

                if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
                {
                    if (lodMeshes[colliderLODIndex].hasMesh)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }
            }
            

        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public Vector3[] meshCoords;
        public bool hasRequestedMesh;
        public bool hasMesh;
        bool storeCoordData;
        int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod, bool storeCoordData)
        {
            this.lod = lod;
            this.storeCoordData = storeCoordData;

        }
        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            if (storeCoordData)
            {
                meshCoords = meshData.getPositions();
            }
            
            hasMesh = true;
            updateCallback();
        }
        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0,MeshGenerator.numSupportedLODs-1)]
        public int lod;
        public float visibleDistThreshold;
        public float sqrVisibleDstThreshold
        {
            get
            {
                return visibleDistThreshold * visibleDistThreshold;
            }
        }

    }
}
