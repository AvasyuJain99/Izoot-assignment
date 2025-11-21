using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float despawnDistance = -30f;
    [SerializeField] private bool shouldMove = true;

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
        if (!shouldMove || !GameManager.Instance || !GameManager.Instance.IsGameStarted || GameManager.Instance.IsGamePaused || GameManager.Instance.IsGameOver)
            return;

        transform.position += Vector3.left * currentSpeed * Time.deltaTime;

        if (playerTransform != null && transform.position.x < playerTransform.position.x + despawnDistance)
        {
            Despawn();
        }
    }

    private void UpdateSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    private void Despawn()
    {
        if (ObjectPooler.Instance != null)
        {
            string poolTag = "";
            if (gameObject.CompareTag("Obstacle"))
                poolTag = "Obstacle";
            else if (gameObject.CompareTag("Coin"))
                poolTag = "Coin";
            else if (gameObject.CompareTag("PowerUp"))
            {
                PowerUp powerUp = GetComponent<PowerUp>();
                if (powerUp != null)
                {
                    poolTag = powerUp.GetPowerUpType().ToString();
                }
            }

            if (!string.IsNullOrEmpty(poolTag))
            {
                ObjectPooler.Instance.ReturnToPool(gameObject, poolTag);
                return;
            }
        }

        Destroy(gameObject);
    }
}
