using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{

    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacurnarity;

    public int seed;
    public Vector2 offset;
    public float turn;
    public AnimationCurve adjustmentCurve;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (lacurnarity < 1)
        {
            lacurnarity = 1;

        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        base.OnValidate();

    }
#endif

    public float SamplePosition(float x, float y)
    {
        return Noise.getNoiseAtPoint(x, y, seed, noiseScale, octaves, persistence, lacurnarity, offset, turn, adjustmentCurve);
    }

}
