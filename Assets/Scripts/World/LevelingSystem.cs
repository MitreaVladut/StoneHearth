using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelingSystem : MonoBehaviour
{
    public GameObject levelingCanvas;
    public TextMeshProUGUI soulsText;
    public Button hpButton, earthButton, magicButton, leaveButton;

    private bool isActive = false;

    void Start()
    {
        levelingCanvas.SetActive(false);
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (hpButton != null) hpButton.onClick.RemoveAllListeners();
        if (earthButton != null) earthButton.onClick.RemoveAllListeners();
        if (magicButton != null) magicButton.onClick.RemoveAllListeners();
        if (leaveButton != null) leaveButton.onClick.RemoveAllListeners();

        hpButton.onClick.AddListener(() => OnLevelUpButtonClick("HP"));
        earthButton.onClick.AddListener(() => OnLevelUpButtonClick("Earth"));
        magicButton.onClick.AddListener(() => OnLevelUpButtonClick("Magic"));
        leaveButton.onClick.AddListener(DeactivateLeveling);

    }
    public void OnLevelUpButtonClick(string category)
    {
        PlayerController.instance.LevelUp(category);
        UpdateLevelingUI(); // Refresh UI after leveling up
        UpdateCellColors(); // Ensure cells reflect the new level
    }

    public void ActivateLeveling(Totem totem)
    {
        if (!isActive)
        {
            Debug.Log("Activating leveling canvas: " + levelingCanvas.name);
            levelingCanvas.SetActive(true);
            isActive = true;
            UpdateLevelingUI();
        }
    }

    public void DeactivateLeveling()
    {
        if (isActive)
        {
            Debug.Log("Deactivating leveling canvas: " + levelingCanvas.name);
            levelingCanvas.SetActive(false);
            isActive = false;
        }
    }

    public void UpdateLevelingUI()
    {
        if (soulsText != null)
        {
            int nextCost = PlayerController.instance.globalBaseCost;
            soulsText.text = $"Souls: {PlayerController.instance.souls} | Next Level: {nextCost}";
            Debug.Log("UI updated: Souls = " + PlayerController.instance.souls + ", Next Cost = " + nextCost);
        }
        UpdateCellColors(); // Ensure cells update after UI text change
    }

    private void UpdateCellColors()
    {
        LevelCell[] cells = levelingCanvas.GetComponentsInChildren<LevelCell>();
        Debug.Log("Found " + cells.Length + " LevelCell components");
        foreach (LevelCell cell in cells)
        {
            cell.UpdateColor();
        }
    }
}
