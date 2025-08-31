using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DS_Terrain : MonoBehaviour
{
    public enum AlgorithmType { Recursive, Iterative }

    [Header("Diamond-Square Settings")]
    public AlgorithmType algorithm = AlgorithmType.Recursive;
    public int size = 7;
    public float xScale = 1f;
    public float yScale = 1f;
    public float heightScale = 20f;
    public float roughness = 0.7f;

    private MeshFilter meshFilter;
    float[,] heightMap;

    void Awake()
    {
        size = (int)Mathf.Pow(2, size) + 1;
        meshFilter = GetComponent<MeshFilter>();



        if (algorithm == AlgorithmType.Iterative)
        {
            DS_PCG_Script dsIter = new DS_PCG_Script(size, roughness);
            dsIter.GenerateHeightmap();
            heightMap = dsIter.GetHeightMap();
        }
        else
        {
            DS_PCG_Script_Rec dsRec = new DS_PCG_Script_Rec(size, roughness);
            dsRec.GenerateHeightmap();
            heightMap = dsRec.GetHeightMap();
        }

    }

    private void Start()
    {
        Mesh mesh = BuildMesh(heightMap);
        meshFilter.mesh = mesh;
    }

    Mesh BuildMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Vector3[] vertices = new Vector3[width * height];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        Color[] colors = new Color[vertices.Length];

        int vertIndex = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = heightMap[z, x] * heightScale;
                vertices[vertIndex] = new Vector3(x * xScale, y, z * yScale);
                uv[vertIndex] = new Vector2((float)x / (width - 1), (float)z / (height - 1));
                colors[vertIndex] = GetColorByHeight(y / heightScale);
                vertIndex++;
            }
        }

        int triIndex = 0;
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int topLeft = z * width + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + width;
                int bottomRight = bottomLeft + 1;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomRight;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    Color GetColorByHeight(float h)
    {
        if (h < 0.3f) return Color.blue;
        else if (h < 0.5f) return Color.green;
        else if (h < 0.7f) return Color.grey;
        else return Color.white;
    }

    public float[,] returnHeighMap()
    {
        return heightMap;
    }
}
