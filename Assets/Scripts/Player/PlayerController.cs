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

    private bool isGrounded = true;
    private bool isDead = false;
    private bool hasShield = false;
    private bool hasDoubleJump = false;
    private bool canDoubleJump = false;

    private void Awake()
    {
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
        if (animator != null)
        {
            animator.SetBool(RUN_PARAM, false);
        }
    }

    private void Update()
    {
        if (isDead || !GameManager.Instance || !GameManager.Instance.IsGameStarted)
            return;

        bool wasGrounded = isGrounded;
        CheckGrounded();
        
        if (isGrounded && !wasGrounded)
        {
            canDoubleJump = false;
        }

        if (transform.position.y > maxJumpHeight)
        {
            Vector3 pos = transform.position;
            pos.y = maxJumpHeight;
            transform.position = pos;
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }

        if (transform.position.y < groundYPosition)
        {
            Vector3 pos = transform.position;
            pos.y = groundYPosition;
            transform.position = pos;
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            isGrounded = true;
        }
    }

    public void Jump()
    {
        if (isDead || !GameManager.Instance || !GameManager.Instance.IsGameStarted)
            return;

        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            canDoubleJump = true;
        }
        else if (hasDoubleJump && canDoubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canDoubleJump = false;
        }
        AudioManager.Instance.PlaySFX("Jump");
    }

    private void CheckGrounded()
    {
        if (Mathf.Abs(transform.position.y - groundYPosition) < 0.1f && rb.velocity.y <= 0.1f)
        {
            isGrounded = true;
        }
    }

    private void OnGameStart()
    {
        if (animator != null)
        {
            animator.SetBool(RUN_PARAM, true);
        }
    }

    private void OnGameOver()
    {
        if (animator != null)
        {
            animator.SetTrigger(DEATH_TRIGGER);
        }
        isDead = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.CompareTag("Obstacle"))
        {
            if (!hasShield)
            {
                Die();
            }
        }
        else if (collision.CompareTag("Coin"))
        {
            CollectCoin(collision.gameObject);
        }
        else if (collision.CompareTag("PowerUp"))
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
        {
            animator.SetTrigger(DEATH_TRIGGER);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Death");
        }
    }

    private void CollectCoin(GameObject coin)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Coin");
        }

        ObjectPooler.Instance?.ReturnToPool(coin, "Coin");
    }

    private void CollectPowerUp(GameObject powerUp)
    {
        PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
        if (powerUpComponent != null)
        {
            powerUpComponent.Activate();
        }

        ObjectPooler.Instance?.ReturnToPool(powerUp, powerUpComponent.GetPowerUpType().ToString());
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
        transform.position = new Vector3(-13f, groundYPosition, 0f);
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.ResetTrigger(DEATH_TRIGGER);
            animator.SetBool(RUN_PARAM, false);
            animator.Play("Idle", 0, 0f);
        }
    }
}
