using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_TerrainUpgrade
{
    private float waterLevel;
    private float sandLevel;
    private float grassLevel;

    public CA_TerrainUpgrade(TerrainConfig config)
    {
        waterLevel = config.worldWaterLevel;
        sandLevel = config.worldSandLevel;
        grassLevel = config.worldGrassLevel;
    }

    public float[,] ApplyCA(float[,] heightMap, int iterations = 1, int radius = 1)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        for (int iter = 0; iter < iterations; iter++)
        {
            float[,] newMap = (float[,])heightMap.Clone();

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = heightMap[z, x];

                    if (value < waterLevel)
                    {
                        newMap[z, x] = AverageWater(heightMap, x, z, radius);
                    }
                    else if ((value >= waterLevel && value < grassLevel) && TouchesWater(heightMap, x, z, radius))
                    {
                        newMap[z, x] = AverageNeighbors(heightMap, x, z, radius);
                    }
                }
            }

            heightMap = newMap;
        }
        return heightMap;
    }
    private float AverageNeighbors(float[,] map, int x, int z, int radius)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        float sum = 0f;
        int count = 0;

        for (int dz = -radius; dz <= radius; dz++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx < 0 || nz < 0 || nx >= width || nz >= height)
                    continue;

                sum += map[nz, nx];
                count++;
            }
        }

        if (count == 0) return map[z, x];
        return sum / count;
    }
    private float AverageWater(float[,] map, int x, int z, int radius)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        float sum = 0f;
        int count = 0;

        for (int dz = -radius; dz <= radius; dz++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx < 0 || nz < 0 || nx >= width || nz >= height)
                    continue;

                if (map[nz, nx] < waterLevel)
                {
                    sum += map[nz, nx];
                    count++;
                }
            }
        }

        if (count == 0) return map[z, x];
        return sum / count;
    }

    private bool TouchesWater(float[,] map, int x, int z, int radius)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int dz = -radius; dz <= radius; dz++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx < 0 || nz < 0 || nx >= width || nz >= height)
                    continue;

                if (map[nz, nx] < waterLevel)
                    return true;
            }
        }
        return false;
    }
}
