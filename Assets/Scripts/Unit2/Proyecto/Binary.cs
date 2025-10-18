using UnityEngine;
using System.IO;

public class BinaryWithSymmetry : MonoBehaviour
{
    public enum Symmetry { None, Horizontal, Vertical, Both, Rotational }

    [Header("Configuración General")]
    public int imageSize = 256;
    public int cellSize = 16;
    public Symmetry symmetry = Symmetry.Horizontal;
    public int count = 5;
    public int seed = 12345;
    public string folderName = "GeneratedPatterns";

    [Header("Colores")]
    public Color colorA = Color.black;
    public Color colorB = Color.white;

    [ContextMenu("Generar Imágenes")]
    public void GenerateImages()
    {
        string path = Path.Combine(Application.persistentDataPath, folderName);
        Directory.CreateDirectory(path);
        Random.InitState(seed);

        for (int i = 0; i < count; i++)
        {
            Texture2D tex = GenerateBinarySymmetryTexture(imageSize, imageSize, cellSize, symmetry);
            byte[] bytes = tex.EncodeToPNG();
            string filePath = Path.Combine(path, $"pattern_{i:D2}.png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"Guardado: {filePath}");

            SaveTiles(tex, path, $"pattern_{i:D2}_tiles", 4);
        }

        Debug.Log($"Listo. Carpeta: {path}");
    }

    Texture2D GenerateBinarySymmetryTexture(int width, int height, int cellSize, Symmetry sym)
    {
        int cols = Mathf.CeilToInt((float)width / cellSize);
        int rows = Mathf.CeilToInt((float)height / cellSize);
        bool[,] map = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                bool value = false;

                switch (sym)
                {
                    case Symmetry.None:
                        value = Random.value > 0.5f;
                        break;
                    case Symmetry.Horizontal:
                        int midH = (cols + 1) / 2;
                        if (c < midH) value = Random.value > 0.5f;
                        else value = map[r, cols - 1 - c];
                        break;
                    case Symmetry.Vertical:
                        int midV = (rows + 1) / 2;
                        if (r < midV) value = Random.value > 0.5f;
                        else value = map[rows - 1 - r, c];
                        break;
                    case Symmetry.Both:
                        int midCols = (cols + 1) / 2;
                        int midRows = (rows + 1) / 2;
                        if (r < midRows && c < midCols)
                            value = Random.value > 0.5f;
                        else if (r < midRows && c >= midCols)
                            value = map[r, cols - 1 - c];
                        else if (r >= midRows && c < midCols)
                            value = map[rows - 1 - r, c];
                        else
                            value = map[rows - 1 - r, cols - 1 - c];
                        break;
                    case Symmetry.Rotational:
                        int r2 = rows - 1 - r;
                        int c2 = cols - 1 - c;
                        if (r < rows / 2 || (r == rows / 2 && c <= cols / 2))
                            value = Random.value > 0.5f;
                        else
                            value = map[r2, c2];
                        break;
                }

                map[r, c] = value;
            }
        }

        // Crear textura
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.filterMode = FilterMode.Point;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Color col = map[r, c] ? colorA : colorB;
                for (int y = 0; y < cellSize; y++)
                {
                    for (int x = 0; x < cellSize; x++)
                    {
                        int px = c * cellSize + x;
                        int py = r * cellSize + y;
                        if (px < width && py < height)
                            tex.SetPixel(px, py, col);
                    }
                }
            }
        }

        tex.Apply();
        return tex;
    }

    void SaveTiles(Texture2D tex, string baseFolder, string baseName, int n)
    {
        int tileW = tex.width / n;
        int tileH = tex.height / n;
        string tilePath = Path.Combine(baseFolder, baseName);
        Directory.CreateDirectory(tilePath);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == n - 1 && j == n - 1)
                    continue; // última casilla vacía

                Texture2D tile = new Texture2D(tileW, tileH, TextureFormat.RGB24, false);
                tile.filterMode = FilterMode.Point;
                tile.SetPixels(tex.GetPixels(j * tileW, i * tileH, tileW, tileH));
                tile.Apply();

                string tileFile = Path.Combine(tilePath, $"tile_{i}_{j}.png");
                File.WriteAllBytes(tileFile, tile.EncodeToPNG());
            }
        }

        Debug.Log($"Tiles guardados en: {tilePath}");
    }
}
