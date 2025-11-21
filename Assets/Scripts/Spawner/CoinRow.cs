using UnityEngine;

public class CoinRow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float despawnDistance = -30f;

    private Transform playerTransform;
    private float currentSpeed = 0f;
    private Transform cachedTransform;
    private GameManager gameManager;

    private void Start()
    {
        cachedTransform = transform;
        gameManager = GameManager.Instance;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void OnEnable()
    {
        GameManager.OnSpeedChanged += UpdateSpeed;
        if (gameManager != null)
            currentSpeed = gameManager.CurrentSpeed;
    }

    private void OnDisable()
    {
        GameManager.OnSpeedChanged -= UpdateSpeed;
    }

    private void Update()
    {
        if (gameManager == null || !gameManager.IsGameStarted || gameManager.IsGamePaused || gameManager.IsGameOver)
            return;

        cachedTransform.position += Vector3.left * currentSpeed * Time.deltaTime;

        if (playerTransform != null && cachedTransform.position.x < playerTransform.position.x + despawnDistance)
            Destroy(gameObject);
    }

    private void UpdateSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
}
