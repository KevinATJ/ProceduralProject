using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualMapLoader : MonoBehaviour
{
    [Header("Ruta relativa dentro de Assets (sin / al inicio)")]
    public string mapsFolderPath = "Maps/Manual";

    [Header("Mostrar mensajes en consola")]
    public bool debugLogs = true;

    public List<int[,]> LoadAllManualMaps()
    {
        List<int[,]> result = new List<int[,]>();

        string fullPath = Path.Combine(Application.dataPath, mapsFolderPath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($" La carpeta no existe: {fullPath}");
            return result;
        }

        string[] files = Directory.GetFiles(fullPath, "*.txt");

        if (files.Length == 0)
        {
            Debug.LogError($" No se encontraron archivos .txt en {fullPath}");
            return result;
        }

        foreach (string file in files)
        {
            try
            {
                int[,] mapMatrix = LoadSingleMap(file);
                result.Add(mapMatrix);

                if (debugLogs)
                    Debug.Log($" Mapa cargado correctamente: {Path.GetFileName(file)}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($" Error cargando el mapa {file}: {ex.Message}");
            }
        }

        Debug.Log($" TOTAL MAPAS CARGADOS: {result.Count}");
        return result;
    }
    private int[,] LoadSingleMap(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length == 0)
            throw new System.Exception("El archivo está vacío");

        List<int[]> rows = new List<int[]>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] tokens = line.Split(' ', '\t');

            List<int> parsedRow = new List<int>();

            foreach (string token in tokens)
            {
                if (token.Trim() == "")
                    continue;

                if (!int.TryParse(token, out int value))
                    throw new System.Exception($"No se pudo convertir \"{token}\" a número");

                parsedRow.Add(value);
            }

            rows.Add(parsedRow.ToArray());
        }

        int width = rows[0].Length;

        foreach (var row in rows)
        {
            if (row.Length != width)
                throw new System.Exception("Las filas del mapa no tienen el mismo ancho.");
        }

        int height = rows.Count;
        int[,] matrix = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x] = rows[y][x];
            }
        }

        return matrix;
    }
}
