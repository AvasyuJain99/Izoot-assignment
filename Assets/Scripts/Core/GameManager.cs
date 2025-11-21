using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float initialSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float speedIncreaseRate = 0.1f;
    [SerializeField] private float speedIncreaseInterval = 1f;

    [Header("Game State")]
    [SerializeField] private bool isGameStarted = false;
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isGameOver = false;

    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameOver;
    public static event Action<float> OnSpeedChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<float> OnTimeChanged;

    public float CurrentSpeed { get; private set; }
    public int Score { get; private set; }
    public float TimeElapsed { get; private set; }
    public bool IsGameStarted => isGameStarted;
    public bool IsGamePaused => isGamePaused;
    public bool IsGameOver => isGameOver;

    private float speedIncreaseTimer = 0f;
    private PlayerController cachedPlayer;
    private PowerUpSystem cachedPowerUpSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CurrentSpeed = initialSpeed;
        Score = 0;
        TimeElapsed = 0f;
    }

    private void Update()
    {
        if (!isGameStarted || isGamePaused || isGameOver)
            return;

        TimeElapsed += Time.deltaTime;
        OnTimeChanged?.Invoke(TimeElapsed);

        speedIncreaseTimer += Time.deltaTime;
        if (speedIncreaseTimer >= speedIncreaseInterval)
        {
            speedIncreaseTimer = 0f;
            IncreaseSpeed();
        }
    }

    private void IncreaseSpeed()
    {
        if (CurrentSpeed < maxSpeed)
        {
            CurrentSpeed = Mathf.Min(CurrentSpeed * (1f + speedIncreaseRate), maxSpeed);
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }
    }

    public void StartGame()
    {
        if (isGameStarted)
            return;

        isGameStarted = true;
        isGamePaused = false;
        isGameOver = false;
        CurrentSpeed = initialSpeed;
        Score = 0;
        TimeElapsed = 0f;
        speedIncreaseTimer = 0f;

        OnGameStart?.Invoke();
    }

    public void PauseGame()
    {
        if (!isGameStarted || isGameOver)
            return;

        isGamePaused = true;
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        if (!isGameStarted || isGameOver || !isGamePaused)
            return;

        isGamePaused = false;
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        isGamePaused = false;
        Time.timeScale = 1f;
        OnGameOver?.Invoke();
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetGame()
    {
        isGameStarted = false;
        isGamePaused = false;
        isGameOver = false;
        CurrentSpeed = initialSpeed;
        Score = 0;
        TimeElapsed = 0f;
        speedIncreaseTimer = 0f;
        Time.timeScale = 1f;
        
        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<PlayerController>();
        if (cachedPlayer != null)
            cachedPlayer.ResetPlayer();
        
        if (cachedPowerUpSystem == null)
            cachedPowerUpSystem = FindObjectOfType<PowerUpSystem>();
        if (cachedPowerUpSystem != null)
            cachedPowerUpSystem.ResetPowerUps();
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
