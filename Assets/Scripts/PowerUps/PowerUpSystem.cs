using UnityEngine;
using System;
using System.Collections;

public class PowerUpSystem : MonoBehaviour
{
    public static PowerUpSystem Instance { get; private set; }

    public static event Action OnShieldActivated;
    public static event Action OnShieldDeactivated;
    public static event Action OnDoubleJumpActivated;
    public static event Action OnDoubleJumpDeactivated;

    private bool hasShield = false;
    private bool hasDoubleJump = false;
    private Coroutine shieldCoroutine;
    private Coroutine doubleJumpCoroutine;
    private AudioManager audioManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                ActivateShield(duration);
                break;
            case PowerUpType.DoubleJump:
                ActivateDoubleJump(duration);
                break;
        }
    }

    private void ActivateShield(float duration)
    {
        if (audioManager != null)
            audioManager.PlaySFX("Shield");
        if (hasShield && shieldCoroutine != null)
            StopCoroutine(shieldCoroutine);

        hasShield = true;
        OnShieldActivated?.Invoke();
        shieldCoroutine = StartCoroutine(ShieldCoroutine(duration));
    }

    private void ActivateDoubleJump(float duration)
    {
        if (audioManager != null)
            audioManager.PlaySFX("DoubleJump");
        if (hasDoubleJump && doubleJumpCoroutine != null)
            StopCoroutine(doubleJumpCoroutine);

        hasDoubleJump = true;
        OnDoubleJumpActivated?.Invoke();
        doubleJumpCoroutine = StartCoroutine(DoubleJumpCoroutine(duration));
    }

    private IEnumerator ShieldCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        hasShield = false;
        OnShieldDeactivated?.Invoke();
        shieldCoroutine = null;
    }

    private IEnumerator DoubleJumpCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        hasDoubleJump = false;
        OnDoubleJumpDeactivated?.Invoke();
        doubleJumpCoroutine = null;
    }

    public bool HasShield() => hasShield;

    public bool HasDoubleJump() => hasDoubleJump;

    public void ResetPowerUps()
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }

        if (doubleJumpCoroutine != null)
        {
            StopCoroutine(doubleJumpCoroutine);
            doubleJumpCoroutine = null;
        }

        hasShield = false;
        hasDoubleJump = false;
        OnShieldDeactivated?.Invoke();
        OnDoubleJumpDeactivated?.Invoke();
    }
}
