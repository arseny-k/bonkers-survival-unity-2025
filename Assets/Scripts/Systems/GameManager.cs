using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public string nextLevelName = "Level2";
    public string mainMenuScene = "MainMenu";

    [Header("Runtime Tracker")]
    public int totalEnemies;
    public int killedEnemies;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player && player.TryGetComponent<Health>(out var playerHealth))
        {
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
            UIManager.Instance.UpdatePlayerHealth(playerHealth.maxHealth, playerHealth.maxHealth);
            playerHealth.OnDamageTaken.AddListener((current) => 
                UIManager.Instance.UpdatePlayerHealth(current, playerHealth.maxHealth));
        }

        var enemies =  FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        totalEnemies = enemies.Length;
        
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<Health>(out var h))
            {
                h.OnDeath.AddListener(OnEnemyKilled);
            }
        }

        UIManager.Instance.UpdateEnemyCount(killedEnemies, totalEnemies);
    }

    private void OnEnemyKilled()
    {
        if (IsGameOver) return;

        killedEnemies++;
        UIManager.Instance.UpdateEnemyCount(killedEnemies, totalEnemies);

        if (killedEnemies >= totalEnemies)
        {
            LevelComplete();
        }
    }

    private void OnPlayerDeath()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        UIManager.Instance.ShowGameOverScreen();
    }

    private void LevelComplete()
    {
        IsGameOver = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UIManager.Instance.ShowVictoryScreen();
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        if (Application.CanStreamedLevelBeLoaded(nextLevelName))
            SceneManager.LoadScene(nextLevelName);
        else
            Debug.LogWarning($"Scene {nextLevelName} not found in Build Settings.");
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}