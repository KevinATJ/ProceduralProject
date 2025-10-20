using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GenerationMethod { GoalBackward, HillClimbing, GeneticAlgorithm }
public class PuzzleController : MonoBehaviour
{
    [Header("Configuración de Piezas")]
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private float shuffleTime = 1f;
    private float debugDelay = 0.2f;

    [Header("Goal Backward")]
    [SerializeField] private int goalBackwardIterations = 100;

    [Header("Hill Climbing")]
    [SerializeField] private int hillClimbingIterations = 100;

    [Header("GA")]
    [SerializeField] private int populationSize = 50;
    [SerializeField][Range(0f, 1f)] private float crossoverRate = 0.7f;
    [SerializeField][Range(0f, 1f)] private float mutationRate = 0.05f;
    [SerializeField] private int maxGenerations = 20;

    [Header("Seed (opcional)")]
    [SerializeField] private bool useSeed = false;
    [SerializeField] private int seed = 0;

    private System.Random rng;

    private Transform gameTransform;
    private int size = 4;
    private GenerationMethod generationMethod;  
    public class GenerationResult
    {
        public float TimeSeconds { get; set; }
        public int FinalFitness { get; set; }
    }

    private List<Transform> pieces;
    private int emptyLocation;
    private bool shuffling = false;
    private class State
    {
        public string[] Order;
        public int EmptyIndex;

        public State(List<Transform> currentPieces, int emptyLocation)
        {
            Order = new string[currentPieces.Count];
            for (int i = 0; i < currentPieces.Count; i++)
            {
                Order[i] = currentPieces[i].name;
            }
            EmptyIndex = emptyLocation;
        }

        public State Clone()
        {
            return new State
            {
                Order = (string[])Order.Clone(),
                EmptyIndex = EmptyIndex
            };
        }

        public State() { }
    }
    public void Initialize(int newSize, GenerationMethod method, Transform spawnPoint)
    {
        if (newSize < 2) newSize = 2;
        size = newSize;
        generationMethod = method;
        gameTransform = spawnPoint;

        if (useSeed)
        {
            rng = new System.Random(seed);
        }
        else
        {
            int generatedSeed = Environment.TickCount ^ this.GetInstanceID();
            seed = generatedSeed;
            rng = new System.Random(seed);
        }

        Debug.Log($"[{name}] Initialize method={generationMethod} UseSeed={useSeed} Seed={seed}");

        pieces = new List<Transform>();
        CreateGamePieces(0.01f);

        CurrentGoalBackwardIteration = 0;
        CurrentHillClimbingIteration = 0;
        CurrentHillClimbingBestFitness = 0;
        CurrentGeneration = 0;
        CurrentGABestFitness = 0;

        StartCoroutine(StartGeneration());
    }

    private IEnumerator StartGeneration()
    {
        shuffling = true;
        yield return new WaitForSeconds(shuffleTime);

        if (generationMethod == GenerationMethod.GoalBackward)
        {
            yield return StartCoroutine(GenerateGoalBackwardStepByStep());
        }
        else if (generationMethod == GenerationMethod.HillClimbing)
        {
            yield return StartCoroutine(HillClimbing_GenerateStepByStep());
        }
        else if (generationMethod == GenerationMethod.GeneticAlgorithm)
        {
            yield return StartCoroutine(GeneticAlgorithm());
        }
    }

    #region General

    private void CreateGamePieces(float gapThickness)
    {
        if (pieces == null)
        {
            pieces = new List<Transform>();
        }
        else
        {
            foreach (Transform piece in pieces)
            {
                if (piece != null) Destroy(piece.gameObject);
            }
            pieces.Clear();
        }

        float width = 1 / (float)size;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);

