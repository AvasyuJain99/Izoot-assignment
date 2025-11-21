using UnityEngine;

public class CoinRow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float despawnDistance = -30f;

    private Transform playerTransform;
    private float currentSpeed = 0f;

    private void OnEnable()
    {
        GameManager.OnSpeedChanged += UpdateSpeed;
        if (GameManager.Instance != null)
        {
            currentSpeed = GameManager.Instance.CurrentSpeed;
        }
    }

    private void OnDisable()
    {
        GameManager.OnSpeedChanged -= UpdateSpeed;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (!GameManager.Instance || !GameManager.Instance.IsGameStarted || GameManager.Instance.IsGamePaused || GameManager.Instance.IsGameOver)
            return;

        transform.position += Vector3.left * currentSpeed * Time.deltaTime;

        if (playerTransform != null && transform.position.x < playerTransform.position.x + despawnDistance)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
}
