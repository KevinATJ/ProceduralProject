using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public DS_Terrain dsTerrain;
    public TreeSpawner treeSpawner;

    public InputField mapSizeExponentInput;
    public InputField heightScaleInput;
    public InputField mapXScaleInput;
    public InputField mapYScaleInput;
    public InputField caIterationsInput;
    public InputField caNeighborhoodInput;
    public InputField roughnessInput;
    public InputField treeCountInput;
    public Dropdown terrainTypeDropdown;

    public static bool IsMenuOpen = false;

    void Start()
    {
        menuPanel.SetActive(false);

        mapSizeExponentInput.text = dsTerrain.mapSizeExponent.ToString();
        heightScaleInput.text = dsTerrain.heightScale.ToString();
        mapXScaleInput.text = dsTerrain.mapX_Scale.ToString();
        mapYScaleInput.text = dsTerrain.mapY_Scale.ToString();
        caIterationsInput.text = dsTerrain.CA_iterations.ToString();
        caNeighborhoodInput.text = dsTerrain.CA_neighborhood.ToString();
        roughnessInput.text = dsTerrain.roughness.ToString();

        treeCountInput.text = treeSpawner.treeCount.ToString();

        terrainTypeDropdown.value = (int)dsTerrain.terrainType;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
            IsMenuOpen = menuPanel.activeSelf;
            Cursor.visible = IsMenuOpen;
            Cursor.lockState = IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void OnApplyButtonPressed()
    {
        if (int.TryParse(mapSizeExponentInput.text, out int mapSizeExponent))
            dsTerrain.mapSizeExponent = mapSizeExponent;
        if (float.TryParse(heightScaleInput.text, out float heightScale))
            dsTerrain.heightScale = heightScale;
        if (float.TryParse(mapXScaleInput.text, out float mapXScale))
            dsTerrain.mapX_Scale = mapXScale;
        if (float.TryParse(mapYScaleInput.text, out float mapYScale))
            dsTerrain.mapY_Scale = mapYScale;
        if (int.TryParse(caIterationsInput.text, out int caIterations))
            dsTerrain.CA_iterations = caIterations;
        if (int.TryParse(caNeighborhoodInput.text, out int caNeighborhood))
            dsTerrain.CA_neighborhood = caNeighborhood;
        if (float.TryParse(roughnessInput.text, out float roughness))
            dsTerrain.roughness = roughness;

   
        if (int.TryParse(treeCountInput.text, out int treeCount))
            treeSpawner.treeCount = treeCount;


        dsTerrain.terrainType = (DS_Terrain.TerrainType)terrainTypeDropdown.value;

        dsTerrain.RegenerateTerrain(dsTerrain.terrainType);
        treeSpawner.RegenerateTrees();
    }
}