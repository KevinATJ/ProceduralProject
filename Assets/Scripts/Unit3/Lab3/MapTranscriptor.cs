using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MapTranscriptor : MonoBehaviour
{
    [Header("Configuración del Archivo")]
    [Tooltip("Arrastra aquí tu archivo de texto desde la carpeta Resources")]
    public TextAsset archivoMapa;

    [Header("Configuración del Mapa")]
    public bool usarEspaciosComoSeparador = true;
    public char separador = ',';

    [Header("Información del Mapa (Solo lectura)")]
    [SerializeField] private int filas;
    [SerializeField] private int columnas;

    // Matriz pública que almacena los datos del mapa
    public int[,] matrizMapa;

    void Start()
    {
        if (archivoMapa != null)
        {
            CargarMapa();
            MostrarMatriz();
        }
        else
        {
            Debug.LogError("No se ha asignado ningún archivo de mapa!");
        }
    }

    /// <summary>
    /// Carga el archivo TXT y lo convierte en una matriz
    /// </summary>
    public void CargarMapa()
    {
        if (archivoMapa == null)
        {
            Debug.LogError("Archivo de mapa no asignado");
            return;
        }

        // Leer todas las líneas del archivo
        string[] lineas = archivoMapa.text.Split('\n');

        // Filtrar líneas vacías
        List<string> lineasValidas = new List<string>();
        foreach (string linea in lineas)
        {
            string lineaLimpia = linea.Trim();
            if (!string.IsNullOrEmpty(lineaLimpia))
            {
                lineasValidas.Add(lineaLimpia);
            }
        }

        if (lineasValidas.Count == 0)
        {
            Debug.LogError("El archivo está vacío o no tiene datos válidos");
            return;
        }

        // Determinar dimensiones de la matriz
        filas = lineasValidas.Count;

        // Separar la primera fila para determinar columnas
        string[] primeraFila;
        if (usarEspaciosComoSeparador)
        {
            primeraFila = lineasValidas[0].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            primeraFila = lineasValidas[0].Split(separador);
        }
        columnas = primeraFila.Length;

        // Inicializar la matriz
        matrizMapa = new int[filas, columnas];

        // Llenar la matriz con los datos del archivo
        for (int i = 0; i < filas; i++)
        {
            string[] valores;

            // Separar por espacios o por el separador definido
            if (usarEspaciosComoSeparador)
            {
                valores = lineasValidas[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                valores = lineasValidas[i].Split(separador);
            }

            for (int j = 0; j < columnas && j < valores.Length; j++)
            {
                string valor = valores[j].Trim();

                // Convertir a entero
                if (int.TryParse(valor, out int numero))
                {
                    matrizMapa[i, j] = numero;
                }
                else
                {
                    Debug.LogWarning($"No se pudo convertir '{valor}' en la posición [{i},{j}]. Se usará 0.");
                    matrizMapa[i, j] = 0;
                }
            }
        }

        Debug.Log($"Mapa cargado exitosamente: {filas} filas x {columnas} columnas");
    }

    /// <summary>
    /// Obtiene el valor en una posición específica de la matriz
    /// </summary>
    public int ObtenerValor(int fila, int columna)
    {
        if (matrizMapa == null)
        {
            Debug.LogError("La matriz no ha sido inicializada. Llama primero a CargarMapa()");
            return -1;
        }

        if (fila < 0 || fila >= filas || columna < 0 || columna >= columnas)
        {
            Debug.LogError($"Índice fuera de rango: [{fila},{columna}]");
            return -1;
        }

        return matrizMapa[fila, columna];
    }

    /// <summary>
    /// Devuelve la matriz completa
    /// </summary>
    public int[,] ObtenerMatriz()
    {
        return matrizMapa;
    }

    /// <summary>
    /// Obtiene las dimensiones del mapa
    /// </summary>
    public Vector2Int ObtenerDimensiones()
    {
        return new Vector2Int(columnas, filas);
    }

    /// <summary>
    /// Muestra la matriz en la consola para debug
    /// </summary>
    public void MostrarMatriz()
    {
        if (matrizMapa == null)
        {
            Debug.LogError("La matriz no ha sido inicializada");
            return;
        }

        string resultado = "Matriz del Mapa:\n";
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                resultado += matrizMapa[i, j].ToString().PadLeft(3) + " ";
            }
            resultado += "\n";
        }

        Debug.Log(resultado);
    }

    /// <summary>
    /// Carga un mapa desde una ruta específica (alternativa al TextAsset)
    /// </summary>
    public void CargarDesdeRuta(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            Debug.LogError($"El archivo no existe: {rutaArchivo}");
            return;
        }

        string[] lineas = File.ReadAllLines(rutaArchivo);

        // Filtrar líneas vacías
        List<string> lineasValidas = new List<string>();
        foreach (string linea in lineas)
        {
            string lineaLimpia = linea.Trim();
            if (!string.IsNullOrEmpty(lineaLimpia))
            {
                lineasValidas.Add(lineaLimpia);
            }
        }

        if (lineasValidas.Count == 0)
        {
            Debug.LogError("El archivo está vacío");
            return;
        }

        filas = lineasValidas.Count;

        // Separar la primera fila para determinar columnas
        string[] primeraFila;
        if (usarEspaciosComoSeparador)
        {
            primeraFila = lineasValidas[0].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            primeraFila = lineasValidas[0].Split(separador);
        }
        columnas = primeraFila.Length;

        matrizMapa = new int[filas, columnas];

        for (int i = 0; i < filas; i++)
        {
            string[] valores;

            if (usarEspaciosComoSeparador)
            {
                valores = lineasValidas[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                valores = lineasValidas[i].Split(separador);
            }

            for (int j = 0; j < columnas && j < valores.Length; j++)
            {
                string valor = valores[j].Trim();

                if (int.TryParse(valor, out int numero))
                {
                    matrizMapa[i, j] = numero;
                }
                else
                {
                    matrizMapa[i, j] = 0;
                }
            }
        }

        Debug.Log($"Mapa cargado desde ruta: {filas} filas x {columnas} columnas");
    }
}
