using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public DS_Terrain terrainScript;
    public LSystemGenerator lSystemGenerator;
    public GameObject treeBuilderPrefab;

    [Header("Tree Placement Settings")]
    public int treeCount = 20;
    public float treeHeightOffset = -0.5f;

    private float minHeight;
    private float maxHeight;


    private void Awake()
    {
        if (terrainScript == null || lSystemGenerator == null || treeBuilderPrefab == null)
        {
            return;
        }

        TerrainConfig config = terrainScript.terrainType == DS_Terrain.TerrainType.Normal
            ? terrainScript.normalConfig
            : terrainScript.volcanicConfig;

        minHeight = config.treePosMinHeight;
        maxHeight = config.treePosMaxHeight;
    }

    private void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        float[,] heightMap = terrainScript.returnHeighMap();
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Vector3 terrainPosition = terrainScript.transform.position;

        List<Vector2Int> validTreePositions = new List<Vector2Int>();
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float hNormalized = heightMap[z, x];
                if (hNormalized >= minHeight && hNormalized < maxHeight)
                {
                    validTreePositions.Add(new Vector2Int(x, z));
                }
            }
        }

        Shuffle(validTreePositions);
        int treesToSpawn = Mathf.Min(treeCount, validTreePositions.Count);
        for (int i = 0; i < treesToSpawn; i++)
        {
            Vector2Int pos2D = validTreePositions[i];
            int x = pos2D.x;
            int z = pos2D.y;

            float hNormalized = heightMap[z, x];
            float treeY = hNormalized * terrainScript.heightScale + treeHeightOffset;

            Vector3 pos = new Vector3(x * terrainScript.mapX_Scale, treeY, z * terrainScript.mapY_Scale);
            pos += terrainPosition;

            GameObject treeParent = Instantiate(treeBuilderPrefab, pos, Quaternion.identity, transform);

            int treeTypeIndex = Random.Range(0, lSystemGenerator.treeTypes.Count);
            string treeSentence = lSystemGenerator.GenerateSentence(treeTypeIndex);

            TreeBuilder builder = treeParent.GetComponent<TreeBuilder>();
            if (builder != null)
            {
                var ruleSet = lSystemGenerator.treeTypes[treeTypeIndex];
                builder.angle = ruleSet.angle;
                builder.branchScaleY = ruleSet.branchScaleY;
                builder.SetMaterials(terrainScript.terrainType);
                builder.Awake();
                builder.DrawTree(treeSentence);
            }
        }
    }

    private void Shuffle(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector2Int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void RegenerateTrees()
    {
        TerrainConfig config = terrainScript.terrainType == DS_Terrain.TerrainType.Normal
            ? terrainScript.normalConfig
            : terrainScript.volcanicConfig;

        minHeight = config.treePosMinHeight;
        maxHeight = config.treePosMaxHeight;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        SpawnTrees();
    }
}