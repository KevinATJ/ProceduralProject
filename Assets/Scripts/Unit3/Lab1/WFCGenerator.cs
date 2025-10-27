using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WFCGenerator : MonoBehaviour
{
    [Header("Configuración")]
    public Tile[] AllTiles;
    public int GridSize = 10;
    public float StepDelay = 0.1f;
    public float TileSize = 1f;

    private Cell[,] Grid;
    private List<int> AllTileIDs;
    private Stack<Cell[,]> HistoryStack = new Stack<Cell[,]>();
    private GameObject MapContainer;

    void Start()
    {
        CleanupPreviousMap();
        StartCoroutine(WaveFunctionCollapseCoroutine());
    }

    public IEnumerator WaveFunctionCollapseCoroutine()
    {
        InitializeGrid();

        while (!CheckIfDone())
        {
            Cell cellToCollapse = FindCellWithMinEntropy();

            if (cellToCollapse == null)
            {
                Debug.Log("Todas las celdas colapsadas");
                break;
            }

            if (cellToCollapse.IsContradiction)
            {
                Debug.LogError($"Contradicción detectada en ({cellToCollapse.Row}, {cellToCollapse.Col}). Intentando Backtracking...");

                if (HistoryStack.Count > 0)
                {
                    Grid = HistoryStack.Pop();
                    yield return null;
                    continue;
                }
                else
                {
                    Debug.LogError("No hay historial de backtracking, reiniciando completamente");
                    InitializeGrid();
                    yield return null;
                    continue;
                }
            }

            SaveState();
            CollapseCell(cellToCollapse);
            Propagate(cellToCollapse);

            yield return new WaitForSeconds(StepDelay);
        }

        Debug.Log("Mapa creado");
    }

    private void CleanupPreviousMap()
    {
        GameObject oldMap = GameObject.Find("GeneratedMapContainer");
        if (oldMap != null) Destroy(oldMap);
    }

    private void InitializeGrid()
    {
        CleanupPreviousMap();

        AllTileIDs = AllTiles.Select(t => t.ID).ToList();
        Grid = new Cell[GridSize, GridSize];
        HistoryStack.Clear();

        MapContainer = new GameObject("GeneratedMapContainer");

        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                Grid[r, c] = new Cell(r, c, AllTileIDs);
            }
        }
    }

    private void SaveState()
    {
        Cell[,] newState = new Cell[GridSize, GridSize];
        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                newState[r, c] = Grid[r, c].DeepCopy();
            }
        }
        HistoryStack.Push(newState);
    }

    private Cell FindCellWithMinEntropy()
    {
        Cell minEntropyCell = null;
        int minEntropy = int.MaxValue;
        List<Cell> minimumEntropyCells = new List<Cell>();

        foreach (Cell cell in Grid)
        {
            if (!cell.Collapsed)
            {
                if (cell.Entropy < minEntropy)
                {
                    minEntropy = cell.Entropy;
                    minimumEntropyCells.Clear();
                    minimumEntropyCells.Add(cell);
                }
                else if (cell.Entropy == minEntropy)
                {
                    minimumEntropyCells.Add(cell);
                }
            }
        }

        if (minimumEntropyCells.Count > 0)
            return minimumEntropyCells[Random.Range(0, minimumEntropyCells.Count)];

        return null;
    }

    private void CollapseCell(Cell cell)
    {
        int chosenID = cell.PossibleTileIDs[Random.Range(0, cell.PossibleTileIDs.Count)];

        cell.ChosenTileID = chosenID;
        cell.PossibleTileIDs.Clear();
        cell.PossibleTileIDs.Add(chosenID);
        cell.Collapsed = true;

        Tile chosenTile = AllTiles.First(t => t.ID == cell.ChosenTileID);

        if (chosenTile.Prefab != null)
        {
            float offset = (GridSize - 1) / 2f;
            float size = TileSize;

            if (Mathf.Approximately(TileSize, 0f))
            {
                Renderer rend = chosenTile.Prefab.GetComponentInChildren<Renderer>();
                if (rend != null) size = rend.bounds.size.x;
                else size = 1f;
            }

            Vector3 position = new Vector3(
                cell.Col * size - offset * size,
                cell.Row * size - offset * size,
                0
            );

            Quaternion prefabRotation = chosenTile.Prefab.transform.rotation;

            Instantiate(chosenTile.Prefab, position, prefabRotation, MapContainer.transform);
        }

        Debug.Log($"Celda colapsada: ({cell.Row}, {cell.Col}) | ID Elegido: {chosenID}");
    }

    private void Propagate(Cell startCell)
    {
        var stack = new Stack<Cell>();
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            Cell currentCell = stack.Pop();
            int r = currentCell.Row;
            int c = currentCell.Col;

            (int dr, int dc, string direction)[] neighbors =
            {
                (1, 0, "UP"), (-1, 0, "DOWN"), (0, -1, "LEFT"), (0, 1, "RIGHT")
            };

            foreach (var (dr, dc, direction) in neighbors)
            {
                int nr = r + dr;
                int nc = c + dc;

                if (nr >= 0 && nr < GridSize && nc >= 0 && nc < GridSize)
                {
                    Cell neighborCell = Grid[nr, nc];

                    if (!neighborCell.Collapsed)
                    {
                        List<int> removedIDs = EnforceConstraints(currentCell, neighborCell, direction);

                        if (removedIDs.Count > 0)
                        {
                            Debug.Log($"   Propagación ({direction}): ({neighborCell.Row}, {neighborCell.Col}) | Nueva Entropía: {neighborCell.Entropy}");
                            stack.Push(neighborCell);
                        }
                    }
                }
            }
        }
    }

    private List<int> EnforceConstraints(Cell sourceCell, Cell targetCell, string relation)
    {
        List<int> removableIDs = new List<int>();

        foreach (int targetID in targetCell.PossibleTileIDs)
        {
            bool isCompatible = false;
            Tile targetTile = AllTiles.First(t => t.ID == targetID);

            foreach (int sourceID in sourceCell.PossibleTileIDs)
            {
                Tile sourceTile = AllTiles.First(t => t.ID == sourceID);
                List<int> requiredCompatibility = new List<int>();

                switch (relation)
                {
                    case "UP": requiredCompatibility = sourceTile.UpCompatibility; break;
                    case "DOWN": requiredCompatibility = sourceTile.DownCompatibility; break;
                    case "LEFT": requiredCompatibility = sourceTile.LeftCompatibility; break;
                    case "RIGHT": requiredCompatibility = sourceTile.RightCompatibility; break;
                }

                if (requiredCompatibility.Contains(targetID))
                {
                    isCompatible = true;
                    break;
                }
            }

            if (!isCompatible)
            {
                removableIDs.Add(targetID);
            }
        }

        foreach (int id in removableIDs)
        {
            targetCell.RemoveOption(id);
        }

        return removableIDs;
    }

    private bool CheckIfDone()
    {
        foreach (Cell cell in Grid)
        {
            if (!cell.Collapsed)
                return false;
        }
        return true;
    }

}