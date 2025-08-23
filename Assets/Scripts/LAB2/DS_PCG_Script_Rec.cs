using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DS_PCG_Script_Rec : MonoBehaviour
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

        float diamondAvg = AverageCorners(x0, y0, x1, y1);
        _heightmap[midY, midX] = Mathf.Clamp01(diamondAvg + RandomNoise(randRange));

        SetEdge(midX, y0, half, randRange);
        SetEdge(midX, y1, half, randRange);
        SetEdge(x0, midY, half, randRange);
        SetEdge(x1, midY, half, randRange);

        float newRand = randRange * _roughness;

        DiamondSquare(x0, y0, midX, midY, newRand);
        DiamondSquare(midX, y0, x1, midY, newRand);
        DiamondSquare(x0, midY, midX, y1, newRand);
        DiamondSquare(midX, midY, x1, y1, newRand);
    }

    private float AverageCorners(int x0, int y0, int x1, int y1)
    {
        float sum = 0f;
        int count = 0;

        void AddIfValid(int x, int y)
        {
            if (x >= 0 && x < _size && y >= 0 && y < _size)
            {
                sum += _heightmap[y, x];
                count++;
            }
        }

        AddIfValid(x0, y0);
        AddIfValid(x0, y1);
        AddIfValid(x1, y0);
        AddIfValid(x1, y1);

        return sum / count;
    }

    private void SetEdge(int x, int y, int half, float randRange)
    {
        float sum = 0f;
        int count = 0;

        if (x - half >= 0) { sum += _heightmap[y, x - half]; count++; }
        if (x + half < _size) { sum += _heightmap[y, x + half]; count++; }
        if (y - half >= 0) { sum += _heightmap[y - half, x]; count++; }
        if (y + half < _size) { sum += _heightmap[y + half, x]; count++; }

        _heightmap[y, x] = Mathf.Clamp01(sum / count + RandomNoise(randRange));
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

