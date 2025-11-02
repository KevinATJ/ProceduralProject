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
    public int PatternSize = 3;

    [Header("Configuración")]
    public Tile[] AllTiles;
    public int SimpleTiledContextGridSize = 10;
    public int FinalOverlappingGridSize = 20;
    public float StepDelay = 0.05f;
    public float TileSize = 1f;

    private Cell[,] Grid;
    private List<int> AllTileIDs;
    private Stack<Cell[,]> HistoryStack = new Stack<Cell[,]>();
    private GameObject MapContainer;
    private List<PatternData> AllPatterns = new List<PatternData>();
    private int[,] GeneratedContextMatrix;
    private int GridSize;
    private GenerationMode CurrentRunMode;

    void Start()
    {
        if (AllTiles == null || AllTiles.Length == 0) return;
        CleanupPreviousMap();
        if (Mode == GenerationMode.SimpleTiled)
            StartCoroutine(SimpleTiledWFC(SimpleTiledContextGridSize, true));
        else if (Mode == GenerationMode.ComplexWFC)
            StartCoroutine(StartComplexWFC());
    }

    public IEnumerator StartComplexWFC()
    {
        int[,] sourceMatrix = null;
        if (UseCustomMatrix)
        {
            if (ManualMatrix != null && ManualMatrix.rows != null && ManualMatrix.rows.Count > 0)
                sourceMatrix = ManualMatrix.ToArray();
            else yield break;
        }
        else
        {
            yield return StartCoroutine(SimpleTiledWFC(SimpleTiledContextGridSize, false));
            sourceMatrix = GeneratedContextMatrix;
        }
        if (sourceMatrix == null) yield break;
        yield return StartCoroutine(OverlappingWFC(FinalOverlappingGridSize, true, sourceMatrix));
    }

    public IEnumerator SimpleTiledWFC(int targetGridSize, bool isFinalRender)
    {
        InitializeGrid(GenerationMode.SimpleTiled, targetGridSize, null);
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
                    InitializeGrid(GenerationMode.SimpleTiled, targetGridSize, null);
                    yield return null;
                    continue;
                }
            }
            SaveState();
            CollapseCell(cellToCollapse, isFinalRender);
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

    public IEnumerator OverlappingWFC(int targetGridSize, bool isFinalRender, int[,] sourceMatrix)
    {
        InitializeGrid(GenerationMode.ComplexWFC, targetGridSize, sourceMatrix);
        if (AllPatterns == null || AllPatterns.Count == 0) yield break;
        int safetyCounter = 0;
        int safetyLimit = targetGridSize * targetGridSize * 200;
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
                    InitializeGrid(GenerationMode.ComplexWFC, targetGridSize, sourceMatrix);
                    yield return null;
                    continue;
                }
            }
            SaveState();
            CollapseCell(cellToCollapse, isFinalRender);
            Propagate(cellToCollapse);
            if (isFinalRender) yield return new WaitForSeconds(StepDelay);
        }
        yield return null;
    }

    private void InitializeGrid(GenerationMode initMode, int initGridSize, int[,] contextMatrix)
    {
        GridSize = initGridSize;
        CurrentRunMode = initMode;
        CleanupPreviousMap();
        HistoryStack.Clear();
        MapContainer = new GameObject("GeneratedMapContainer");
        Grid = new Cell[GridSize, GridSize];
        if (initMode == GenerationMode.SimpleTiled)
            AllTileIDs = AllTiles.Select(t => t.ID).ToList();
        else
        {
            AllPatterns = ExtractPatternsAndRules(contextMatrix) ?? new List<PatternData>();
            AllTileIDs = AllPatterns.Select(p => p.ID).ToList();
            if (AllTileIDs == null || AllTileIDs.Count == 0) AllTileIDs = AllTiles.Select(t => t.ID).ToList();
        }
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                Grid[r, c] = new Cell(r, c, AllTileIDs);
    }

    private void CleanupPreviousMap()
    {
        GameObject oldMap = GameObject.Find("GeneratedMapContainer");
        if (oldMap != null) Destroy(oldMap);
    }

    private void SaveState()
    {
        Cell[,] newState = new Cell[GridSize, GridSize];
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                newState[r, c] = Grid[r, c].DeepCopy();
        HistoryStack.Push(newState);
    }

    private Cell FindCellWithMinEntropy()
    {
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
                    minimumEntropyCells.Add(cell);
            }
        }
        if (minimumEntropyCells.Count > 0)
            return minimumEntropyCells[Random.Range(0, minimumEntropyCells.Count)];
        return null;
    }

    private void CollapseCell(Cell cell, bool isFinalRender)
    {
        int chosenID;
        if (cell.PossibleTileIDs == null || cell.PossibleTileIDs.Count == 0)
        {
            chosenID = AllTileIDs[Random.Range(0, AllTileIDs.Count)];
        }
        else
        {
            chosenID = CurrentRunMode == GenerationMode.SimpleTiled
                ? cell.PossibleTileIDs[Random.Range(0, cell.PossibleTileIDs.Count)]
                : GetWeightedRandomTileID(cell.PossibleTileIDs);
        }
        cell.ChosenTileID = chosenID;
        cell.PossibleTileIDs.Clear();
        cell.PossibleTileIDs.Add(chosenID);
        cell.Collapsed = true;
        if (isFinalRender)
        {
            if (CurrentRunMode == GenerationMode.SimpleTiled)
            {
                Tile chosenTile = AllTiles.FirstOrDefault(t => t.ID == cell.ChosenTileID);
                if (chosenTile != null && chosenTile.Prefab != null)
                {
                    float offset = (GridSize - 1) / 2f;
                    float size = TileSize;
                    if (Mathf.Approximately(TileSize, 0f))
                    {
                        Renderer rend = chosenTile.Prefab.GetComponentInChildren<Renderer>();
                        size = (rend != null) ? rend.bounds.size.x : 1f;
                    }
                    Vector3 position = new Vector3(cell.Col * size - offset * size, offset * size - cell.Row * size, 0);
                    Instantiate(chosenTile.Prefab, position, chosenTile.Prefab.transform.rotation, MapContainer.transform);
                }
            }
            else
            {
                PatternData chosenPattern = AllPatterns.FirstOrDefault(p => p.ID == cell.ChosenTileID);
                if (chosenPattern != null) RenderPattern(cell, chosenPattern);
            }
        }
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
                int dirIndex = 0;
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
                else
                {
                    PatternData sourcePattern = AllPatterns.FirstOrDefault(p => p.ID == sourceID);
                    if (sourcePattern == null) continue;
                    switch (relation)
                    {
                        case "UP": dirIndex = 0; break;
                        case "DOWN": dirIndex = 1; break;
                        case "LEFT": dirIndex = 2; break;
                        case "RIGHT": dirIndex = 3; break;
                    }
                    requiredCompatibility = sourcePattern.Adjacency[dirIndex];
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

    private bool CheckIfDone()
    {
        foreach (Cell cell in Grid)
            if (!cell.Collapsed) return false;
        return true;
    }

    private int GetWeightedRandomTileID(List<int> possibleIDs)
    {
        if (possibleIDs == null || possibleIDs.Count == 0) return -1;
        float totalWeight = 0f;
        foreach (int id in possibleIDs)
        {
            PatternData pattern = AllPatterns?.FirstOrDefault(p => p.ID == id);
            if (pattern != null) totalWeight += Mathf.Max(0f, pattern.Weight);
        }
        if (totalWeight <= 0f) return possibleIDs[Random.Range(0, possibleIDs.Count)];
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        foreach (int id in possibleIDs)
        {
            PatternData pattern = AllPatterns?.FirstOrDefault(p => p.ID == id);
            if (pattern == null) continue;
            currentWeight += Mathf.Max(0f, pattern.Weight);
            if (randomValue < currentWeight) return id;
        }
        return possibleIDs.Last();
    }

    private List<PatternData> ExtractPatternsAndRules(int[,] contextMatrix)
    {
        if (contextMatrix == null) return new List<PatternData>();
        int sampleHeight = contextMatrix.GetLength(0);
        int sampleWidth = contextMatrix.GetLength(1);
        int patternIDCounter = 0;
        int N = PatternSize;
        if (sampleWidth < N || sampleHeight < N) return new List<PatternData>();
        var uniquePatterns = new Dictionary<string, PatternData>();
        var patternCounts = new Dictionary<string, int>();
        for (int y = 0; y <= sampleHeight - N; y++)
        {
            for (int x = 0; x <= sampleWidth - N; x++)
            {
                int[] currentPatternArray = new int[N * N];
                int index = 0;
                for (int dy = 0; dy < N; dy++)
                    for (int dx = 0; dx < N; dx++)
                        currentPatternArray[index++] = contextMatrix[y + dy, x + dx];
                string patternKey = string.Join(",", currentPatternArray);
                if (!uniquePatterns.ContainsKey(patternKey))
                {
                    PatternData newPattern = new PatternData(patternIDCounter++, currentPatternArray, 0f);
                    uniquePatterns.Add(patternKey, newPattern);
                    patternCounts.Add(patternKey, 0);
                }
                patternCounts[patternKey]++;
            }
        }
        foreach (var kvp in uniquePatterns)
            kvp.Value.Weight = patternCounts[kvp.Key];
        int maxOverlapY = sampleHeight - N;
        int maxOverlapX = sampleWidth - N;
        for (int y = 0; y <= maxOverlapY; y++)
        {
            for (int x = 0; x <= maxOverlapX; x++)
            {
                string currentKey = GetPatternKey(contextMatrix, x, y, N);
                PatternData currentPattern = uniquePatterns[currentKey];
                (int dx, int dy, int dirIndex)[] neighbors = { (0, -1, 0), (0, 1, 1), (-1, 0, 2), (1, 0, 3) };
                foreach (var (dx, dy, dirIndex) in neighbors)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx <= maxOverlapX && ny >= 0 && ny <= maxOverlapY)
                    {
                        string neighborKey = GetPatternKey(contextMatrix, nx, ny, N);
                        PatternData neighborPattern = uniquePatterns[neighborKey];
                        if (!currentPattern.Adjacency[dirIndex].Contains(neighborPattern.ID))
                            currentPattern.Adjacency[dirIndex].Add(neighborPattern.ID);
                    }
                }
            }
        }
        foreach (var pattern in uniquePatterns.Values)
            for (int i = 0; i < 4; i++)
                if (pattern.Adjacency[i].Count == 0)
                    pattern.Adjacency[i].AddRange(uniquePatterns.Values.Select(p => p.ID));
        return uniquePatterns.Values.ToList();
    }

    private string GetPatternKey(int[,] grid, int startX, int startY, int N)
    {
        int[] patternArray = new int[N * N];
        int index = 0;
        for (int dy = 0; dy < N; dy++)
            for (int dx = 0; dx < N; dx++)
                patternArray[index++] = grid[startY + dy, startX + dx];
        return string.Join(",", patternArray);
    }

    private void RenderPattern(Cell cell, PatternData pattern)
    {
        int N = PatternSize;
        if (pattern == null || pattern.Pattern == null || pattern.Pattern.Length != N * N) return;
        int tileToRenderID = pattern.Pattern[0];
        Tile tileToRender = AllTiles.FirstOrDefault(t => t.ID == tileToRenderID);
        if (tileToRender == null || tileToRender.Prefab == null) return;
        float size = TileSize;
        if (Mathf.Approximately(size, 0f))
        {
            Renderer rend = tileToRender.Prefab.GetComponentInChildren<Renderer>();
            size = (rend != null) ? rend.bounds.size.x : 1f;
        }
        float halfMapWorld = (GridSize - 1) * size * 0.5f;
        float xPos = cell.Col * size - halfMapWorld;
        float yPos = halfMapWorld - cell.Row * size;
        if (Mathf.Approximately(size, 1f))
        {
            xPos = Mathf.Round(xPos);
            yPos = Mathf.Round(yPos);
        }
        Vector3 finalPosition = new Vector3(xPos, yPos, 0f);
        Instantiate(tileToRender.Prefab, finalPosition, tileToRender.Prefab.transform.rotation, MapContainer.transform);
    }
}
