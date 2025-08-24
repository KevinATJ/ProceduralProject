using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DS_PCG_Script_Rec
{
    private readonly int _size;
    private readonly float _roughness;
    private readonly float[,] _heightmap;

    public DS_PCG_Script_Rec(int size, float roughness)
    {
        _size = size;
        _roughness = roughness;
        _heightmap = new float[size, size];
    }

    public void GenerateHeightmap()
    {
        InitialiseCorners();
        int max = _size - 1;
        float initialRange = 1f;

        DiamondSquare(0, 0, max, max, initialRange);
    }

    private void InitialiseCorners()
    {
        float mid = 0.5f;
        _heightmap[0, 0] = Random.Range(mid - 0.1f, mid + 0.1f);
        _heightmap[0, _size - 1] = Random.Range(mid - 0.1f, mid + 0.1f);
        _heightmap[_size - 1, 0] = Random.Range(mid - 0.1f, mid + 0.1f);
        _heightmap[_size - 1, _size - 1] = Random.Range(mid - 0.1f, mid + 0.1f);
    }

    private void DiamondSquare(int x0, int y0, int x1, int y1, float randRange)
    {
        int half = (x1 - x0) / 2;
        if (half < 1) return;

        int midX = x0 + half;
        int midY = y0 + half;

        float diamondAvg = (_heightmap[y0, x0] + _heightmap[y0, x1] +
                            _heightmap[y1, x0] + _heightmap[y1, x1]) * 0.25f;
        _heightmap[midY, midX] = Mathf.Clamp01(diamondAvg + RandomNoise(randRange));

        SetEdge(midX, y0, _heightmap[y0, x0], _heightmap[y0, x1], randRange); 
        SetEdge(midX, y1, _heightmap[y1, x0], _heightmap[y1, x1], randRange); 
        SetEdge(x0, midY, _heightmap[y0, x0], _heightmap[y1, x0], randRange); 
        SetEdge(x1, midY, _heightmap[y0, x1], _heightmap[y1, x1], randRange);

        float newRand = randRange * (1f-_roughness);

        DiamondSquare(x0, y0, midX, midY, newRand);
        DiamondSquare(midX, y0, x1, midY, newRand);
        DiamondSquare(x0, midY, midX, y1, newRand);
        DiamondSquare(midX, midY, x1, y1, newRand);
    }

    private void SetEdge(int x, int y, float a, float b, float randRange)
    {
        if (_heightmap[y, x] != 0f) return; // no sobreescribir
        _heightmap[y, x] = Mathf.Clamp01((a + b) * 0.5f + RandomNoise(randRange));
    }

    private float RandomNoise(float range)
    {
        return (Random.value * 2f - 1f) * range;
    }

    public float[,] GetHeightMap()
    {
        return _heightmap;
    }
}

