using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PauseMenu : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private WFCGenerator wfcGenerator;

    [Header("UI Elements - Configuración General")]
    [SerializeField] private TMP_Dropdown modeDropdown;
    [SerializeField] private TMP_InputField simpleTiledGridSizeInput;
    [SerializeField] private TMP_InputField finalOverlappingGridSizeInput;
    [SerializeField] private TMP_InputField stepDelayInput;
    [SerializeField] private TMP_InputField tileSizeInput;

    [Header("UI Elements - Complex WFC")]
    [SerializeField] private Toggle complexUseSeedToggle;
    [SerializeField] private TMP_InputField complexSeedInput;
    [SerializeField] private TMP_InputField contextMatrixDisplayTimeInput;

    [Header("UI Elements - Markov N-gram")]
    [SerializeField] private Toggle markovUseSeedToggle;
    [SerializeField] private TMP_InputField markovSeedInput;
    [SerializeField] private Slider markovNSlider;
    [SerializeField] private TMP_Text markovNValueText;
    [SerializeField] private TMP_InputField markovGridWidthInput;
    [SerializeField] private Toggle markovStepByStepToggle;
    [SerializeField] private TMP_InputField markovStepDelayInput;

    /*[Header("UI Elements - Preview y Guardado")]
    [SerializeField] private Toggle previewGeneratedMapsToggle;
    [SerializeField] private TMP_InputField totalPreviewTimeInput;
    [SerializeField] private Toggle saveGeneratedToggle;
    [SerializeField] private TMP_InputField generatedToSaveCountInput;
    [SerializeField] private Toggle useManualTXTMapsToggle;*/

    private void Awake()
    {
        if (wfcGenerator == null)
        {
            wfcGenerator = FindObjectOfType<WFCGenerator>();
            if (wfcGenerator == null)
                Debug.LogWarning("WFCPauseMenuController: no se encontró WFCGenerator en la escena.");
        }
    }

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        SetupUI();
        SetupListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();
    }

    private void SetupListeners()
    {
        if (markovNSlider != null)
        {
            markovNSlider.onValueChanged.AddListener((value) => {
                if (markovNValueText != null)
                    markovNValueText.text = Mathf.RoundToInt(value).ToString();
            });
        }
    }

    private void SetupUI()
    {
        if (wfcGenerator == null)
        {
            SetDefaultValues();
            return;
        }

        // Modo de generación
        if (modeDropdown != null)
            modeDropdown.value = (int)wfcGenerator.Mode;

        // Configuración general
        if (simpleTiledGridSizeInput != null)
            simpleTiledGridSizeInput.text = wfcGenerator.SimpleTiledContextGridSize.ToString();

        if (finalOverlappingGridSizeInput != null)
            finalOverlappingGridSizeInput.text = wfcGenerator.FinalOverlappingGridSize.ToString();

        if (stepDelayInput != null)
            stepDelayInput.text = wfcGenerator.StepDelay.ToString();

        if (tileSizeInput != null)
            tileSizeInput.text = wfcGenerator.TileSize.ToString();

        // Complex WFC
        if (complexUseSeedToggle != null)
            complexUseSeedToggle.isOn = wfcGenerator.ComplexWFCSeed != 0;

        if (complexSeedInput != null)
            complexSeedInput.text = wfcGenerator.ComplexWFCSeed.ToString();

        if (contextMatrixDisplayTimeInput != null)
            contextMatrixDisplayTimeInput.text = wfcGenerator.ContextMatrixDisplayTime.ToString();

        // Markov N-gram
        if (markovUseSeedToggle != null)
            markovUseSeedToggle.isOn = wfcGenerator.MarkovNGramSeed != 0;

        if (markovSeedInput != null)
            markovSeedInput.text = wfcGenerator.MarkovNGramSeed.ToString();

        if (markovNSlider != null)
        {
            markovNSlider.value = wfcGenerator.MarkovColumnN;
            if (markovNValueText != null)
                markovNValueText.text = wfcGenerator.MarkovColumnN.ToString();
        }

        if (markovGridWidthInput != null)
            markovGridWidthInput.text = wfcGenerator.MarkovColumnGridWidth.ToString();

        if (markovStepByStepToggle != null)
            markovStepByStepToggle.isOn = wfcGenerator.MarkovColumnStepByStep;

        if (markovStepDelayInput != null)
            markovStepDelayInput.text = wfcGenerator.MarkovColumnStepDelay.ToString();

        // Preview y guardado
        /*if (previewGeneratedMapsToggle != null)
            previewGeneratedMapsToggle.isOn = wfcGenerator.PreviewGeneratedMaps;

        if (totalPreviewTimeInput != null)
            totalPreviewTimeInput.text = wfcGenerator.TotalPreviewTime.ToString();

        if (saveGeneratedToggle != null)
            saveGeneratedToggle.isOn = wfcGenerator.SaveGeneratedToTxt;

        if (generatedToSaveCountInput != null)
            generatedToSaveCountInput.text = wfcGenerator.GeneratedToSaveCount.ToString();

        if (useManualTXTMapsToggle != null)
            useManualTXTMapsToggle.isOn = wfcGenerator.UseManualTXTMaps;*/
    }

    private void SetDefaultValues()
    {
        if (modeDropdown != null) modeDropdown.value = 0;
        if (simpleTiledGridSizeInput != null) simpleTiledGridSizeInput.text = "10";
        if (finalOverlappingGridSizeInput != null) finalOverlappingGridSizeInput.text = "10";
        if (stepDelayInput != null) stepDelayInput.text = "0.05";
        if (tileSizeInput != null) tileSizeInput.text = "1";
        if (complexUseSeedToggle != null) complexUseSeedToggle.isOn = false;
        if (complexSeedInput != null) complexSeedInput.text = "0";
        if (contextMatrixDisplayTimeInput != null) contextMatrixDisplayTimeInput.text = "2";
        if (markovUseSeedToggle != null) markovUseSeedToggle.isOn = false;
        if (markovSeedInput != null) markovSeedInput.text = "0";
        if (markovNSlider != null) markovNSlider.value = 2;
        if (markovNValueText != null) markovNValueText.text = "2";
        if (markovGridWidthInput != null) markovGridWidthInput.text = "10";
        if (markovStepByStepToggle != null) markovStepByStepToggle.isOn = false;
        if (markovStepDelayInput != null) markovStepDelayInput.text = "0.02";
        /*if (previewGeneratedMapsToggle != null) previewGeneratedMapsToggle.isOn = true;
        if (totalPreviewTimeInput != null) totalPreviewTimeInput.text = "10";
        if (saveGeneratedToggle != null) saveGeneratedToggle.isOn = false;
        if (generatedToSaveCountInput != null) generatedToSaveCountInput.text = "10";
        if (useManualTXTMapsToggle != null) useManualTXTMapsToggle.isOn = false;*/
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
        if (wfcGenerator == null) return;

        // Modo de generación
        if (modeDropdown != null)
            wfcGenerator.Mode = (WFCGenerator.GenerationMode)modeDropdown.value;

        // Configuración general
        if (int.TryParse(simpleTiledGridSizeInput.text, out int simpleSize))
            wfcGenerator.SimpleTiledContextGridSize = Mathf.Max(1, simpleSize);

        if (int.TryParse(finalOverlappingGridSizeInput.text, out int finalSize))
            wfcGenerator.FinalOverlappingGridSize = Mathf.Max(1, finalSize);

        if (float.TryParse(stepDelayInput.text, out float stepDelay))
            wfcGenerator.StepDelay = Mathf.Max(0f, stepDelay);

        if (float.TryParse(tileSizeInput.text, out float tileSize))
            wfcGenerator.TileSize = Mathf.Max(0.1f, tileSize);

        // Complex WFC
        if (complexUseSeedToggle != null && complexSeedInput != null)
        {
            if (complexUseSeedToggle.isOn && int.TryParse(complexSeedInput.text, out int complexSeed))
                wfcGenerator.ComplexWFCSeed = complexSeed;
            else
                wfcGenerator.ComplexWFCSeed = 0;
        }

        if (float.TryParse(contextMatrixDisplayTimeInput.text, out float displayTime))
            wfcGenerator.ContextMatrixDisplayTime = Mathf.Max(0f, displayTime);

        // Markov N-gram
        if (markovUseSeedToggle != null && markovSeedInput != null)
        {
            if (markovUseSeedToggle.isOn && int.TryParse(markovSeedInput.text, out int markovSeed))
                wfcGenerator.MarkovNGramSeed = markovSeed;
            else
                wfcGenerator.MarkovNGramSeed = 0;
        }

        if (markovNSlider != null)
            wfcGenerator.MarkovColumnN = Mathf.RoundToInt(markovNSlider.value);

        if (int.TryParse(markovGridWidthInput.text, out int markovWidth))
            wfcGenerator.MarkovColumnGridWidth = Mathf.Max(1, markovWidth);

        if (markovStepByStepToggle != null)
            wfcGenerator.MarkovColumnStepByStep = markovStepByStepToggle.isOn;

        if (float.TryParse(markovStepDelayInput.text, out float markovDelay))
            wfcGenerator.MarkovColumnStepDelay = Mathf.Max(0f, markovDelay);

        // Preview y guardado
        /*if (previewGeneratedMapsToggle != null)
            wfcGenerator.PreviewGeneratedMaps = previewGeneratedMapsToggle.isOn;

        if (float.TryParse(totalPreviewTimeInput.text, out float previewTime))
            wfcGenerator.TotalPreviewTime = Mathf.Max(0f, previewTime);

        if (saveGeneratedToggle != null)
            wfcGenerator.SaveGeneratedToTxt = saveGeneratedToggle.isOn;

        if (int.TryParse(generatedToSaveCountInput.text, out int saveCount))
            wfcGenerator.GeneratedToSaveCount = Mathf.Max(1, saveCount);

        if (useManualTXTMapsToggle != null)
            wfcGenerator.UseManualTXTMaps = useManualTXTMapsToggle.isOn;*/

        Debug.Log("Configuración WFC aplicada. Reinicia la escena para ver los cambios.");

        SetupUI();
    }

    public void RestartGeneration()
    {
        if (wfcGenerator == null) return;

        ApplyChanges();

        // Reiniciar la generación
        wfcGenerator.StopAllCoroutines();
        wfcGenerator.enabled = false;
        wfcGenerator.enabled = true;

        TogglePauseMenu();
    }

    public void ClosePauseMenu()
    {
        TogglePauseMenu();
    }
}
