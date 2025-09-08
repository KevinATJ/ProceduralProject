using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SquareType
{
    EMPTY,
    FILL,
    BORDER
}

public class CA_PCG_Script : MonoBehaviour
{
    public Camera mainCamera;

    SquareType[,] grid;

    [SerializeField] int roomWidth = 50;
    [SerializeField] int roomHeight = 50;

    [SerializeField] public int iterations = 4;

    [SerializeField] double noiseDensity = 0.45;

    [SerializeField] int range = 1;

    [SerializeField] int fillingSpace = 4;

    public GameObject fillPrefab;
    public GameObject emptyPrefab;
    public GameObject borderPrefab;

    void Start()
    {
        GenerateGrid();
        DisplayGrid();
        AdjustCameraPosition();
    }

    private void Setup()
    {
        grid = new SquareType[roomWidth, roomHeight];
    }


    void Update()
    {

    }

    public SquareType[,] GenerateGrid()
    {
        Setup();
        GenerateNoise();
        CellularAutomata();

        return grid;
    }

    private void GenerateNoise()
    {
        for (var i = 0; i < roomWidth; i++)
        {
            for (var j = 0; j < roomHeight; j++)
            {
                if (Random.value > noiseDensity)
                {
                    grid[i, j] = SquareType.FILL;
                    continue;
                }

                grid[i, j] = SquareType.EMPTY;
            }
        }
    }

    private void CellularAutomata()
    {
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            SquareType[,] tempGrid = (SquareType[,])grid.Clone();

            for (int i = 0; i < roomWidth; i++)
            {
                for (int j = 0; j < roomHeight; j++)
                {
                    var fillNumber = 0;
                    var border = false;

                    for (int k = i - range; k <= i + range; k++)
                    {
                        for (int l = j - range; l <= j + range; l++)
                        {
                            if (k >= 0 && l >= 0 && k < roomWidth && l < roomHeight)
                            {
                                if (k != i || l != j)
                                {
                                    if (tempGrid[k, l] == SquareType.FILL || tempGrid[k, l] == SquareType.BORDER)
                                    {
                                        fillNumber++;
                                    }
                                }
                            }
                            else
                            {
                                border = true;
                            }
                        }
                    }

                    if (fillNumber > fillingSpace)
                    {
                        grid[i, j] = SquareType.FILL;
                    }
                    else
                    {
                        grid[i, j] = SquareType.EMPTY;
                    }
                    if (border)
                    {
                        grid[i, j] = SquareType.BORDER;
                    }

                }
            }
        }
    }
    private void DisplayGrid()
    {
        for (int i = 0; i < roomWidth; i++)
        {
            for (int j = 0; j < roomHeight; j++)
            {
                Vector2 position = new Vector2(i, j);

                if (grid[i, j] == SquareType.FILL)
                {
                    Instantiate(fillPrefab, position, Quaternion.identity);
                }
                if (grid[i, j] == SquareType.EMPTY)
                {
                    Instantiate(emptyPrefab, position, Quaternion.identity);
                }
                if (grid[i, j] == SquareType.BORDER)
                {
                    Instantiate(borderPrefab, position, Quaternion.identity);
                }
            }
        }
    }

    void AdjustCameraPosition()
    {

        mainCamera.transform.position = new Vector3(roomWidth / 2f, roomHeight / 2f, -10f);

        AdjustCameraOrthographicSize();
    }

    void AdjustCameraOrthographicSize()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float desiredSize = Mathf.Max(roomHeight, roomWidth / aspectRatio) / 2f;

        mainCamera.orthographicSize = desiredSize;
    }
}
