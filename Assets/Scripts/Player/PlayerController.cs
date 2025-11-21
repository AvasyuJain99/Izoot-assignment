using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float maxJumpHeight = -3f;
    [SerializeField] private float groundYPosition = -12.59f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private BoxCollider2D playerCollider;

    private const string RUN_PARAM = "Run";
    private const string DEATH_TRIGGER = "Death";
    private const string OBSTACLE_TAG = "Obstacle";
    private const string COIN_TAG = "Coin";
    private const string POWERUP_TAG = "PowerUp";

    private bool isGrounded = true;
    private bool isDead = false;
    private bool hasShield = false;
    private bool hasDoubleJump = false;
    private bool canDoubleJump = false;
    private Transform cachedTransform;
    private GameManager gameManager;
    private AudioManager audioManager;
    private Vector2 velocityCache;
    private Vector3 positionCache;

    private void Awake()
    {
        cachedTransform = transform;
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (playerCollider == null)
            playerCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameOver += OnGameOver;
        PowerUpSystem.OnShieldActivated += ActivateShield;
        PowerUpSystem.OnShieldDeactivated += DeactivateShield;
        PowerUpSystem.OnDoubleJumpActivated += ActivateDoubleJump;
        PowerUpSystem.OnDoubleJumpDeactivated += DeactivateDoubleJump;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameOver -= OnGameOver;
        PowerUpSystem.OnShieldActivated -= ActivateShield;
        PowerUpSystem.OnShieldDeactivated -= DeactivateShield;
        PowerUpSystem.OnDoubleJumpActivated -= ActivateDoubleJump;
        PowerUpSystem.OnDoubleJumpDeactivated -= DeactivateDoubleJump;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        audioManager = AudioManager.Instance;
        if (animator != null)
            animator.SetBool(RUN_PARAM, false);
    }

    private void Update()
    {
        if (isDead || gameManager == null || !gameManager.IsGameStarted)
            return;

        bool wasGrounded = isGrounded;
        CheckGrounded();
        
        if (isGrounded && !wasGrounded)
            canDoubleJump = false;

        positionCache = cachedTransform.position;
        if (positionCache.y > maxJumpHeight)
        {
            positionCache.y = maxJumpHeight;
            cachedTransform.position = positionCache;
            velocityCache = rb.velocity;
            if (velocityCache.y > 0)
            {
                velocityCache.y = 0;
                rb.velocity = velocityCache;
            }
        }

        if (positionCache.y < groundYPosition)
        {
            positionCache.y = groundYPosition;
            cachedTransform.position = positionCache;
            velocityCache = rb.velocity;
            if (velocityCache.y < 0)
            {
                velocityCache.y = 0;
                rb.velocity = velocityCache;
            }
            isGrounded = true;
        }
    }

    public void Jump()
    {
        if (isDead || gameManager == null || !gameManager.IsGameStarted)
            return;

        velocityCache = rb.velocity;
        if (isGrounded)
        {
            velocityCache.y = jumpForce;
            rb.velocity = velocityCache;
            isGrounded = false;
            canDoubleJump = true;
        }
        else if (hasDoubleJump && canDoubleJump)
        {
            velocityCache.y = jumpForce;
            rb.velocity = velocityCache;
            canDoubleJump = false;
        }
        if (audioManager != null)
            audioManager.PlaySFX("Jump");
    }

    private void CheckGrounded()
    {
        if (Mathf.Abs(cachedTransform.position.y - groundYPosition) < 0.1f && rb.velocity.y <= 0.1f)
            isGrounded = true;
    }

    private void OnGameStart()
    {
        if (animator != null)
            animator.SetBool(RUN_PARAM, true);
    }

    private void OnGameOver()
    {
        if (animator != null)
            animator.SetTrigger(DEATH_TRIGGER);
        isDead = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        string tag = collision.tag;
        if (tag == OBSTACLE_TAG)
        {
            if (!hasShield)
                Die();
        }
        else if (tag == COIN_TAG)
        {
            CollectCoin(collision.gameObject);
        }
        else if (tag == POWERUP_TAG)
        {
            CollectPowerUp(collision.gameObject);
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        if (animator != null)
            animator.SetTrigger(DEATH_TRIGGER);

        if (gameManager != null)
            gameManager.GameOver();

        if (audioManager != null)
            audioManager.PlaySFX("Death");
    }

    private void CollectCoin(GameObject coin)
    {
        if (gameManager != null)
            gameManager.AddScore(10);

        if (audioManager != null)
            audioManager.PlaySFX("Coin");

        ObjectPooler.Instance?.ReturnToPool(coin, "Coin");
    }

    private void CollectPowerUp(GameObject powerUp)
    {
        PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
        if (powerUpComponent != null)
        {
            powerUpComponent.Activate();
            ObjectPooler.Instance?.ReturnToPool(powerUp, powerUpComponent.GetPowerUpType().ToString());
        }
    }

    private void ActivateShield()
    {
        hasShield = true;
    }

    private void DeactivateShield()
    {
        hasShield = false;
    }

    private void ActivateDoubleJump()
    {
        hasDoubleJump = true;
    }

    private void DeactivateDoubleJump()
    {
        hasDoubleJump = false;
        canDoubleJump = false;
    }

    public void ResetPlayer()
    {
        isDead = false;
        isGrounded = true;
        hasShield = false;
        hasDoubleJump = false;
        canDoubleJump = false;
        cachedTransform.position = new Vector3(-13f, groundYPosition, 0f);
        
        if (rb != null)
            rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.ResetTrigger(DEATH_TRIGGER);
            animator.SetBool(RUN_PARAM, false);
            animator.Play("Idle", 0, 0f);
        }
    }
}
