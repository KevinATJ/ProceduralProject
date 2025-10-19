using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameManager gameManager;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField sizeInput;
    [SerializeField] private TMP_InputField goalBackwardIterationsInput;
    [SerializeField] private TMP_InputField hillClimbingIterationsInput;
    [SerializeField] private TMP_InputField populationSizeInput;
    [SerializeField] private TMP_InputField crossoverRateInput;
    [SerializeField] private TMP_InputField mutationRateInput;
    [SerializeField] private TMP_InputField maxGenerationsInput;
    [Header("Seed UI - por puzzle")]
    [SerializeField] private Toggle gbUseSeedToggle;
    [SerializeField] private TMP_InputField gbSeedInput;
    [SerializeField] private Toggle hcUseSeedToggle;
    [SerializeField] private TMP_InputField hcSeedInput;
    [SerializeField] private Toggle gaUseSeedToggle;
    [SerializeField] private TMP_InputField gaSeedInput;
    [SerializeField] private TMP_InputField debugDelayInput;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
                Debug.LogWarning("PauseMenuController: no se encontró GameManager en la escena. Asignar manualmente en el Inspector.");
        }
    }

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        SetupUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();
    }

    private void SetupUI()
    {
        if (gameManager == null)
        {
            sizeInput.text = "4";
            goalBackwardIterationsInput.text = "";
            hillClimbingIterationsInput.text = "";
            populationSizeInput.text = "";
            crossoverRateInput.text = "";
            mutationRateInput.text = "";
            maxGenerationsInput.text = "";
            debugDelayInput.text = "";
            if (gbUseSeedToggle != null) gbUseSeedToggle.isOn = false;
            if (hcUseSeedToggle != null) hcUseSeedToggle.isOn = false;
            if (gaUseSeedToggle != null) gaUseSeedToggle.isOn = false;
            if (gbSeedInput != null) gbSeedInput.text = "";
            if (hcSeedInput != null) hcSeedInput.text = "";
            if (gaSeedInput != null) gaSeedInput.text = "";
            return;
        }

        var gb = gameManager.GBPuzzle;
        var hc = gameManager.HCPuzzle;
        var ga = gameManager.GAPuzzle;

        if (gb != null) sizeInput.text = gb.Size.ToString();
        else if (hc != null) sizeInput.text = hc.Size.ToString();
        else if (ga != null) sizeInput.text = ga.Size.ToString();
        else sizeInput.text = "4";

        goalBackwardIterationsInput.text = gb != null ? gb.GoalBackwardIterations.ToString() : "";
        hillClimbingIterationsInput.text = hc != null ? hc.HillClimbingIterations.ToString() : "";

        if (ga != null)
        {
            populationSizeInput.text = ga.PopulationSize.ToString();
            crossoverRateInput.text = ga.CrossoverRate.ToString();
            mutationRateInput.text = ga.MutationRate.ToString();
            maxGenerationsInput.text = ga.MaxGenerations.ToString();
        }
        else
        {
            populationSizeInput.text = "";
            crossoverRateInput.text = "";
            mutationRateInput.text = "";
            maxGenerationsInput.text = "";
        }

        float currentDebug = gb?.DebugDelay ?? hc?.DebugDelay ?? ga?.DebugDelay ?? 0.2f;
        debugDelayInput.text = currentDebug.ToString();

        if (gbUseSeedToggle != null) gbUseSeedToggle.isOn = gb != null ? gb.UseSeed : false;
        if (gbSeedInput != null) gbSeedInput.text = gb != null ? gb.Seed.ToString() : "";

        if (hcUseSeedToggle != null) hcUseSeedToggle.isOn = hc != null ? hc.UseSeed : false;
        if (hcSeedInput != null) hcSeedInput.text = hc != null ? hc.Seed.ToString() : "";

        if (gaUseSeedToggle != null) gaUseSeedToggle.isOn = ga != null ? ga.UseSeed : false;
        if (gaSeedInput != null) gaSeedInput.text = ga != null ? ga.Seed.ToString() : "";
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuPanel == null) return;

        bool opening = !pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(opening);
        Time.timeScale = opening ? 0 : 1;
        Cursor.visible = opening;
        Cursor.lockState = opening ? CursorLockMode.None : CursorLockMode.Locked;

        if (opening) SetupUI();
    }

    public void ApplyChanges()
    {
        if (gameManager == null) return;

        var gb = gameManager.GBPuzzle;
        var hc = gameManager.HCPuzzle;
        var ga = gameManager.GAPuzzle;

        int currentSize = gb?.Size ?? hc?.Size ?? ga?.Size ?? 4;
        int currentGB = gb?.GoalBackwardIterations ?? 100;
        int currentHC = hc?.HillClimbingIterations ?? 100;
        int currentPop = ga?.PopulationSize ?? 50;
        float currentCross = ga?.CrossoverRate ?? 0.7f;
        float currentMut = ga?.MutationRate ?? 0.05f;
        int currentMaxGen = ga?.MaxGenerations ?? 20;
        float currentDebug = gb?.DebugDelay ?? hc?.DebugDelay ?? ga?.DebugDelay ?? 0.2f;

        bool gbUseSeed = gb?.UseSeed ?? false;
        int gbSeed = gb?.Seed ?? 0;
        bool hcUseSeed = hc?.UseSeed ?? false;
        int hcSeed = hc?.Seed ?? 0;
        bool gaUseSeed = ga?.UseSeed ?? false;
        int gaSeed = ga?.Seed ?? 0;

        if (int.TryParse(sizeInput.text, out int parsedSize)) currentSize = Mathf.Max(2, parsedSize);

        if (int.TryParse(goalBackwardIterationsInput.text, out int parsedGB)) currentGB = Mathf.Max(0, parsedGB);
        if (int.TryParse(hillClimbingIterationsInput.text, out int parsedHC)) currentHC = Mathf.Max(0, parsedHC);

        if (int.TryParse(populationSizeInput.text, out int parsedPop)) currentPop = Mathf.Max(1, parsedPop);
        if (float.TryParse(crossoverRateInput.text, out float parsedCross)) currentCross = Mathf.Clamp01(parsedCross);
        if (float.TryParse(mutationRateInput.text, out float parsedMut)) currentMut = Mathf.Clamp01(parsedMut);
        if (int.TryParse(maxGenerationsInput.text, out int parsedMaxGen)) currentMaxGen = Mathf.Max(0, parsedMaxGen);
        if (float.TryParse(debugDelayInput.text, out float parsedDebug)) currentDebug = Mathf.Max(0f, parsedDebug);

        if (gbUseSeedToggle != null) gbUseSeed = gbUseSeedToggle.isOn;
        if (gbSeedInput != null && int.TryParse(gbSeedInput.text, out int parsedGbSeed)) gbSeed = parsedGbSeed;

        if (hcUseSeedToggle != null) hcUseSeed = hcUseSeedToggle.isOn;
        if (hcSeedInput != null && int.TryParse(hcSeedInput.text, out int parsedHcSeed)) hcSeed = parsedHcSeed;

        if (gaUseSeedToggle != null) gaUseSeed = gaUseSeedToggle.isOn;
        if (gaSeedInput != null && int.TryParse(gaSeedInput.text, out int parsedGaSeed)) gaSeed = parsedGaSeed;

        gameManager.RecreatePuzzles(currentSize, currentGB, currentHC, currentPop, currentCross, currentMut, currentMaxGen, currentDebug,
                                   gbUseSeed, gbSeed, hcUseSeed, hcSeed, gaUseSeed, gaSeed);

        SetupUI();
    }
}