using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startScreenPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Settings UI")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI timeElapsedText;
    [SerializeField] private TextMeshProUGUI coinCollectedText;

    [Header("Game Over Panel")]
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI finalCoinsText;

    private PlayerController playerController;
    private int coinsCollected = 0;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGamePause += OnGamePause;
        GameManager.OnGameResume += OnGameResume;
        GameManager.OnGameOver += OnGameOver;
        GameManager.OnScoreChanged += OnScoreChanged;
        GameManager.OnTimeChanged += OnTimeChanged;
    }

    private void Start()
    {
        ShowStartScreen();
        SetupVolumeSliders();
        LoadVolumeSettings();
    }

    private void SetupVolumeSliders()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGamePause -= OnGamePause;
        GameManager.OnGameResume -= OnGameResume;
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnScoreChanged -= OnScoreChanged;
        GameManager.OnTimeChanged -= OnTimeChanged;
    }

    private void ShowStartScreen()
    {
        SetPanelActive(startScreenPanel, true);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
    }

    private void ShowHUD()
    {
        SetPanelActive(startScreenPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
    }

    private void ShowPausePanel()
    {
        SetPanelActive(startScreenPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(pausePanel, true);
    }

    private void HidePausePanel()
    {
        SetPanelActive(pausePanel, false);
    }

    private void ShowGameOverPanel()
    {
        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, true);

        if (GameManager.Instance != null)
        {
            UpdateGameOverStats();
        }
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    public void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    public void OnSettingsButtonClicked()
    {
        SetPanelActive(startScreenPanel, false);
        SetPanelActive(settingsPanel, true);
    }

    public void OnExitButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    public void OnSettingsBackButtonClicked()
    {
        SetPanelActive(settingsPanel, false);
        SetPanelActive(startScreenPanel, true);
    }

    public void OnPauseButtonClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameStarted && !GameManager.Instance.IsGamePaused)
        {
            GameManager.Instance.PauseGame();
        }
    }

    public void OnResumeButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    public void OnPauseMenuButtonClicked()
    {
        SceneManager.LoadScene(0);
    }

    public void OnPauseQuitButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    public void OnReturnToMenuButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitGameButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    public void OnJumpButtonClicked()
    {
        if (playerController != null)
        {
            playerController.Jump();
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnGameStart()
    {
        ShowHUD();
        coinsCollected = 0;
        UpdateCoinText();
    }

    private void OnGamePause()
    {
        ShowPausePanel();
    }

    private void OnGameResume()
    {
        HidePausePanel();
    }

    private void OnGameOver()
    {
        ShowGameOverPanel();
    }

    private void OnScoreChanged(int score)
    {
        coinsCollected = score / 10;
        UpdateCoinText();
    }

    private void OnTimeChanged(float time)
    {
        UpdateTimeText(time);
    }

    private void UpdateTimeText(float time)
    {
        if (timeElapsedText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeElapsedText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }

    private void UpdateCoinText()
    {
        if (coinCollectedText != null)
        {
            coinCollectedText.text = coinsCollected.ToString();
        }
    }

    private void UpdateGameOverStats()
    {
        if (GameManager.Instance == null)
            return;

        if (timeSurvivedText != null)
        {
            float time = GameManager.Instance.TimeElapsed;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeSurvivedText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        if (finalCoinsText != null)
        {
            int finalCoins = GameManager.Instance.Score / 10;
            finalCoinsText.text = finalCoins.ToString();
        }
    }

    private void LoadVolumeSettings()
    {
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            }
        }
    }
}
