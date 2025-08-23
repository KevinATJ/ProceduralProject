using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DS_PCG_Script : MonoBehaviour
{
    private readonly int _size;
    private readonly float _roughness;
    private readonly float[,] _heightmap;

    public DS_PCG_Script(int size, float roughness)
    {
        _size = size;
        _roughness = roughness;
        _heightmap = new float[size, size];
    }

    public void GenerateHeightmap()
    {
        InitialiseCorners();

        int max = _size - 1;
        float randRange = 1f;
        DiamondSquare(0, 0, max, max, randRange);
    }

    private void InitialiseCorners()
    {
        _heightmap[0, 0] = Random.value;
        _heightmap[0, _size - 1] = Random.value;
        _heightmap[_size - 1, 0] = Random.value;
        _heightmap[_size - 1, _size - 1] = Random.value;
    }

    private void DiamondSquare(int x0, int y0, int x1, int y1, float randRange)
    {
        int half = (x1 - x0) / 2;
        if (half < 1) return;

        int midX = x0 + half;
        int midY = y0 + half;

        float avg = (_heightmap[y0, x0] + _heightmap[y0, x1] + _heightmap[y1, x0] + _heightmap[y1, x1]) / 4f;
        _heightmap[midY, midX] = avg + (Random.value * 2 - 1) * randRange;

        SetSquare(midX, y0, _heightmap[y0, x0], _heightmap[y0, x1], randRange);
        SetSquare(midX, y1, _heightmap[y1, x0], _heightmap[y1, x1], randRange);
        SetSquare(x0, midY, _heightmap[y0, x0], _heightmap[y1, x0], randRange);
        SetSquare(x1, midY, _heightmap[y0, x1], _heightmap[y1, x1], randRange);

        float newRandRange = randRange * Mathf.Pow(0.5f, _roughness);

        DiamondSquare(x0, y0, midX, midY, newRandRange);
        DiamondSquare(midX, y0, x1, midY, newRandRange);
        DiamondSquare(x0, midY, midX, y1, newRandRange);
        DiamondSquare(midX, midY, x1, y1, newRandRange);
    }

    private void SetSquare(int x, int y, float corner1, float corner2, float randRange)
    {
        float val = (corner1 + corner2) / 2f + (Random.value * 2 - 1) * randRange;

        val = Mathf.Clamp01(val);

        _heightmap[y, x] = val;
    }

    public float[,] GetHeightMap()
    {
        return _heightmap;
    }
}
