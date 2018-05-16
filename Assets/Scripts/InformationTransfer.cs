using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationTransfer : MonoBehaviour {

    public NoiseData terrainNoise;
    public NoiseData biomeNoise;
    public NoiseData biodiversityNoise;
    public NoiseData islandNoise;


    public int HexStringToInt(string s)
    {
        return int.Parse(s, System.Globalization.NumberStyles.HexNumber);
    }

    public void SetTerrainNoise(string s)
    {
        int set = s.Length > 0 ? HexStringToInt(s) : Random.Range(int.MinValue, int.MaxValue);
        terrainNoise.seed = set;
    }
    public void SetBiomeNoise(string s)
    {
        int set = s.Length > 0 ? HexStringToInt(s) : Random.Range(int.MinValue, int.MaxValue);
        biomeNoise.seed = set;
        biodiversityNoise.seed = set + 1;
    }
    public void SetIslandNoise(string s)
    {
        int set = s.Length > 0 ? HexStringToInt(s) : Random.Range(int.MinValue,int.MaxValue);
        islandNoise.seed = set;
    }

    public void SetBirdModel(int i)
    {
        AssetInfo.setCharacterNumber(i);
    }
}
