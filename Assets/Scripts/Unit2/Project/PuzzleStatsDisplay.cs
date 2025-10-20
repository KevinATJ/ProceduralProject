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

    [SerializeField] private TextMeshProUGUI gbDifficultyText;
    [SerializeField] private TextMeshProUGUI hcDifficultyText;
    [SerializeField] private TextMeshProUGUI gaDifficultyText;
    [SerializeField] private TextMeshProUGUI gbTimeText;
    [SerializeField] private TextMeshProUGUI hcTimeText;
    [SerializeField] private TextMeshProUGUI gaTimeText;

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
            if (hc != null) { 
                hcIterationsText.text = $"Iterations: {hc.CurrentHillClimbingIteration}/{hc.HillClimbingIterations}";}
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

        if(gbDifficultyText != null)
        {
            if (gb.GBResult != null)
                gbDifficultyText.text = $"Difficulty: {gb.GBResult.FinalFitness}";
            else
                gbDifficultyText.text = "Difficulty: -";
        }

        if(hcDifficultyText != null)
        {
            if (hc.HCResult != null)
                hcDifficultyText.text = $"Difficulty: {hc.HCResult.FinalFitness}";
            else
                hcDifficultyText.text = "Difficulty: -";
        }

        if(gaDifficultyText != null)
        {
            if (ga.GAResult != null)
                gaDifficultyText.text = $"Difficulty: {ga.GAResult.FinalFitness}";
            else
                gaDifficultyText.text = "Difficulty: -";
        }

        if(gbTimeText != null)
        {
            if (gb.GBResult != null)
                gbTimeText.text = $"Time: {gb.GBResult.TimeSeconds:F4}s";
            else
                gbTimeText.text = "Time: -";
        }

        if(hcTimeText != null)
        {
            if (hc.HCResult != null)
                hcTimeText.text = $"Time: {hc.HCResult.TimeSeconds:F4}s";
            else
                hcTimeText.text = "Time: -";
        }

        if(gaTimeText != null)
        {
            if (ga.GAResult != null)
                gaTimeText.text = $"Time: {ga.GAResult.TimeSeconds:F4}s";
            else
                gaTimeText.text = "Time: -";
        }
    }
}