using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DS_Terrain : MonoBehaviour
{
    public enum AlgorithmType { Recursive, Iterative }
    public enum TerrainType { Normal, Volcanic }

    [Header("Diamond-Square Settings")]
    public AlgorithmType algorithm = AlgorithmType.Recursive;
    public int size = 7;
    public float xScale = 1f;
    public float yScale = 1f;
    public float heightScale = 20f;
    public float roughness = 0.7f;

    private MeshFilter meshFilter;
    float[,] heightMap;
    float[,] newHeightMap;

    public TerrainConfig normalConfig = new TerrainConfig(
    20f, 0.7f, 0.35f, 0.5f, 0.25f, 0.35f, 0.5f
);

    public TerrainConfig volcanicConfig = new TerrainConfig(
        40f, 0.7f, 0.15f, 0.25f, 0.15f, 0.25f, 0.35f
    );

    public TerrainType terrainType = TerrainType.Normal;

    public Material[] normalMaterials;
    public float[] normalThresholds;

    public Material[] volcanicMaterials;
    public float[] volcanicThresholds;

    void Awake()
    {
        size = (int)Mathf.Pow(2, size) + 1;
        meshFilter = GetComponent<MeshFilter>();

        TerrainConfig config = terrainType == TerrainType.Normal ? normalConfig : volcanicConfig;
        heightScale = config.heightScale;
        roughness = config.roughness;


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
        TerrainConfig config = terrainType == TerrainType.Normal ? normalConfig : volcanicConfig;
        CA_TerrainUpgrade caGenerator = new CA_TerrainUpgrade(config);
        heightMap = caGenerator.ApplyCA(heightMap, 20);
        newHeightMap = heightMap;
        Mesh mesh = BuildMesh(newHeightMap);
        meshFilter.mesh = mesh;
    }

    Mesh BuildMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Vector3[] vertices = new Vector3[width * height];
        Vector2[] uv = new Vector2[vertices.Length];

        List<int>[] submeshTriangles;
        Material[] materials;
        float[] thresholds;

        if (terrainType == TerrainType.Normal)
        {
            materials = normalMaterials;
            thresholds = normalThresholds;
        }
        else
        {
            materials = volcanicMaterials;
            thresholds = volcanicThresholds;
        }

        submeshTriangles = new List<int>[materials.Length];
        for (int i = 0; i < submeshTriangles.Length; i++)
            submeshTriangles[i] = new List<int>();

        int vertIndex = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = heightMap[z, x] * heightScale;
                vertices[vertIndex] = new Vector3(x * xScale, y, z * yScale);
                uv[vertIndex] = new Vector2((float)x / (width - 1), (float)z / (height - 1));
                vertIndex++;
            }
        }

        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int topLeft = z * width + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + width;
                int bottomRight = bottomLeft + 1;

                float avgH1 = (heightMap[z, x] + heightMap[z + 1, x] + heightMap[z, x + 1]) / 3f;
                int matIndex1 = GetMaterialIndex(avgH1, thresholds);
                submeshTriangles[matIndex1].Add(topLeft);
                submeshTriangles[matIndex1].Add(bottomLeft);
                submeshTriangles[matIndex1].Add(topRight);

                float avgH2 = (heightMap[z, x + 1] + heightMap[z + 1, x] + heightMap[z + 1, x + 1]) / 3f;
                int matIndex2 = GetMaterialIndex(avgH2, thresholds);
                submeshTriangles[matIndex2].Add(topRight);
                submeshTriangles[matIndex2].Add(bottomLeft);
                submeshTriangles[matIndex2].Add(bottomRight);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.subMeshCount = materials.Length;
        for (int i = 0; i < materials.Length; i++)
            mesh.SetTriangles(submeshTriangles[i].ToArray(), i);

        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        meshFilter.GetComponent<MeshRenderer>().materials = materials;

        return mesh;
    }

    int GetMaterialIndex(float h, float[] thresholds)
    {
        for (int i = 0; i < thresholds.Length; i++)
            if (h < thresholds[i]) return i;
        return thresholds.Length - 1;
    }

    /*Color GetColorByHeight(float h)
    {
        if (terrainType == TerrainType.Normal)
        {
            if (h < 0.25f) return Color.blue;
            else if (h < 0.35f) return Color.yellow;
            else if (h < 0.5f) return Color.green;
            else if (h < 0.7f) return Color.grey;
            else return Color.white;
        }
        else
        {
            if (h < 0.12f) return new Color(0.15f, 0.07f, 0.03f);
            else if (h < 0.18f) return new Color(0.3f, 0.15f, 0.05f);
            else if (h < 0.25f) return new Color(0.5f, 0.2f, 0.1f);
            else if (h < 0.35f) return new Color(0.7f, 0.3f, 0.1f);
            else if (h < 0.55f) return Color.grey;
            else if (h < 0.75f) return new Color(0.9f, 0.3f, 0.1f);
            else return Color.black;
        }
    }*/

    public float[,] returnHeighMap()
    {
        return newHeightMap;
    }
}
