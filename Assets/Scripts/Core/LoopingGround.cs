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
        if (ground1 != null)
        {
            ground1.position = new Vector3(0f, ground1.position.y, ground1.position.z);
        }

        if (ground2 != null)
        {
            ground2.position = new Vector3(groundWidth, ground2.position.y, ground2.position.z);
        }

        currentGameSpeed = 0f;
        
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsGameStarted)
            {
                currentGameSpeed = GameManager.Instance.CurrentSpeed;
            }
        }
    }

    private void Update()
    {
        if (!GameManager.Instance || !GameManager.Instance.IsGameStarted || GameManager.Instance.IsGamePaused || GameManager.Instance.IsGameOver)
            return;

        if (ground1 != null)
        {
            ground1.position += Vector3.left * currentGameSpeed * Time.deltaTime;
            
            if (ground1.position.x <= leftBoundary)
            {
                Vector3 newPos = ground1.position;
                newPos.x = rightBoundary;
                ground1.position = newPos;
            }
        }

        if (ground2 != null)
        {
            ground2.position += Vector3.left * currentGameSpeed * Time.deltaTime;
            
            if (ground2.position.x <= leftBoundary)
            {
                Vector3 newPos = ground2.position;
                newPos.x = rightBoundary;
                ground2.position = newPos;
            }
        }
    }

    private void UpdateSpeed(float newSpeed)
    {
        currentGameSpeed = newSpeed;
    }

    private void OnGameStart()
    {
        if (GameManager.Instance != null)
        {
            currentGameSpeed = GameManager.Instance.CurrentSpeed;
        }
    }
}
