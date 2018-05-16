using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeData : UpdatableData
{

    

    [Range(0,1)]
    public float overallDensity;
    public TextureData texture;
    public TerrainData terrain;


    public AssetGroupData[] biomeGroups;
    

    static GameObjectData nonce = new GameObjectData();


    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

    }
#endif

    public float lerp(float low, float high, float queryPos)
    {
        return Mathf.Clamp01((queryPos - low) / (high - low));
    }

    public AssetGroup weightedDiceRoleGroups(AssetGroup[] groups, float totalDensity, int numGroups, System.Random rand)
    {
        float roll = Range(rand, 0, totalDensity);

        for (int i = 0; i < numGroups; i++)
        {
            roll -= groups[i].density;
            if (roll <= 0)
            {
                return groups[i];
            }
        }
        return null;
    }

    public static float Range(System.Random rand, float start,float end)
    {
        return (float)rand.NextDouble() * (end - start) + start;
    }

    public Asset weightedDiceRoleAsset(Asset[] assets, float totalDensity, System.Random rand)
    {
        float roll = Range(rand,0, totalDensity);
        for (int i = 0; i < assets.Length; i++)
        {
            roll -= assets[i].probability;
            if (roll <= 0)
            {
                return assets[i];
            }
        }
        return assets[assets.Length-1];
    }

    public PreFabData generatePrefabMap(Vector3[] meshCoordinates, MapData map, float maxHeight, float minHeight)
    {
        System.Random rand = new System.Random();
        
        nonce.placeable = false;
        int dim = (int)Mathf.Floor(Mathf.Sqrt(meshCoordinates.Length));
        GameObjectData[,] coordinateBasedPlacements = new GameObjectData[dim, dim];
        bool[,,] acceptableAssets = new bool[dim, dim, biomeGroups.Length];

        

        //initializes placement permission to true for all objects at each position
        for (int i = 1; i < dim; i++)
        {
            for (int j = 1; j < dim; j++)
            {
                for (int k = 0; k < biomeGroups.Length; k++)
                {
                    acceptableAssets[i, j, k] = true;
                }
            }
        }
       

        for (int k = 0; k < biomeGroups.Length; k++)
        {
           biomeGroups[k].representative.setIndex(k);
        }

        int total = 0;
        //Looks at each coordinate and attempts to instantiate a random Asset
        for (int i = 1; i < dim-1; i++)
        {
            for (int j = 1; j < dim-1; j++)
            {
                //Finds the percentage based height
                float height = lerp(minHeight, maxHeight, meshCoordinates[i * dim + j].y);
                AssetGroup[] options = new AssetGroup[biomeGroups.Length];

                int validGroups = 0;
                float totalDensity = 0;

                //loads all options available at the specified heights
                for (int k = 0; k < biomeGroups.Length; k++)
                {

                    if (height < biomeGroups[k].representative.end && height > biomeGroups[k].representative.start)
                    {

                        if (acceptableAssets[i, j, k])
                        {
                            options[validGroups] = biomeGroups[k].representative;
                            totalDensity += biomeGroups[k].representative.density;
                            validGroups++;
                        }
                        

                    }

                }
                float seed = Range(rand, 0, 1f);
            
                if (seed < overallDensity && validGroups != 0)
                {
                    AssetGroup group = weightedDiceRoleGroups(options, totalDensity, validGroups, rand);
                    if(group == null)
                    {
                        coordinateBasedPlacements[i, j] = nonce;
                        
                    }
                    else
                    {
                        float totalProbability = 0;
                        for (int k = 0; k < group.assets.Length; k++)
                        {
                            totalProbability += group.assets[k].probability;
                        }
                        if(weightedDiceRoleAsset(group.assets, totalProbability, rand).orientAndPlace(i, j, height, coordinateBasedPlacements, acceptableAssets, group.getIndex() , group.claustraphobia, rand, nonce))
                        {
                            total++;
                        }
                    }
                }
                else
                {
                    coordinateBasedPlacements[i, j] = nonce;
                }
            }
        }


        GameObjectData[] ret = new GameObjectData[total];
        float distance = (meshCoordinates[1].x - meshCoordinates[0].x);
        float radius = distance / 4;
        float offset = radius / distance;
        int curr = 0;
        int wrong = 0;
        float magnification = (float)map.heightMap.GetLength(0) / dim;
        AnimationCurve curve = new AnimationCurve(terrain.meshHeightCurve.keys);
        
        for(int i = 1; i < dim; i++)
        {
            for(int j = 1; j < dim; j++)
            {
                GameObjectData temp = coordinateBasedPlacements[i, j];
                if (temp!= null && temp.placeable)
                {
                    float angle = Range(rand, 0, 360f);

                    int exZ = (int)((temp.position.x - offset * Mathf.Sin(angle)) * magnification) + 2;
                    int exX = (int)((temp.position.z + offset * Mathf.Sin(angle)) * magnification) + 2;

                    float z = (temp.position.x - dim / 2) ;
                    float x = (temp.position.z - dim / 2) ;
                   
                    temp.position.z = (z) * distance * -1 + radius * Mathf.Sin(angle);
                    temp.position.x = (x) * distance + radius * Mathf.Cos(angle);

                    

                    float tempX = (dim/2 - (temp.position.x / distance ) * magnification) + 2;
                    float tempZ = ((temp.position.z / distance + dim / 2) * magnification) + 2;
                    
                    int iz = Mathf.Clamp((int)tempX, 0, map.heightMap.GetLength(0) - 1);
                    int ix = Mathf.Clamp((int)tempZ, 0, map.heightMap.GetLength(1) - 1);
                    iz = exZ;
                    ix = exX;

                    float y = curve.Evaluate(Mathf.Min(map.heightMap[ix, iz], map.heightMap[ix + 1, iz], map.heightMap[ix + 1, iz + 1], map.heightMap[ix, iz + 1]));
                    
                    temp.position.y = y * terrain.mapHeightMultiplier;//temp.position.y * (maxHeight - minHeight) + minHeight;
                    ret[curr] = temp;
                    
                    curr++;
                }
                else
                {
                    wrong++;
                }
                
            }
        }
        /*
        GameObject piece = ret[0].gamePiece;
        ret = new GameObjectData[map.heightMap.Length];
        float half = map.heightMap.GetLength(0) / 2;
        for (int i = 0; i < map.heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < map.heightMap.GetLength(1); j++)
            {

                GameObjectData toStore = new GameObjectData();
                toStore.placeable = true;
                toStore.position = new Vector3(i - half, curve.Evaluate(map.heightMap[i, j]) * terrain.mapHeightMultiplier, half - j);
                toStore.gamePiece = piece;
                toStore.rotation = Quaternion.identity;
                toStore.scale = Vector3.one;
                ret[i * map.heightMap.GetLength(0) + j] = toStore;
            }
        }
        */
        PreFabData dataRet = new PreFabData();
        
        dataRet.prefabs = ret;

        return dataRet;
    }


    

    
}

public class GameObjectData
{
    public GameObject gamePiece;
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
    public bool placeable;
}

public class PreFabData
{
    public GameObjectData[] prefabs;
}