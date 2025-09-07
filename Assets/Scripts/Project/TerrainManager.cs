using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public DS_Terrain terrain;
    public TreeSpawner treeSpawner;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            terrain.RegenerateTerrain(DS_Terrain.TerrainType.Normal);
            treeSpawner.RegenerateTrees();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            terrain.RegenerateTerrain(DS_Terrain.TerrainType.Volcanic);
            treeSpawner.RegenerateTrees();
        }
    }
}