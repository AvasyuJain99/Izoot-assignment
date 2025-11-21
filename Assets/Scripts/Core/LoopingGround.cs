using UnityEngine;

public class LoopingGround : MonoBehaviour
{
    [Header("Ground Objects")]
    [SerializeField] private Transform ground1;
    [SerializeField] private Transform ground2;

    [Header("Ground Settings")]
    [SerializeField] private float groundWidth = 64f;
    [SerializeField] private float leftBoundary = -64f;
    [SerializeField] private float rightBoundary = 64f;

    private float currentGameSpeed = 0f;
    private GameManager gameManager;
    private Vector3 ground1Pos;
    private Vector3 ground2Pos;

    private void OnEnable()
    {
        GameManager.OnSpeedChanged += UpdateSpeed;
        GameManager.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        GameManager.OnSpeedChanged -= UpdateSpeed;
        GameManager.OnGameStart -= OnGameStart;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (ground1 != null)
        {
            ground1Pos = ground1.position;
            ground1Pos.x = 0f;
            ground1.position = ground1Pos;
        }

        if (ground2 != null)
        {
            ground2Pos = ground2.position;
            ground2Pos.x = groundWidth;
            ground2.position = ground2Pos;
        }

        currentGameSpeed = 0f;
        if (gameManager != null && gameManager.IsGameStarted)
            currentGameSpeed = gameManager.CurrentSpeed;
    }

    private void Update()
    {
        if (gameManager == null || !gameManager.IsGameStarted || gameManager.IsGamePaused || gameManager.IsGameOver)
            return;

        if (ground1 != null)
        {
            ground1Pos = ground1.position;
            ground1Pos.x -= currentGameSpeed * Time.deltaTime;
            if (ground1Pos.x <= leftBoundary)
                ground1Pos.x = rightBoundary;
            ground1.position = ground1Pos;
        }

        if (ground2 != null)
        {
            ground2Pos = ground2.position;
            ground2Pos.x -= currentGameSpeed * Time.deltaTime;
            if (ground2Pos.x <= leftBoundary)
                ground2Pos.x = rightBoundary;
            ground2.position = ground2Pos;
        }
    }

    private void UpdateSpeed(float newSpeed)
    {
        currentGameSpeed = newSpeed;
    }

    private void OnGameStart()
    {
        if (gameManager != null)
            currentGameSpeed = gameManager.CurrentSpeed;
    }
}
