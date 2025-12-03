using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedMapsFolderCreator : MonoBehaviour
{
    public string folderPath = "Assets/GeneratedMaps/";

    void Awake()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Carpeta creada: " + folderPath);
        }
    }
}
