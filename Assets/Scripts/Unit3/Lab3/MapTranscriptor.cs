using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapTranscriptor : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public string mapsFolder = "Assets/GeneratedMaps/";

    public void LoadMapFromTxt(string fileName)
    {
        string path = Path.Combine(mapsFolder, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("El archivo no existe: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);
        int height = lines.Length;
        int width = lines[0].Split(' ').Length;

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int y = 0; y < height; y++)
        {
            string[] values = lines[y].Split(' ');

            for (int x = 0; x < width; x++)
            {
                int id = int.Parse(values[x]);

                if (id < 0 || id >= tilePrefabs.Length)
                {
                    Debug.LogError($"ID inválido en el mapa: {id}");
                    continue;
                }

                Instantiate(tilePrefabs[id], new Vector3(x, -y, 0), Quaternion.identity, this.transform);
            }
        }

        Debug.Log("Mapa cargado correctamente desde " + fileName);
    }
}
