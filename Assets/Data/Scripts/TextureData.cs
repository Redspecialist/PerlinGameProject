﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[CreateAssetMenu()]
public class TextureData : UpdatableData {

    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;
    public Layer[] layers;
    public Material material;
    public string nameID;

    float saveMinHeight;
    float saveMaxHeight;

    public void ApplyToMaterial() {

        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColors", layers.Select(x=> x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture as Texture2D).ToArray());
        material.SetTexture("baseTextures", texturesArray);
        UpdateMeshHeights(saveMinHeight, saveMaxHeight);
    }
    public void UpdateMeshHeights(float minHeight, float maxHeight)
    {
        saveMinHeight = minHeight;
        saveMaxHeight = maxHeight;
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);


    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        
        Texture2DArray textureArray = new Texture2DArray(textureSize,textureSize,textures.Length, textureFormat,true);
        for(int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;

    }

    [System.Serializable]
    public class Layer
    {

        public Texture texture;
        public Color tint;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float tintStrength;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale;
    }
}