                piece.localPosition = new Vector3(-1 + (2 * width * col) + width, +1 - (2 * width * row) - width, 0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";

                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                    mesh.uv = uv;
                }
            }
        }
    }

    void Update()
    {
        if (pieces == null || pieces.Count == 0)
        {
            return;
        }

        if (!shuffling && CheckCompletion())
        {
            shuffling = true;
            Debug.Log($"Puzzle {generationMethod} Resuelto");
            StartCoroutine(StartGeneration());
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.parent == gameTransform)
                {
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        if (pieces[i] == hit.transform)
                        {
                            if (SwapIfValid(i, -size, size)) { break; }
                            if (SwapIfValid(i, +size, size)) { break; }
                            if (SwapIfValid(i, -1, 0)) { break; }
                            if (SwapIfValid(i, +1, size - 1)) { break; }
                        }
                    }
                }
            }
        }
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);

            (pieces[i].localPosition, pieces[i + offset].localPosition) = ((pieces[i + offset].localPosition, pieces[i].localPosition));

            emptyLocation = i;
            return true;
        }
        return false;
    }

    private bool CheckCompletion()
    {
        if (pieces == null || pieces.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    private void DebugPuzzleState(State stateToDebug = null)
    {
        string[] order;
        int N = size * size;

        if (stateToDebug != null)
        {
            order = stateToDebug.Order;
        }
        else
        {
            order = new string[N];
            for (int i = 0; i < N; i++)
            {
                order[i] = pieces[i].name;
            }
        }

        string debugMessage = $"\n--- ESTADO FINAL ({generationMethod}) ---\n";
        debugMessage += $"Tamaño: {size}x{size}\n";

        string sequence = "";
        for (int i = 0; i < N; i++)
        {
            string value = order[i];

            if (order[i] == $"{N - 1}")
            {
                value = "  ";
            }

            sequence += $"{order[i]} ";
            debugMessage += $"{value.PadLeft(2, ' ')} ";
            if ((i + 1) % size == 0)
            {
                debugMessage += "\n";
            }
        }

        debugMessage += $"\nSecuencia de Nombres (Top-Left a Bottom-Right):\n[{sequence.Trim()}]";

        Debug.Log(debugMessage);
    }

    #endregion

    #region GOAL BACKWARD

    public int CurrentGoalBackwardIteration { get; private set; } = 0;

    private IEnumerator GenerateGoalBackwardStepByStep()
    {
        if (rng == null) rng = new System.Random();

        float startTime = Time.realtimeSinceStartup;
        int count = 0;
        CurrentGoalBackwardIteration = 0;
        int last = 0;
        while (count < goalBackwardIterations)
        {

            int rnd = rng.Next(0, size * size);
            if (rnd == last) { continue; }
            last = emptyLocation;
            bool moved = false;
            if (SwapIfValid(rnd, -size, size)) { moved = true; }
            else if (SwapIfValid(rnd, +size, size)) { moved = true; }
            else if (SwapIfValid(rnd, -1, 0)) { moved = true; }
            else if (SwapIfValid(rnd, +1, size - 1)) { moved = true; }

            if (moved)
            {
                count++;
                CurrentGoalBackwardIteration = count;

                yield return new WaitForSeconds(debugDelay);
            }
        }

        CurrentGoalBackwardIteration = goalBackwardIterations;

        float endTime = Time.realtimeSinceStartup;
        float timeSeconds = endTime - startTime;

        State finalCurrentState = new State(pieces, emptyLocation);
        int finalFitness = CalculateFitness(finalCurrentState);

        GenerationResult result = new GenerationResult
        {
            TimeSeconds = timeSeconds,
            FinalFitness = finalFitness
        };
        GBResult = result;

        Debug.Log($"Puzzle GoalBackward generado en {result.TimeSeconds:F4}s. Total Movimientos: {goalBackwardIterations}. Dificultad Final (Manhattan): {result.FinalFitness}");
        DebugPuzzleState();
        shuffling = false;
    }

    #endregion

    #region HILL CLIMBING

    public int CurrentHillClimbingIteration { get; private set; } = 0;
    public int CurrentHillClimbingBestFitness { get; private set; } = 0;

    private IEnumerator HillClimbing_GenerateStepByStep()
    {
        if (rng == null) rng = new System.Random();

        float startTime = Time.realtimeSinceStartup;
        State currentState = new State(pieces, emptyLocation);
        int currentFitness = CalculateFitness(currentState);

        State bestState = currentState.Clone();
        int bestFitness = currentFitness;

        CurrentHillClimbingIteration = 0;
        CurrentHillClimbingBestFitness = bestFitness;

        List<State> currentPath = new List<State> { currentState.Clone() };
        List<State> bestPath = new List<State> { currentState.Clone() };

        int[] offsets = { -size, +size, -1, +1 };
        int[] colChecks = { size, size, 0, size - 1 };

        for (int i = 0; i < hillClimbingIterations; i++)
        {
            //CurrentHillClimbingIteration = i + 1;

            int offsetIndex = rng.Next(offsets.Length);
            State neighbor = TryGenerateNeighbor(currentState, offsets[offsetIndex], colChecks[offsetIndex]);

            if (neighbor != null)
            {
                int neighborFitness = CalculateFitness(neighbor);



                if (neighborFitness > bestFitness)
                {
                    bestFitness = neighborFitness;
                    bestState = neighbor.Clone();
                    bestPath = new List<State>(currentPath);
                    CurrentHillClimbingBestFitness = bestFitness;
                }

                currentState = neighbor;
                currentPath.Add(currentState.Clone());
            }
        }

        //CurrentHillClimbingIteration = hillClimbingIterations;
        CurrentHillClimbingBestFitness = bestFitness;

        float generationEndTime = Time.realtimeSinceStartup;
        float generationTimeSeconds = generationEndTime - startTime;
        GenerationResult result = new GenerationResult
        {
            TimeSeconds = generationTimeSeconds,
            FinalFitness = bestFitness
        };
        HCResult = result;
        foreach (var state in bestPath)
        {
            CurrentHillClimbingIteration++;
            ApplyStateToGame(state);
            yield return new WaitForSeconds(debugDelay);
        }
        while ((hillClimbingIterations-CurrentHillClimbingIteration)>0)
        {
            CurrentHillClimbingIteration++;
            yield return new WaitForSeconds(debugDelay);
        }
        
        Debug.Log($"Puzzle HillClimbing generado con dificultad (Manhattan): {result.FinalFitness} en {result.TimeSeconds:F4}s. Total Iteraciones: {hillClimbingIterations}");
        DebugPuzzleState(bestState);
        shuffling = false;
    }

    private int CalculateFitness(State state)
    {
        int totalDistance = 0;

        for (int i = 0; i < state.Order.Length; i++)
        {
            if (state.Order[i] == $"{size * size - 1}")
                continue;

            int currentIndex = i;
            int targetIndex = int.Parse(state.Order[i]);

            int currentRow = currentIndex / size;
            int currentCol = currentIndex % size;

            int targetRow = targetIndex / size;
            int targetCol = targetIndex % size;

            totalDistance += Mathf.Abs(currentRow - targetRow) + Mathf.Abs(currentCol - targetCol);
        }

        return totalDistance;
    }


    private State TryGenerateNeighbor(State parentState, int offset, int colCheck)
    {
        int i = parentState.EmptyIndex - offset;
        int targetIndex = parentState.EmptyIndex;

        if (i >= 0 && i < parentState.Order.Length &&
            ((i % size) != colCheck) && (targetIndex == parentState.EmptyIndex))
        {
            State neighbor = parentState.Clone();

            string temp = neighbor.Order[i];
            neighbor.Order[i] = neighbor.Order[targetIndex];
            neighbor.Order[targetIndex] = temp;

            neighbor.EmptyIndex = i;
            return neighbor;
        }
        return null;
    }
    private void ApplyStateToGame(State finalState)
    {
        Dictionary<string, Transform> nameToTransform = new Dictionary<string, Transform>();
        foreach (Transform piece in pieces)
        {
            nameToTransform.Add(piece.name, piece);
        }

        for (int i = 0; i < finalState.Order.Length; i++)
        {
            string pieceName = finalState.Order[i];
            Transform pieceToPlace = nameToTransform[pieceName];

            pieces[i] = pieceToPlace;

            float width = 1 / (float)size;
            float x = -1 + (2 * width * (i % size)) + width;
            float y = +1 - (2 * width * (i / size)) - width;
            pieceToPlace.localPosition = new Vector3(x, y, 0);
        }

        emptyLocation = finalState.EmptyIndex;
    }

    #endregion

    #region GENETIC ALGORITHM

    public int CurrentGeneration { get; private set; } = 0;
    public int CurrentGABestFitness { get; private set; } = 0;

    private IEnumerator GeneticAlgorithm()
    {
        if (rng == null) rng = new System.Random();

        float startTime = Time.realtimeSinceStartup;
        List<State> population = InitializePopulation();

        State bestState = FindBestState(population);
        int bestFitness = CalculateFitness(bestState);

        CurrentGeneration = 0;
        CurrentGABestFitness = bestFitness;

        for (int gen = 0; gen < maxGenerations; gen++)
        {
            CurrentGeneration = gen + 1;

            List<State> newPopulation = new List<State>();

            while (newPopulation.Count < populationSize)
            {
                State parent1 = Selection(population, 3);
                State parent2 = Selection(population, 3);

                State child1 = parent1.Clone();
                State child2 = parent2.Clone();

                if (rng.NextDouble() < crossoverRate)
                {
                    (child1, child2) = OrderCrossover(parent1, parent2);
                }

                Mutate(child1);
                Mutate(child2);

                CorrectParity(child1);
                CorrectParity(child2);

                newPopulation.Add(child1);
                if (newPopulation.Count < populationSize)
                {
                    newPopulation.Add(child2);
                }
            }

            population = newPopulation;

            State currentGenBest = FindBestState(population);
            int currentGenBestFitness = CalculateFitness(currentGenBest);

            if (currentGenBestFitness > bestFitness)
            {
                bestFitness = currentGenBestFitness;
                bestState = currentGenBest;
            }

            CurrentGABestFitness = bestFitness;

            Debug.Log($"Gen {gen}: Mejor Fitness = {bestFitness}");

            ApplyStateToGame(bestState);
            yield return new WaitForSeconds(debugDelay);
        }

        if (maxGenerations > 0) CurrentGeneration = maxGenerations;
        CurrentGABestFitness = bestFitness;

        float endTime = Time.realtimeSinceStartup;
        float timeSeconds = endTime - startTime;

        GenerationResult result = new GenerationResult
        {
            TimeSeconds = timeSeconds,
            FinalFitness = bestFitness
        };
        GAResult = result;
        ApplyStateToGame(bestState);
        Debug.Log($"Puzzle AG generado con dificultad (Manhattan): {result.FinalFitness} en {result.TimeSeconds:F4}s. Total Generaciones: {maxGenerations}");
        DebugPuzzleState(bestState);
        shuffling = false;
    }
    private List<State> InitializePopulation()
    {
        List<State> population = new List<State>();

        State solvedState = new State(pieces, emptyLocation);

        for (int i = 0; i < populationSize; i++)
        {
            State newState = solvedState.Clone();

            SimulatedGoalBackward(newState, size * 2);

            population.Add(newState);
        }
        return population;
    }

    private State Selection(List<State> population, int size)
    {
        State best = null;
        int bestFitness = -1;

        for (int i = 0; i < size; i++)
        {
            int randomIndex = rng.Next(population.Count);
            State candidate = population[randomIndex];
            int fitness = CalculateFitness(candidate);

            if (fitness > bestFitness)
            {
                bestFitness = fitness;
                best = candidate;
            }
        }
        return best;
    }
    private (State, State) OrderCrossover(State parent1, State parent2)
    {
        State child1 = new State();
        State child2 = new State();
        int N = parent1.Order.Length;

        child1.Order = new string[N];
        child2.Order = new string[N];

        int start = rng.Next(N);
        int end = rng.Next(N);

        if (start > end) (start, end) = (end, start);

        for (int i = start; i <= end; i++)
        {
            child1.Order[i] = parent1.Order[i];
            child2.Order[i] = parent2.Order[i];
        }

        FillGaps(child1, parent2, start, end);
        FillGaps(child2, parent1, start, end);

        child1.EmptyIndex = System.Array.IndexOf(child1.Order, $"{N - 1}");
        child2.EmptyIndex = System.Array.IndexOf(child2.Order, $"{N - 1}");

        return (child1, child2);
    }

    private void FillGaps(State child, State parent, int start, int end)
    {
        int N = child.Order.Length;
        int parentIndex = (end + 1) % N;
        int childIndex = (end + 1) % N;

        while (childIndex != start)
        {
            string gene = parent.Order[parentIndex];

            bool alreadyCopied = false;
            for (int i = start; i <= end; i++)
            {
                if (child.Order[i] == gene)
                {
                    alreadyCopied = true;
                    break;
                }
            }

            if (!alreadyCopied)
            {
                child.Order[childIndex] = gene;
                childIndex = (childIndex + 1) % N;
            }

            parentIndex = (parentIndex + 1) % N;
        }
    }

    private void Mutate(State state)
    {
        if (rng.NextDouble() < mutationRate)
        {
            int N = state.Order.Length;
            int index1 = rng.Next(N);
            int index2 = rng.Next(N);

            string temp = state.Order[index1];
            state.Order[index1] = state.Order[index2];
            state.Order[index2] = temp;

            if (state.Order[index1] == $"{N - 1}")
            {
                state.EmptyIndex = index1;
            }
            else if (state.Order[index2] == $"{N - 1}")
            {
                state.EmptyIndex = index2;
            }
        }
    }
    private void SimulatedGoalBackward(State state, int iterations)
    {
        int count = 0;
        int lastEmptyIndex = -1;

        int[] offsets = { -size, +size, -1, +1 };
        int[] colChecks = { size, size, 0, size - 1 };

        while (count < iterations)
        {
            int empty = state.EmptyIndex;
            int offsetToUse = 0;
            int colCheckToUse = 0;

            int rndDirection = rng.Next(offsets.Length);

            offsetToUse = offsets[rndDirection];
            colCheckToUse = colChecks[rndDirection];

            int targetIndex = empty - offsetToUse;

            if (targetIndex >= 0 && targetIndex < state.Order.Length &&
                ((targetIndex % size) != colCheckToUse) && (targetIndex != lastEmptyIndex))
            {
                string temp = state.Order[targetIndex];
                state.Order[targetIndex] = state.Order[empty];
                state.Order[empty] = temp;

                lastEmptyIndex = empty;
                state.EmptyIndex = targetIndex;
                count++;
            }
        }
    }
    private State FindBestState(List<State> population)
    {
        State best = population[0];
        int bestFitness = CalculateFitness(best);

        for (int i = 1; i < population.Count; i++)
        {
            int currentFitness = CalculateFitness(population[i]);
            if (currentFitness > bestFitness)
            {
                bestFitness = currentFitness;
                best = population[i];
            }
        }
        return best;
    }
    private int CountInversions(State state)
    {
        int inversions = 0;
        int N = state.Order.Length;
        int emptyTileId = N - 1;

        List<int> pieces = new List<int>();
        for (int i = 0; i < N; i++)
        {
            int tileId = int.Parse(state.Order[i]);
            if (tileId != emptyTileId)
            {
                pieces.Add(tileId);
            }
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            for (int j = i + 1; j < pieces.Count; j++)
            {
                if (pieces[i] > pieces[j])
                {
                    inversions++;
                }
            }
        }
        return inversions;
    }
    private bool IsSolvable(State state)
    {
        int inversions = CountInversions(state);
        int N = size * size;

        if (size % 2 != 0)
        {
            return inversions % 2 == 0;
        }
        else
        {
            int emptyRowFromTop = state.EmptyIndex / size;
            int emptyRowFromBottom = size - emptyRowFromTop;

            return (inversions + emptyRowFromBottom) % 2 == 0;
        }
    }
    private void CorrectParity(State state)
    {
        if (!IsSolvable(state))
        {
            int N = state.Order.Length;
            int emptyTileId = N - 1;
            int index1 = -1;
            int index2 = -1;

            for (int i = 0; i < N; i++)
            {
                if (int.Parse(state.Order[i]) != emptyTileId)
                {
                    index1 = i;
                    break;
                }
            }

            for (int i = index1 + 1; i < N; i++)
            {
                if (int.Parse(state.Order[i]) != emptyTileId)
                {
                    index2 = i;
                    break;
                }
            }
            if (index1 != -1 && index2 != -1)
            {
                string temp = state.Order[index1];
                state.Order[index1] = state.Order[index2];
                state.Order[index2] = temp;

            }
        }
    }

    #endregion

    public int Size { get => size; set => size = value; }
    public GenerationMethod GenerationMethod { get => generationMethod; set => generationMethod = value; }
    public int GoalBackwardIterations { get => goalBackwardIterations; set => goalBackwardIterations = value; }
    public int HillClimbingIterations { get => hillClimbingIterations; set => hillClimbingIterations = value; }
    public int PopulationSize { get => populationSize; set => populationSize = value; }
    public float CrossoverRate { get => crossoverRate; set => crossoverRate = value; }
    public float MutationRate { get => mutationRate; set => mutationRate = value; }
    public int MaxGenerations { get => maxGenerations; set => maxGenerations = value; }
    public float DebugDelay { get => debugDelay; set => debugDelay = value; }
    public bool UseSeed { get => useSeed; set => useSeed = value; }
    public int Seed { get => seed; set => seed = value; }
    public GenerationResult GBResult { get; private set; }
    public GenerationResult HCResult { get; private set; }
    public GenerationResult GAResult { get; private set; }
}