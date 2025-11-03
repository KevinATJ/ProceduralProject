using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WFCGenerator : MonoBehaviour
{
    public enum GenerationMode { SimpleTiled, ComplexWFC }
    [Header("Configuración del Modo")]
    public GenerationMode Mode = GenerationMode.SimpleTiled;
    public bool UseCustomMatrix = false;
    public EditableMatrix ManualMatrix;

    [Header("Configuración")]
    public Tile[] AllTiles;
    public int SimpleTiledContextGridSize = 10;
    public int FinalOverlappingGridSize = 10;
    public float StepDelay = 0.05f;
    public float TileSize = 1f;

    private Cell[,] Grid;
    private List<int> AllTileIDs;
    private Stack<Cell[,]> HistoryStack = new Stack<Cell[,]>();
    private GameObject MapContainer;
    private int[,] GeneratedContextMatrix;
    private int GridSize;
    private GenerationMode CurrentRunMode;
    private Dictionary<int, Dictionary<string, Dictionary<int, float>>> adjacencyProbs;

    void Start()
    {
        if (AllTiles == null || AllTiles.Length == 0) return;
        CleanupPreviousMap();
        if (Mode == GenerationMode.SimpleTiled)
            StartCoroutine(SimpleTiledWFC(SimpleTiledContextGridSize, true));
        else if (Mode == GenerationMode.ComplexWFC)
            StartCoroutine(ProbabilisticWFC());
    }

    public IEnumerator SimpleTiledWFC(int targetGridSize, bool isFinalRender)
    {
        InitializeGrid(targetGridSize);
        if (AllTileIDs == null || AllTileIDs.Count == 0) yield break;
        int safetyCounter = 0;
        int safetyLimit = targetGridSize * targetGridSize * 50;
        while (!CheckIfDone() && safetyCounter < safetyLimit)
        {
            safetyCounter++;
            Cell cellToCollapse = FindCellWithMinEntropy();
            if (cellToCollapse == null || cellToCollapse.IsContradiction)
            {
                if (HistoryStack.Count > 0)
                {
                    Grid = HistoryStack.Pop();
                    yield return null;
                    continue;
                }
                else
                {
                    if (CheckIfDone()) break;
                    InitializeGrid(targetGridSize);
                    yield return null;
                    continue;
                }
            }
            SaveState();
            CollapseCellSimple(cellToCollapse, isFinalRender);
            Propagate(cellToCollapse);
            if (isFinalRender) yield return new WaitForSeconds(StepDelay);
        }
        GeneratedContextMatrix = new int[targetGridSize, targetGridSize];
        for (int r = 0; r < targetGridSize; r++)
            for (int c = 0; c < targetGridSize; c++)
            {
                int id = Grid[r, c].ChosenTileID;
                if (id == -1 || !Grid[r, c].Collapsed)
                    id = AllTileIDs[Random.Range(0, AllTileIDs.Count)];
                GeneratedContextMatrix[r, c] = id;
            }
        yield return null;
    }

    IEnumerator ProbabilisticWFC()
    {
        int[,] sourceMatrix = null;
        if (UseCustomMatrix && ManualMatrix != null && ManualMatrix.rows.Count > 0)
            sourceMatrix = ManualMatrix.ToArray();
        else
        {
            yield return StartCoroutine(SimpleTiledWFC(SimpleTiledContextGridSize, false));
            sourceMatrix = GeneratedContextMatrix;
        }
        if (sourceMatrix == null) yield break;

        adjacencyProbs = ComputeAdjacencyProbabilities(sourceMatrix);
        int[,] result = GenerateMatrixByProbability(FinalOverlappingGridSize, sourceMatrix, adjacencyProbs);
        RenderGeneratedMatrix(result);
        yield return null;
    }

    Dictionary<int, Dictionary<string, Dictionary<int, float>>> ComputeAdjacencyProbabilities(int[,] matrix)
    {
        var result = new Dictionary<int, Dictionary<string, Dictionary<int, float>>>();
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);
        string[] dirs = { "UP", "DOWN", "LEFT", "RIGHT" };

        foreach (string d in dirs)
            foreach (int val in matrix)
                if (!result.ContainsKey(val))
                    result[val] = new Dictionary<string, Dictionary<int, float>>()
                    {
                        {"UP", new Dictionary<int,float>()},
                        {"DOWN", new Dictionary<int,float>()},
                        {"LEFT", new Dictionary<int,float>()},
                        {"RIGHT", new Dictionary<int,float>()}
                    };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int v = matrix[y, x];
                if (y > 0) AddProb(result[v]["UP"], matrix[y - 1, x]);
                if (y < h - 1) AddProb(result[v]["DOWN"], matrix[y + 1, x]);
                if (x > 0) AddProb(result[v]["LEFT"], matrix[y, x - 1]);
                if (x < w - 1) AddProb(result[v]["RIGHT"], matrix[y, x + 1]);
            }
        }

        foreach (var valPair in result)
        {
            foreach (var dirPair in valPair.Value)
            {
                float total = dirPair.Value.Values.Sum();
                if (total > 0)
                {
                    var keys = dirPair.Value.Keys.ToList();
                    foreach (var k in keys)
                        dirPair.Value[k] /= total;
                }
            }
        }

        return result;
    }

    void AddProb(Dictionary<int, float> dict, int neighbor)
    {
        if (!dict.ContainsKey(neighbor)) dict[neighbor] = 0;
        dict[neighbor]++;
    }

    int[,] GenerateMatrixByProbability(int size, int[,] source, Dictionary<int, Dictionary<string, Dictionary<int, float>>> probs)
    {
        int[,] result = new int[size, size];
        int[] allValues = source.Cast<int>().Distinct().ToArray();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (y == 0 && x == 0)
                {
                    result[y, x] = allValues[Random.Range(0, allValues.Length)];
                    continue;
                }

                Dictionary<int, float> combined = new Dictionary<int, float>();

                void MergeProb(int centerVal, string dir)
                {
                    if (!probs.ContainsKey(centerVal)) return;
                    foreach (var kv in probs[centerVal][dir])
                    {
                        if (!combined.ContainsKey(kv.Key)) combined[kv.Key] = 0;
                        combined[kv.Key] += kv.Value;
                    }
                }

                if (y > 0) MergeProb(result[y - 1, x], "DOWN");
                if (x > 0) MergeProb(result[y, x - 1], "RIGHT");

                if (combined.Count == 0)
                    result[y, x] = allValues[Random.Range(0, allValues.Length)];
                else
                {
                    float total = combined.Values.Sum();
                    float rand = Random.Range(0f, total);
                    float acc = 0;
                    foreach (var kv in combined)
                    {
                        acc += kv.Value;
                        if (rand <= acc)
                        {
                            result[y, x] = kv.Key;
                            break;
                        }
                    }
                }
            }
        }
        return result;
    }

    void RenderGeneratedMatrix(int[,] matrix)
    {
        CleanupPreviousMap();
        MapContainer = new GameObject("GeneratedMapContainer");
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);
        float spacing = TileSize;
        if (Mathf.Approximately(spacing, 0f)) spacing = GetDefaultSpacing();
        float halfMapWorld = (w - 1) * spacing * 0.5f;
        for (int r = 0; r < h; r++)
        {
            for (int c = 0; c < w; c++)
            {
                int id = matrix[r, c];
                Tile tile = AllTiles.FirstOrDefault(t => t.ID == id);
                if (tile == null || tile.Prefab == null) continue;
                Vector3 pos = new Vector3(c * spacing - halfMapWorld, halfMapWorld - r * spacing, 0f);
                Instantiate(tile.Prefab, pos, tile.Prefab.transform.rotation, MapContainer.transform);
            }
        }
    }


    void InitializeGrid(int size)
    {
        GridSize = size;
        CleanupPreviousMap();
        HistoryStack.Clear();
        MapContainer = new GameObject("GeneratedMapContainer");
        AllTileIDs = AllTiles.Select(t => t.ID).ToList();
        Grid = new Cell[GridSize, GridSize];
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                Grid[r, c] = new Cell(r, c, AllTileIDs);
    }

    void CleanupPreviousMap()
    {
        GameObject old = GameObject.Find("GeneratedMapContainer");
        if (old != null) Destroy(old);
    }

    void SaveState()
    {
        Cell[,] newState = new Cell[GridSize, GridSize];
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                newState[r, c] = Grid[r, c].DeepCopy();
        HistoryStack.Push(newState);
    }

    Cell FindCellWithMinEntropy()
    {
        int minEntropy = int.MaxValue;
        List<Cell> minCells = new List<Cell>();
        foreach (Cell cell in Grid)
        {
            if (!cell.Collapsed)
            {
                if (cell.Entropy < minEntropy)
                {
                    minEntropy = cell.Entropy;
                    minCells.Clear();
                    minCells.Add(cell);
                }
                else if (cell.Entropy == minEntropy)
                    minCells.Add(cell);
            }
        }
        if (minCells.Count > 0)
            return minCells[Random.Range(0, minCells.Count)];
        return null;
    }

    void CollapseCellSimple(Cell cell, bool render)
    {
        int chosen = cell.PossibleTileIDs[Random.Range(0, cell.PossibleTileIDs.Count)];
        cell.ChosenTileID = chosen;
        cell.PossibleTileIDs.Clear();
        cell.PossibleTileIDs.Add(chosen);
        cell.Collapsed = true;
        if (render)
        {
            Tile t = AllTiles.FirstOrDefault(tt => tt.ID == chosen);
            if (t != null && t.Prefab != null)
            {
                float spacing = TileSize;
                if (Mathf.Approximately(spacing, 0f)) spacing = GetDefaultSpacing();
                float offset = (GridSize - 1) / 2f;
                Vector3 pos = new Vector3(cell.Col * spacing - offset * spacing, offset * spacing - cell.Row * spacing, 0);
                Instantiate(t.Prefab, pos, t.Prefab.transform.rotation, MapContainer.transform);
            }
        }
    }


    float GetDefaultSpacing()
    {
        foreach (var t in AllTiles)
        {
            if (t == null || t.Prefab == null) continue;
            Renderer rend = t.Prefab.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                float s = rend.bounds.size.x;
                if (s > 0.001f) return s;
            }
        }
        return 1f;
    }


    void Propagate(Cell start)
    {
        var stack = new Stack<Cell>();
        stack.Push(start);
        while (stack.Count > 0)
        {
            Cell currentCell = stack.Pop();
            int r = currentCell.Row;
            int c = currentCell.Col;
            (int dr, int dc, string direction)[] neighbors =
            {(-1,0,"UP"),(1,0,"DOWN"),(0,-1,"LEFT"),(0,1,"RIGHT")};
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
                            stack.Push(neighborCell);
                        if (neighborCell.PossibleTileIDs.Count == 0)
                            neighborCell.PossibleTileIDs.AddRange(AllTileIDs);
                    }
                }
            }
        }
    }

    private List<int> EnforceConstraints(Cell sourceCell, Cell targetCell, string relation)
    {
        List<int> removableIDs = new List<int>();
        foreach (int targetID in targetCell.PossibleTileIDs.ToList())
        {
            bool isCompatible = false;
            foreach (int sourceID in sourceCell.PossibleTileIDs)
            {
                List<int> requiredCompatibility = new List<int>();
                if (CurrentRunMode == GenerationMode.SimpleTiled)
                {
                    Tile sourceTile = AllTiles.FirstOrDefault(t => t.ID == sourceID);
                    if (sourceTile == null) continue;
                    switch (relation)
                    {
                        case "UP": requiredCompatibility = sourceTile.UpCompatibility; break;
                        case "DOWN": requiredCompatibility = sourceTile.DownCompatibility; break;
                        case "LEFT": requiredCompatibility = sourceTile.LeftCompatibility; break;
                        case "RIGHT": requiredCompatibility = sourceTile.RightCompatibility; break;
                    }
                }
                if (requiredCompatibility.Contains(targetID))
                {
                    isCompatible = true;
                    break;
                }
            }
            if (!isCompatible) removableIDs.Add(targetID);
        }
        foreach (int id in removableIDs) targetCell.RemoveOption(id);
        if (targetCell.PossibleTileIDs.Count == 0) targetCell.PossibleTileIDs.AddRange(AllTileIDs);
        return removableIDs;
    }

    bool CheckIfDone()
    {
        foreach (Cell c in Grid)
            if (!c.Collapsed) return false;
        return true;
    }
}
