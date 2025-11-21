using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float despawnDistance = -30f;
    [SerializeField] private bool shouldMove = true;

    private Transform playerTransform;
    private float currentSpeed = 0f;
    private Transform cachedTransform;
    private GameManager gameManager;
    private ObjectPooler objectPooler;
    private PowerUp cachedPowerUp;
    private string cachedTag;
    private string poolTag;
    private bool tagChecked = false;

    private void Awake()
    {
        cachedTransform = transform;
        cachedTag = gameObject.tag;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        objectPooler = ObjectPooler.Instance;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void OnEnable()
    {
        GameManager.OnSpeedChanged += UpdateSpeed;
        if (gameManager != null)
            currentSpeed = gameManager.CurrentSpeed;
        tagChecked = false;
    }

    private void OnDisable()
    {
        GameManager.OnSpeedChanged -= UpdateSpeed;
    }

    private void Update()
    {
        if (!shouldMove || gameManager == null || !gameManager.IsGameStarted || gameManager.IsGamePaused || gameManager.IsGameOver)
            return;

        cachedTransform.position += Vector3.left * currentSpeed * Time.deltaTime;

        if (playerTransform != null && cachedTransform.position.x < playerTransform.position.x + despawnDistance)
            Despawn();
    }

    private void UpdateSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    private void Despawn()
    {
        if (objectPooler != null)
        {
            if (!tagChecked)
            {
                if (cachedTag == "Obstacle")
                    poolTag = "Obstacle";
                else if (cachedTag == "Coin")
                    poolTag = "Coin";
                else if (cachedTag == "PowerUp")
                {
                    if (cachedPowerUp == null)
                        cachedPowerUp = GetComponent<PowerUp>();
                    if (cachedPowerUp != null)
                        poolTag = cachedPowerUp.GetPowerUpType().ToString();
                }
                tagChecked = true;
            }

            if (!string.IsNullOrEmpty(poolTag))
            {
                objectPooler.ReturnToPool(gameObject, poolTag);
                return;
            }
        }

        Destroy(gameObject);
    }
}
