using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapAutoLabeler : EditorWindow
{
    string inputFolder = "Assets/Maps/Manual";
    string outputFolder = "Assets/Maps/Labeled";

    [MenuItem("Tools/Map Auto Labeler")]
    public static void ShowWindow()
    {
        GetWindow<MapAutoLabeler>("Map Auto Labeler");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto-Etiquetador de Mapas", EditorStyles.boldLabel);

        inputFolder = EditorGUILayout.TextField("Carpeta de entrada:", inputFolder);
        outputFolder = EditorGUILayout.TextField("Carpeta de salida:", outputFolder);

        if (GUILayout.Button("Etiquetar Mapas"))
        {
            LabelMaps();
        }
    }

    void LabelMaps()
    {
        if (!Directory.Exists(inputFolder))
        {
            Debug.LogError("Carpeta de entrada no existe: " + inputFolder);
            return;
        }

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string[] files = Directory.GetFiles(inputFolder, "*.txt");

        int processed = 0;

        foreach (string file in files)
        {
            string[] lines = File.ReadAllLines(file);
            List<int> numbers = new List<int>();

            foreach (string line in lines)
            {
                var parts = line.Split(' ', '\t');
                foreach (var p in parts)
                {
                    if (int.TryParse(p, out int v))
                        numbers.Add(v);
                }
            }

            int pasto = numbers.Count(n => n == 0);
            int agua = numbers.Count(n => n == 3);
            int casa = numbers.Count(n => n == 1);
            int persona = numbers.Count(n => n == 2);

            int naturales = pasto + agua;
            int poblados = casa + persona;

            int label;

            if (naturales > poblados)
                label = 0;
            else if (poblados > naturales)
                label = 1;
            else
                label = 2;

            string fileName = Path.GetFileNameWithoutExtension(file) + "_labeled.txt";
            string outPath = Path.Combine(outputFolder, fileName);

            List<string> newFile = new List<string>();
            newFile.Add("label: " + label);
            newFile.AddRange(lines);

            File.WriteAllLines(outPath, newFile);

            processed++;
        }

        Debug.Log($"Etiquetado completado. Total mapas procesados: {processed}");
    }
}
