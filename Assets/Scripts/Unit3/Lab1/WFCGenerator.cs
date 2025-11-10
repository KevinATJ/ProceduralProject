using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WFCGenerator : MonoBehaviour
{
    public enum GenerationMode { SimpleTiled, ComplexWFC, MarkovNGram }

    [Header("Configuración del Modo")]
    public GenerationMode Mode = GenerationMode.SimpleTiled;
    public bool UseCustomMatrix = false;
    public EditableMatrix ManualMatrix;

    [Header("Configuración general")]
    public Tile[] AllTiles;
    public int SimpleTiledContextGridSize = 10;
    public int FinalOverlappingGridSize = 10;
    public float StepDelay = 0.05f;
    public float TileSize = 1f;

    [Header("Visualización Complex WFC")]
    public float ContextMatrixDisplayTime = 2f;

    [Header("Markov N-gram")]
    [Range(1, 5)] public int MarkovColumnN = 2;
    public int MarkovColumnGridWidth = 10;
    public bool MarkovColumnStepByStep = false;
    public float MarkovColumnStepDelay = 0.02f;

    private Cell[,] Grid;
    private List<int> AllTileIDs;
    private Stack<Cell[,]> HistoryStack = new Stack<Cell[,]>();
    private GameObject MapContainer;
    public int[,] GeneratedContextMatrix;
    private int GridSize;
    private GenerationMode CurrentRunMode;
    private Dictionary<int, Dictionary<string, Dictionary<int, float>>> adjacencyProbs;
    private Dictionary<string, Dictionary<string, int>> markovModel;

    void Start()
    {
        if (AllTiles == null || AllTiles.Length == 0) return;
        CleanupPreviousMap();
        if (Mode == GenerationMode.SimpleTiled)
        {
            CurrentRunMode = GenerationMode.SimpleTiled;
            StartCoroutine(SimpleTiledWFC(SimpleTiledContextGridSize, true));
        }
        else if (Mode == GenerationMode.ComplexWFC)
        {
            CurrentRunMode = GenerationMode.ComplexWFC;
            StartCoroutine(ProbabilisticWFC());
        }
        else if (Mode == GenerationMode.MarkovNGram)
        {
            CurrentRunMode = GenerationMode.MarkovNGram;
            StartCoroutine(MarkovColumnCoroutine());
        }
    }

    public IEnumerator SimpleTiledWFC(int targetGridSize, bool isFinalRender)
    {
        CurrentRunMode = GenerationMode.SimpleTiled;
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
        CurrentRunMode = GenerationMode.ComplexWFC;

        if (UseCustomMatrix && ManualMatrix != null && ManualMatrix.rows.Count > 0)
            sourceMatrix = ManualMatrix.ToArray();
        else
        {
            yield return StartCoroutine(SimpleTiledWFC(SimpleTiledContextGridSize, false));
            sourceMatrix = GeneratedContextMatrix;
        }
        if (sourceMatrix == null) yield break;

        RenderGeneratedMatrix(sourceMatrix);
        yield return new WaitForSeconds(ContextMatrixDisplayTime);

        CleanupPreviousMap();
        adjacencyProbs = ComputeAdjacencyProbabilities(sourceMatrix);

        AllTileIDs = sourceMatrix.Cast<int>().Distinct().ToList();
        InitializeGrid(FinalOverlappingGridSize);

        int safetyCounter = 0;
        int safetyLimit = FinalOverlappingGridSize * FinalOverlappingGridSize * 50;

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
                    InitializeGrid(FinalOverlappingGridSize);
                    yield return null;
                    continue;
                }
            }

            SaveState();
            CollapseCellProbabilistic(cellToCollapse, true);
            Propagate(cellToCollapse);

            yield return new WaitForSeconds(StepDelay);
        }
        yield return null;
    }

    IEnumerator MarkovColumnCoroutine()
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

        RenderGeneratedMatrix(sourceMatrix);
        yield return new WaitForSeconds(ContextMatrixDisplayTime);
        CleanupPreviousMap();

        LearnMarkovFromMatrixColumnMajor(sourceMatrix, MarkovColumnN);

        int h = sourceMatrix.GetLength(0);
        int targetWidth = MarkovColumnGridWidth;
        float spacing = TileSize;
        if (Mathf.Approximately(spacing, 0f)) spacing = GetDefaultSpacing();
        float halfMapWorld = (targetWidth - 1) * spacing * 0.5f;

        MapContainer = new GameObject("GeneratedMapContainer");

        string currentKey = markovModel.Keys.ElementAt(Random.Range(0, markovModel.Keys.Count));
        List<string> contextCols = currentKey.Split('|').ToList();

        for (int col = 0; col < targetWidth; col++)
        {
            string currentColStr = contextCols.Last();
            int[] colVals = currentColStr.Split(',').Select(int.Parse).ToArray();

            for (int r = 0; r < h; r++)
            {
                int id = colVals[r];
                Tile t = AllTiles.FirstOrDefault(tt => tt.ID == id);
                if (t == null || t.Prefab == null) continue;

                Vector3 pos = new Vector3(col * spacing - halfMapWorld, (h - 1) * spacing * 0.5f - r * spacing, 0f);
                Instantiate(t.Prefab, pos, t.Prefab.transform.rotation, MapContainer.transform);
            }

            if (!markovModel.ContainsKey(currentKey))
                break;

            string nextCol = WeightedPickString(markovModel[currentKey]);
            contextCols.Add(nextCol);
            if (contextCols.Count > MarkovColumnN)
                contextCols.RemoveAt(0);
            currentKey = string.Join("|", contextCols);

            if (MarkovColumnStepByStep)
                yield return new WaitForSeconds(MarkovColumnStepDelay);
        }

        yield return null;
    }

    void LearnMarkovFromMatrixColumnMajor(int[,] matrix, int N)
    {
        markovModel = new Dictionary<string, Dictionary<string, int>>();
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);

        List<string> columnStrings = new List<string>();
        for (int c = 0; c < w; c++)
        {
            List<int> colVals = new List<int>();
            for (int r = 0; r < h; r++)
                colVals.Add(matrix[r, c]);
            columnStrings.Add(string.Join(",", colVals));
        }

        if (columnStrings.Count <= N) return;
        for (int i = 0; i <= columnStrings.Count - N - 1; i++)
        {
            string key = string.Join("|", columnStrings.Skip(i).Take(N));
            string next = columnStrings[i + N];

            if (!markovModel.ContainsKey(key))
                markovModel[key] = new Dictionary<string, int>();
            if (!markovModel[key].ContainsKey(next))
                markovModel[key][next] = 0;
            markovModel[key][next]++;
        }
    }

    string WeightedPickString(Dictionary<string, int> dict)
    {
        int total = dict.Values.Sum();
        int roll = Random.Range(0, total);
        int cumulative = 0;
        foreach (var pair in dict)
        {
            cumulative += pair.Value;
            if (roll < cumulative)
                return pair.Key;
        }
        return dict.Keys.First();
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
        if (CurrentRunMode == GenerationMode.SimpleTiled || AllTileIDs == null || AllTileIDs.Count == 0)
            AllTileIDs = AllTiles.Select(t => t.ID).ToList();
        Grid = new Cell[GridSize, GridSize];
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                Grid[r, c] = new Cell(r, c, AllTileIDs);
    }

    void CleanupPreviousMap()
    {
        GameObject old = GameObject.Find("GeneratedMapContainer");
        if (old != null) GameObject.Destroy(old);
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
        if (cell.PossibleTileIDs == null || cell.PossibleTileIDs.Count == 0)
            cell.PossibleTileIDs = new List<int>(AllTileIDs);
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

    void CollapseCellProbabilistic(Cell cell, bool render)
    {
        Dictionary<int, float> combinedProbabilities = new Dictionary<int, float>();
        (int dr, int dc, string direction)[] neighbors =
        {(-1,0,"UP"),(1,0,"DOWN"),(0,-1,"LEFT"),(0,1,"RIGHT")};
        float neighborCount = 0;
        foreach (var (dr, dc, direction) in neighbors)
        {
            int nr = cell.Row + dr;
            int nc = cell.Col + dc;
            if (nr >= 0 && nr < GridSize && nc >= 0 && nc < GridSize)
            {
                Cell neighbor = Grid[nr, nc];
                if (neighbor.Collapsed)
                {
                    neighborCount++;
                    int neighborID = neighbor.ChosenTileID;
                    string inverseDir = "";
                    if (direction == "UP") inverseDir = "DOWN";
                    else if (direction == "DOWN") inverseDir = "UP";
                    else if (direction == "LEFT") inverseDir = "RIGHT";
                    else if (direction == "RIGHT") inverseDir = "LEFT";
                    if (adjacencyProbs.ContainsKey(neighborID) && adjacencyProbs[neighborID].ContainsKey(inverseDir))
                    {
                        foreach (var kvp in adjacencyProbs[neighborID][inverseDir])
                        {
                            int targetID = kvp.Key;
                            float prob = kvp.Value;
                            if (cell.PossibleTileIDs.Contains(targetID))
                            {
                                if (!combinedProbabilities.ContainsKey(targetID))
                                    combinedProbabilities[targetID] = 0f;
                                combinedProbabilities[targetID] += prob;
                            }
                        }
                    }
                }
            }
        }
        if (combinedProbabilities.Count == 0)
        {
            foreach (int id in cell.PossibleTileIDs)
                combinedProbabilities[id] = 1f;
        }
        int chosen = ChooseTileByWeightedRandom(combinedProbabilities);
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

    private int ChooseTileByWeightedRandom(Dictionary<int, float> weights)
    {
        if (weights.Count == 0) return AllTileIDs[Random.Range(0, AllTileIDs.Count)];
        float totalWeight = weights.Values.Sum();
        if (totalWeight <= 0) return weights.Keys.First();
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        foreach (var kvp in weights)
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight) return kvp.Key;
        }
        return weights.Keys.Last();
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
                    if (requiredCompatibility.Contains(targetID))
                    {
                        isCompatible = true;
                        break;
                    }
                }
                else if (CurrentRunMode == GenerationMode.ComplexWFC)
                {
                    if (adjacencyProbs.ContainsKey(sourceID) &&
                        adjacencyProbs[sourceID].ContainsKey(relation) &&
                        adjacencyProbs[sourceID][relation].ContainsKey(targetID) &&
                        adjacencyProbs[sourceID][relation][targetID] > 0)
                    {
                        isCompatible = true;
                        break;
                    }
                }
                else if (CurrentRunMode == GenerationMode.MarkovNGram)
                {
                    if (AllTileIDs.Contains(targetID))
                    {
                        isCompatible = true;
                        break;
                    }
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