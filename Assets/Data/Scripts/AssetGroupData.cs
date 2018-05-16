using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AssetGroupData : ScriptableObject {
    public AssetGroup representative;
    
}
[System.Serializable]
public class AssetGroup
{
    [Range(0, 8)]
    public int claustraphobia;

    [Range(0, 1)]
    public float start;

    [Range(0, 1)]
    public float end;

    [Range(0, 1)]
    public float density;

    public Asset[] assets;

    int index;

    public int getIndex()
    {
        return index;

    }
    public void setIndex(int i)
    {
        index = i;
    }

}

[System.Serializable]
public class Asset
{
    public GameObject prefab;

    [Range(0, 8)]
    public int placementRadius;

    [Range(1, 10)]
    public int minScaling;

    [Range(1, 10)]
    public int maxScaling;

    [Range(0, 1)]
    public float probability;

    public bool orientAndPlace(int xTL, int yTL, float height, GameObjectData[,] placements, bool[,,] rules, int index, int width, System.Random rand, GameObjectData nonce)
    {

        //oversight assumes left to right up to down placement
        int minX = Mathf.Min(placementRadius * maxScaling, placements.GetLength(1) - xTL);

        for (int i = 1; i <= minX; i++)
        {
            if (placements[i + xTL, yTL] != null)
            {
                minX = i;
            }
        }

        float minScale = placementRadius == 0 ? 1000000 : Mathf.Min((float)minX / placementRadius, (float)(placements.GetLength(0) - yTL) / placementRadius);

        if (minScale < minScaling)
        {
            placements[xTL, yTL] = nonce;

            return false;
        }


        float scale = BiomeData.Range(rand, minScaling, Mathf.Min(minScale, maxScaling));

        Vector3 position = new Vector3(((float)(xTL + placementRadius / 2)), height -1, ((float)(yTL + placementRadius / 2)));
        Vector3 scaling = new Vector3(scale, scale, scale);

        for (int i = xTL; i <= placementRadius; i++)
        {
            for (int j = xTL; j <= placementRadius; j++)
            {
                placements[i, j] = nonce;
            }
        }

        for (int i = Mathf.Max(xTL - width, 0); i < Mathf.Min(xTL + placementRadius * scale + width, placements.GetLength(0)); i++)
        {
            for (int j = Mathf.Max(yTL - width, 0); j < Mathf.Min(yTL + placementRadius * scale + width, placements.GetLength(0)); j++)
            {
                rules[i, j, index] = false;
            }
        }
        float angle = BiomeData.Range(rand, 0, 360f);

        placements[xTL, yTL] = new GameObjectData();
        placements[xTL, yTL].rotation = Quaternion.AngleAxis(angle, Vector3.up);
        placements[xTL, yTL].gamePiece = prefab;
        placements[xTL, yTL].placeable = true;
        placements[xTL, yTL].position = position;
        placements[xTL, yTL].scale = scaling;

        return true;
    }
}