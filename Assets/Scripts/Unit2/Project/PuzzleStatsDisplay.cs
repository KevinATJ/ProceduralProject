using UnityEngine;
using TMPro;

public class PuzzleStatsDisplay : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;

    [Header("TextMeshPro - Stats")]
    [SerializeField] private TextMeshProUGUI gbIterationsText;
    [SerializeField] private TextMeshProUGUI hcIterationsText;
    [SerializeField] private TextMeshProUGUI gaGenerationText;
    [SerializeField] private TextMeshProUGUI gaFitnessText;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void Update()
    {
        var gb = gameManager?.GBPuzzle;
        var hc = gameManager?.HCPuzzle;
        var ga = gameManager?.GAPuzzle;

        if (gbIterationsText != null)
        {
            if (gb != null)
                gbIterationsText.text = $"Iterations: {gb.CurrentGoalBackwardIteration}/{gb.GoalBackwardIterations}";
            else
                gbIterationsText.text = "Iterations: -";
        }

        if (hcIterationsText != null)
        {
            if (hc != null)
                hcIterationsText.text = $"Iterations: {hc.CurrentHillClimbingIteration}/{hc.HillClimbingIterations}  (best {hc.CurrentHillClimbingBestFitness})";
            else
                hcIterationsText.text = "Iterations: -";
        }

        if (gaGenerationText != null)
        {
            if (ga != null)
                gaGenerationText.text = $"Gen: {ga.CurrentGeneration}/{ga.MaxGenerations}";
            else
                gaGenerationText.text = "Gen: -";
        }

        if (gaFitnessText != null)
        {
            if (ga != null)
                gaFitnessText.text = $"Fitness: {ga.CurrentGABestFitness}";
            else
                gaFitnessText.text = "Fitness: -";
        }
    }
}