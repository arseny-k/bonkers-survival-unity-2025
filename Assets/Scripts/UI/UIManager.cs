using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Slider playerHealthSlider;
    public TextMeshProUGUI enemyCountText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    private void Awake()
    {
        Instance = this;
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
    }

    public void UpdatePlayerHealth(int current, int max)
    {
        playerHealthSlider.maxValue = max;
        playerHealthSlider.value = current;
    }

    public void UpdateEnemyCount(int killed, int total)
    {
        enemyCountText.text = $"Enemies: {killed} / {total}";
    }

    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);
    }
    
    // Wire these to Buttons in Inspector
    public void OnRetryClick() => GameManager.Instance.RetryLevel();
    public void OnNextLevelClick() => GameManager.Instance.LoadNextLevel();
    public void OnMenuClick() => GameManager.Instance.ToMainMenu();
}