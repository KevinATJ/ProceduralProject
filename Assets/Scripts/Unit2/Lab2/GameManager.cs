using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [Header("Configuración Global")]
    [SerializeField] private PuzzleController puzzleControllerPrefab;
    [SerializeField] private int size = 4;

    [Header("Posiciones de los Puzzles")]
    [SerializeField] private Transform goalBackwardSpawn;
    [SerializeField] private Transform hillClimbingSpawn;
    [SerializeField] private Transform geneticAlgorithmSpawn;

    [Header("Seeds por defecto (opcional)")]
    [SerializeField] private bool gbUseSeedDefault = false;
    [SerializeField] private int gbSeedDefault = 0;
    [SerializeField] private bool hcUseSeedDefault = false;
    [SerializeField] private int hcSeedDefault = 0;
    [SerializeField] private bool gaUseSeedDefault = false;
    [SerializeField] private int gaSeedDefault = 0;

    private PuzzleController gbPuzzle;
    private PuzzleController hcPuzzle;
    private PuzzleController gaPuzzle;

    void Start()
    {
        gbPuzzle = Instantiate(puzzleControllerPrefab, goalBackwardSpawn.position, Quaternion.identity, goalBackwardSpawn);
        gbPuzzle.UseSeed = gbUseSeedDefault;
        gbPuzzle.Seed = gbSeedDefault;
        gbPuzzle.Initialize(size, GenerationMethod.GoalBackward, gbPuzzle.transform);

        hcPuzzle = Instantiate(puzzleControllerPrefab, hillClimbingSpawn.position, Quaternion.identity, hillClimbingSpawn);
        hcPuzzle.UseSeed = hcUseSeedDefault;
        hcPuzzle.Seed = hcSeedDefault;
        hcPuzzle.Initialize(size, GenerationMethod.HillClimbing, hcPuzzle.transform);

        gaPuzzle = Instantiate(puzzleControllerPrefab, geneticAlgorithmSpawn.position, Quaternion.identity, geneticAlgorithmSpawn);
        gaPuzzle.UseSeed = gaUseSeedDefault;
        gaPuzzle.Seed = gaSeedDefault;
        gaPuzzle.Initialize(size, GenerationMethod.GeneticAlgorithm, gaPuzzle.transform);
    }

    public void RecreatePuzzles(int newSize,
                               int goalBackwardIterations,
                               int hillClimbingIterations,
                               int populationSize,
                               float crossoverRate,
                               float mutationRate,
                               int maxGenerations,
                               float debugDelay,
                               bool gbUseSeed,
                               int gbSeed,
                               bool hcUseSeed,
                               int hcSeed,
                               bool gaUseSeed,
                               int gaSeed)
    {

        if (gbPuzzle != null) Destroy(gbPuzzle.gameObject);
        if (hcPuzzle != null) Destroy(hcPuzzle.gameObject);
        if (gaPuzzle != null) Destroy(gaPuzzle.gameObject);

        gbPuzzle = Instantiate(puzzleControllerPrefab, goalBackwardSpawn.position, Quaternion.identity, goalBackwardSpawn);
        gbPuzzle.GoalBackwardIterations = Mathf.Max(0, goalBackwardIterations);
        gbPuzzle.DebugDelay = Mathf.Max(0f, debugDelay);
        gbPuzzle.UseSeed = gbUseSeed;
        gbPuzzle.Seed = gbSeed;
        gbPuzzle.Initialize(newSize, GenerationMethod.GoalBackward, gbPuzzle.transform);

        hcPuzzle = Instantiate(puzzleControllerPrefab, hillClimbingSpawn.position, Quaternion.identity, hillClimbingSpawn);
        hcPuzzle.HillClimbingIterations = Mathf.Max(0, hillClimbingIterations);
        hcPuzzle.DebugDelay = Mathf.Max(0f, debugDelay);
        hcPuzzle.UseSeed = hcUseSeed;
        hcPuzzle.Seed = hcSeed;
        hcPuzzle.Initialize(newSize, GenerationMethod.HillClimbing, hcPuzzle.transform);

        gaPuzzle = Instantiate(puzzleControllerPrefab, geneticAlgorithmSpawn.position, Quaternion.identity, geneticAlgorithmSpawn);
        gaPuzzle.PopulationSize = Mathf.Max(1, populationSize);
        gaPuzzle.CrossoverRate = Mathf.Clamp01(crossoverRate);
        gaPuzzle.MutationRate = Mathf.Clamp01(mutationRate);
        gaPuzzle.MaxGenerations = Mathf.Max(0, maxGenerations);
        gaPuzzle.DebugDelay = Mathf.Max(0f, debugDelay);
        gaPuzzle.UseSeed = gaUseSeed;
        gaPuzzle.Seed = gaSeed;
        gaPuzzle.Initialize(newSize, GenerationMethod.GeneticAlgorithm, gaPuzzle.transform);
    }

    public PuzzleController GBPuzzle => gbPuzzle;
    public PuzzleController HCPuzzle => hcPuzzle;
    public PuzzleController GAPuzzle => gaPuzzle;
}