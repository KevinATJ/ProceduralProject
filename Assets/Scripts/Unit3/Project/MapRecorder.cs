using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapRecorder : MonoBehaviour
{
    public string saveFolder = "Assets/GeneratedMaps/";

    public void SaveMap(int[,] map, string fileName)
    {
        Directory.CreateDirectory(saveFolder);

        int height = map.GetLength(0);
        int width = map.GetLength(1);

        string path = Path.Combine(saveFolder, fileName);
        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int y = 0; y < height; y++)
            {
                string line = "";
                for (int x = 0; x < width; x++)
                {
                    line += map[y, x];
                    if (x < width - 1) line += " ";
                }
                writer.WriteLine(line);
            }
        }

        Debug.Log("Mapa guardado en: " + path);
    }
}
