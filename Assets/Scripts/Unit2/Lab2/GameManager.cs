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

    void Start()
    {
        PuzzleController gb = Instantiate(puzzleControllerPrefab, goalBackwardSpawn.position, Quaternion.identity, goalBackwardSpawn);
        gb.Initialize(size, GenerationMethod.GoalBackward, gb.transform);

        PuzzleController hc = Instantiate(puzzleControllerPrefab, hillClimbingSpawn.position, Quaternion.identity, hillClimbingSpawn);
        hc.Initialize(size, GenerationMethod.HillClimbing, hc.transform);

        PuzzleController ga = Instantiate(puzzleControllerPrefab, geneticAlgorithmSpawn.position, Quaternion.identity, geneticAlgorithmSpawn);
        ga.Initialize(size, GenerationMethod.GeneticAlgorithm, ga.transform);
    }
}