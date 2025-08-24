using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DS_PCG_Script
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

        int tileSize = _size - 1;
        float scale = 1f;

        while (tileSize > 1)
        {
            int half = tileSize / 2;

            // Diamond Step: puntos medios de los cuadrados
            for (int x = half; x < _size; x += tileSize)
            {
                for (int y = half; y < _size; y += tileSize)
                {
                    DiamondStep(x, y, tileSize, scale);
                }
            }

            // Square Step: puntos medios de los bordes
            for (int y = 0; y < _size; y += tileSize)
            {
                for (int x = half; x < _size; x += tileSize)
                {
                    SquareStep(x, y, tileSize, scale);
                }
            }
            for (int y = half; y < _size; y += tileSize)
            {
                for (int x = 0; x < _size; x += tileSize)
                {
                    SquareStep(x, y, tileSize, scale);
                }
            }

            // Reducir ruido
            scale /= Mathf.Pow(2.0f, _roughness);
            tileSize /= 2;
        }
    }

    private void InitialiseCorners()
    {
        float mid = 0.5f;
        _heightmap[0, 0] = Random.Range(mid - 0.1f, mid + 0.1f);
        _heightmap[0, _size - 1] = Random.Range(mid - 0.1f, mid + 0.1f);
        _heightmap[_size - 1, 0] = Random.Range(mid - 0.1f, mid + 0.1f);
        _heightmap[_size - 1, _size - 1] = Random.Range(mid - 0.1f, mid + 0.1f);
    }

    private void DiamondStep(int x, int y, int tileSize, float scale)
    {
        int half = tileSize / 2;
        float sum = 0f;
        int count = 0;

        // promediar esquinas válidas
        if (x - half >= 0 && y - half >= 0) { sum += _heightmap[y - half, x - half]; count++; }
        if (x + half < _size && y - half >= 0) { sum += _heightmap[y - half, x + half]; count++; }
        if (x - half >= 0 && y + half < _size) { sum += _heightmap[y + half, x - half]; count++; }
        if (x + half < _size && y + half < _size) { sum += _heightmap[y + half, x + half]; count++; }

        _heightmap[y, x] = Mathf.Clamp01(sum / count + (Random.value * 2 - 1) * scale);
    }

    private void SquareStep(int x, int y, int tileSize, float scale)
    {
        int half = tileSize / 2;
        float sum = 0f;
        int count = 0;

        if (x - half >= 0) { sum += _heightmap[y, x - half]; count++; }
        if (x + half < _size) { sum += _heightmap[y, x + half]; count++; }
        if (y - half >= 0) { sum += _heightmap[y - half, x]; count++; }
        if (y + half < _size) { sum += _heightmap[y + half, x]; count++; }

        _heightmap[y, x] = Mathf.Clamp01(sum / count + (Random.value * 2 - 1) * scale);
    }

    public float[,] GetHeightMap()
    {
        return _heightmap;
    }
}
