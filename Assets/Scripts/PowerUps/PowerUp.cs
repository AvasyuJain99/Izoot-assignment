using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    [SerializeField] protected PowerUpType powerUpType;
    [SerializeField] protected float duration = 10f;

    public PowerUpType GetPowerUpType() => powerUpType;

    public virtual void Activate()
    {
        if (PowerUpSystem.Instance != null)
        {
            PowerUpSystem.Instance.ActivatePowerUp(powerUpType, duration);
        }

        gameObject.SetActive(false);
    }
}

public enum PowerUpType
{
    Shield,
    DoubleJump
}
