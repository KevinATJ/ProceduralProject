using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GenerationMethod { GoalBackward, HillClimbing }

    [Header("Puzzle Config")]
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private int size = 4;
    [SerializeField] private float shuffleTime = 1f;

    [Header("Generation Method")]
    [SerializeField] private GenerationMethod generationMethod = GenerationMethod.GoalBackward;
    [SerializeField] private int hillClimbingIterations = 100;
    [SerializeField] private int goalBackwardIterations = 100;

    [Header("Debug")]
    [SerializeField] private float debugDelay = 0.2f;

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

    void Start()
    {
        if (size < 2) size = 2;

        pieces = new List<Transform>();
        CreateGamePieces(0.01f);
        StartCoroutine(WaitShuffle(shuffleTime));
    }
    private IEnumerator WaitShuffle(float duration)
    {
        shuffling = true;
        yield return new WaitForSeconds(duration);

        if (generationMethod == GenerationMethod.GoalBackward)
        {
            yield return StartCoroutine(GenerateGoalBackwardStepByStep());
        }
        else if (generationMethod == GenerationMethod.HillClimbing)
        {
            yield return StartCoroutine(HillClimbing_GenerateStepByStep());
        }

        shuffling = false;
    }

    void Update()
    {
        if (!shuffling && CheckCompletion())
        {
            shuffling = true;
            Debug.Log("Puzzle Resuelto");
            StartCoroutine(WaitShuffle(shuffleTime));
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
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

    private void GenerateGoalBackward()
    {
        int count = 0;
        int last = 0;
        while (count < (goalBackwardIterations))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) { continue; }
            last = emptyLocation;

            if (SwapIfValid(rnd, -size, size)) { count++; }
            else if (SwapIfValid(rnd, +size, size)) { count++; }
            else if (SwapIfValid(rnd, -1, 0)) { count++; }
            else if (SwapIfValid(rnd, +1, size - 1)) { count++; }
        }
    }

    private IEnumerator GenerateGoalBackwardStepByStep()
    {
        int count = 0;
        int last = 0;
        while (count < goalBackwardIterations)
        {
            
            int rnd = Random.Range(0, size * size);
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
                
                yield return new WaitForSeconds(debugDelay);
            }
        }
        DebugPuzzleState();
        shuffling = false;
    }

    #endregion

    #region HILL CLIMBING

    private State HillClimbing_Generate()
    {

        State currentState = new State(pieces, emptyLocation);
        int currentFitness = CalculateFitness(currentState);

        State bestState = currentState;
        int bestFitness = currentFitness;

        int[] offsets = { -size, +size, -1, +1 };
        int[] colChecks = { size, size, 0, size - 1 };

        for (int i = 0; i < hillClimbingIterations; i++)
        {

            int offsetIndex = Random.Range(0, offsets.Length);
            State neighbor = TryGenerateNeighbor(currentState, offsets[offsetIndex], colChecks[offsetIndex]);

            if (neighbor != null)
            {
                int neighborFitness = CalculateFitness(neighbor);


                if (neighborFitness > bestFitness)
                {
                    bestFitness = neighborFitness;
                    bestState = neighbor;
                }


                currentState = neighbor;
            }
        }


        ApplyStateToGame(bestState);

        Debug.Log($"Puzzle generado con dificultad (distancia Manhattan total): {bestFitness}");
        return bestState;
    }

    private IEnumerator HillClimbing_GenerateStepByStep()
    {
        State currentState = new State(pieces, emptyLocation);
        int currentFitness = CalculateFitness(currentState);

        State bestState = currentState.Clone();
        int bestFitness = currentFitness;

        List<State> currentPath = new List<State> { currentState.Clone() };
        List<State> bestPath = new List<State> { currentState.Clone() };

        int[] offsets = { -size, +size, -1, +1 };
        int[] colChecks = { size, size, 0, size - 1 };

        for (int i = 0; i < hillClimbingIterations; i++)
        {
            int offsetIndex = Random.Range(0, offsets.Length);
            State neighbor = TryGenerateNeighbor(currentState, offsets[offsetIndex], colChecks[offsetIndex]);

            if (neighbor != null)
            {
                int neighborFitness = CalculateFitness(neighbor);

                

                if (neighborFitness > bestFitness)
                {
                    bestFitness = neighborFitness;
                    bestState = neighbor.Clone();
                    bestPath = new List<State>(currentPath);
                }
                
                currentState = neighbor;
                currentPath.Add(currentState.Clone());
            }
        }

        foreach (var state in bestPath)
        {
            ApplyStateToGame(state);
            yield return new WaitForSeconds(debugDelay);
        }

        Debug.Log($"Puzzle generado con dificultad (distancia Manhattan total): {bestFitness}");
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
}