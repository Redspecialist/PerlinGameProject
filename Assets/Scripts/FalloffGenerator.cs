using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalloffGenerator : MonoBehaviour
{
    
    public float scale;
    public int octaves;
    public int seed;

    [Range(0, 1)]
    public float persistance;
    public float lacurnarity;

    public Vector2 offset;
    public float rotation;

    public AnimationCurve islandHeightCurve;


    public float[,] GenrateFalloffMap(int mapWidth, int mapHeight, Vector2 position)
    {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float[] cosTurns = new float[octaves];
        float[] sinTurns = new float[octaves];

        float maxPossibleHeight = 0;
        float tempAmp = 1;
        float thetaInc = rotation / 180 * Mathf.PI;
        float theta = 0;


        Vector2 displacement = offset + position;
        for (int i = 0; i < octaves; i++)
        {
            cosTurns[i] = Mathf.Cos(theta);
            sinTurns[i] = Mathf.Sin(theta);
            float offsetX = prng.Next(-100000, 100000) + displacement.x;
            float offsetY = prng.Next(-100000, 100000) - displacement.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            theta += thetaInc;
            maxPossibleHeight += tempAmp;
            tempAmp *= persistance;
        }



        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float halfwidth = mapWidth / 2f;
        float halfheight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfwidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfheight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX * cosTurns[i] - sampleY * sinTurns[i], sampleY * cosTurns[i] + sampleX * sinTurns[i]) * 2 - 1;


                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacurnarity;
                }

                noiseMap[x, y] = noiseHeight;

            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                
                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.5f);
                noiseMap[x, y] = normalizedHeight;
            }
        }

        return noiseMap;
    }
    public static float Evaluate(float value)
    {
        float a = 3f;
        float b = 3f;
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(a - b * value, a));
    }




}
