using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainConfig
{
    public float heightScale;
    public float roughness;
    public float minHeight;
    public float maxHeight;
    public float waterLevel;
    public float sandLevel;
    public float grassLevel;

    public TerrainConfig(float heightScale, float roughness, float minHeight, float maxHeight, float waterLevel, float sandLevel, float grassLevel)
    {
        this.heightScale = heightScale;
        this.roughness = roughness;
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
        this.waterLevel = waterLevel;
        this.sandLevel = sandLevel;
        this.grassLevel = grassLevel;
    }
}
