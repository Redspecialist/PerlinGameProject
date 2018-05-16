using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{

    public float uniformScale = 1f;
	public float mapHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool useFallOff;

    public float minHeight
    {
        get
        {
            return uniformScale * mapHeightMultiplier * meshHeightCurve.Evaluate(0)/1f;
        }
    }
    public float maxHeight
    {
        get
        {
            return uniformScale * mapHeightMultiplier * meshHeightCurve.Evaluate(1)/1f;
        }
    }
}
