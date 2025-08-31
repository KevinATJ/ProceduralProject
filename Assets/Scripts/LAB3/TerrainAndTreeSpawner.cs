using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainAndTreeSpawner : MonoBehaviour
{
    public DS_Terrain terrainScript;
    public LSystemGenerator lSystemGenerator;
    public GameObject treeBuilderPrefab;

    [Header("Tree Placement Settings")]
    public int treeCount = 20;
    public float minHeight = 0.3f;
    public float maxHeight = 0.6f;

    void Start()
    {

        if (terrainScript == null || lSystemGenerator == null || treeBuilderPrefab == null)
        {
            Debug.LogError("Referencias de scripts o prefabs no asignadas.");
            return;
        }

        SpawnTrees();
    }

    void SpawnTrees()
    {
        float[,] heightMap = terrainScript.returnHeighMap();
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        for (int i = 0; i < treeCount; i++)
        {
            int x = Random.Range(0, width);
            int z = Random.Range(0, height);
            float hNormalized = heightMap[x, z];

            if (hNormalized < minHeight || hNormalized > maxHeight)
            {
                i--;
                continue;
            }

            Vector3 pos = new Vector3(x * terrainScript.xScale, hNormalized * terrainScript.heightScale, z * terrainScript.yScale);

            GameObject treeParent = Instantiate(treeBuilderPrefab, pos, Quaternion.identity);

            string treeSentence = lSystemGenerator.GenerateSentence();

            TreeBuilder builder = treeParent.GetComponent<TreeBuilder>();
            if (builder != null)
            {
                builder.DrawTree(treeSentence);
            }
        }
    }
}
