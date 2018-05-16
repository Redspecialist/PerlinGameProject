using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeHub : MonoBehaviour {

    public BiomeDataWeight[] biomes;
    public GroupBiomeDataWeight[] biomeSelection2D;
}

[System.Serializable]
public class BiomeDataWeight
{
    [Range(0, 1)]
    public float weight;
    public BiomeData biome;
    public string name;
    
}

[System.Serializable]
public class GroupBiomeDataWeight
{
    
    [Range(0, 1)]
    public float weight;
    public string name;
    public BiomeDataWeight[] biome;
}