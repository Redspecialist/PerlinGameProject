using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacurnarity, Vector2 offset, float rotation, AnimationCurve curve)
    {

        AnimationCurve curveCopy = new AnimationCurve(curve.keys);
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float[] cosTurns = new float[octaves];
        float[] sinTurns = new float[octaves];

        float maxPossibleHeight = 0;
        float tempAmp = 1;
        float thetaInc = rotation / 180 * Mathf.PI;
        float theta = 0;
        for (int i = 0; i < octaves; i++)
        {
            cosTurns[i] = Mathf.Cos(theta);
            sinTurns[i] = Mathf.Sin(theta);
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
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
                
                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.5f);
                
                noiseMap[x, y] = curveCopy.Evaluate(normalizedHeight);
            }
        }

        return noiseMap;
    }

    public static float getNoiseAtPoint(float x, float y, int seed, float scale, int octaves, float persistance, float lacurnarity, Vector2 offset, float rotation, AnimationCurve curve)
    {

        AnimationCurve curveCopy = new AnimationCurve(curve.keys);
        
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float[] cosTurns = new float[octaves];
        float[] sinTurns = new float[octaves];

        float maxPossibleHeight = 0;
        float tempAmp = 1;
        float thetaInc = rotation / 180 * Mathf.PI;
        float theta = 0;
        for (int i = 0; i < octaves; i++)
        {
            cosTurns[i] = Mathf.Cos(theta);
            sinTurns[i] = Mathf.Sin(theta);
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            theta += thetaInc;
            maxPossibleHeight += tempAmp;
            tempAmp *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + octaveOffsets[i].x) / scale * frequency;
            float sampleY = (y + octaveOffsets[i].y) / scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX * cosTurns[i] - sampleY * sinTurns[i], sampleY * cosTurns[i] + sampleX * sinTurns[i]) * 2 - 1;


            noiseHeight += perlinValue * amplitude;
            amplitude *= persistance;
            frequency *= lacurnarity;
        }

        
        float normalizedHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.5f);

        return curveCopy.Evaluate(normalizedHeight);
    }

}