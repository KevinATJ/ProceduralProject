using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainConfig
{
    public float heightScale = 20f;
    public float roughness = 0.7f;
    public float treePosMinHeight = 0.35f;
    public float treePosMaxHeight = 0.5f;
    public float worldWaterLevel = 0.25f;
    public float worldSandLevel = 0.35f;
    public float worldGrassLevel = 0.5f;
}
