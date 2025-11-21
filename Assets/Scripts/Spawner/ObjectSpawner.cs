using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float fixedSpawnX = 50f;
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 4f;
    [SerializeField] private float safeZoneWidth = 5f;
    [SerializeField] private float cleanupDistance = 30f;

    [Header("Spawn Positions")]
    [SerializeField] private float obstacleYPosition = -12.59f;
    [SerializeField] private float boosterYPosition = -3f;
    [SerializeField] private float minCoinYPosition = -12.59f;
    [SerializeField] private float maxCoinYPosition = -3f;

    [Header("Prefabs")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject shieldPowerUpPrefab;
    [SerializeField] private GameObject doubleJumpPowerUpPrefab;

    [Header("Spawn Probabilities")]
    [SerializeField] [Range(0f, 1f)] private float obstacleSpawnChance = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float coinSpawnChance = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float powerUpSpawnChance = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float coinRowSpawnChance = 0.3f;

    [Header("Coin Row Settings")]
    [SerializeField] private int maxCoinsInRow = 6;
    [SerializeField] private float coinSpacing = 2f;

    private float nextSpawnTime = 0f;
    private float lastSpawnX = 0f;
    private List<Vector3> activeSpawnPositions = new List<Vector3>();
    private GameManager gameManager;
    private Transform cachedTransform;
    private float safeZoneSqr;
    private float cleanupDistanceSqr;
    private ObjectPooler objectPooler;

    private void Awake()
    {
        cachedTransform = transform;
        safeZoneSqr = safeZoneWidth * safeZoneWidth;
        cleanupDistanceSqr = cleanupDistance * cleanupDistance;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        objectPooler = ObjectPooler.Instance;
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameOver -= OnGameOver;
    }

    private void Update()
    {
        if (gameManager == null || !gameManager.IsGameStarted || gameManager.IsGamePaused || gameManager.IsGameOver)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnObjects();
            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        CleanupSpawnPositions();
    }

    private void OnGameStart()
    {
        nextSpawnTime = Time.time + 2f;
        lastSpawnX = fixedSpawnX;
        activeSpawnPositions.Clear();
    }

    private void OnGameOver()
    {
    }

    private void SpawnObjects()
    {
        float spawnX = fixedSpawnX;
        Vector3 spawnPosition = new Vector3(spawnX, obstacleYPosition, 0f);

        if (!IsPositionSafe(spawnPosition))
        {
            spawnX += safeZoneWidth;
            spawnPosition.x = spawnX;
        }

        float randomValue = Random.value;
        float threshold1 = powerUpSpawnChance;
        float threshold2 = threshold1 + obstacleSpawnChance;

        if (randomValue < threshold1)
            SpawnPowerUp(spawnX);
        else if (randomValue < threshold2)
            SpawnObstacle(spawnX);
        else if (randomValue < threshold2 + coinSpawnChance)
            SpawnCoins(spawnX);

        lastSpawnX = spawnX;
        activeSpawnPositions.Add(spawnPosition);
    }

    private void SpawnObstacle(float spawnX)
    {
        if (obstaclePrefab == null)
            return;

        Vector3 obstaclePos = new Vector3(spawnX, obstacleYPosition, 0f);
        GameObject obstacle = objectPooler?.SpawnFromPool("Obstacle", obstaclePos, Quaternion.identity);
        if (obstacle == null && obstaclePrefab != null)
            obstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
    }

    private void SpawnCoins(float spawnX)
    {
        if (Random.value < coinRowSpawnChance)
            SpawnCoinRow(spawnX);
        else
            SpawnSingleCoin(spawnX);
    }

    private void SpawnSingleCoin(float spawnX)
    {
        if (coinPrefab == null)
            return;

        float randomY = Random.Range(minCoinYPosition, maxCoinYPosition);
        Vector3 coinPos = new Vector3(spawnX, randomY, 0f);
        GameObject coin = objectPooler?.SpawnFromPool("Coin", coinPos, Quaternion.identity);
        if (coin == null && coinPrefab != null)
            coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
    }

    private void SpawnCoinRow(float spawnX)
    {
        if (coinPrefab == null)
            return;

        int coinCount = Random.Range(1, maxCoinsInRow + 1);
        float rowY = Random.Range(minCoinYPosition, maxCoinYPosition);
        
        for (int i = 0; i < coinCount; i++)
        {
            float coinX = spawnX + (i * coinSpacing);
            Vector3 coinPos = new Vector3(coinX, rowY, 0f);
            GameObject coin = objectPooler?.SpawnFromPool("Coin", coinPos, Quaternion.identity);
            if (coin == null && coinPrefab != null)
                coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
        }
    }

    private void SpawnPowerUp(float spawnX)
    {
        GameObject powerUpPrefab;
        string poolTag;

        if (Random.value < 0.5f)
        {
            powerUpPrefab = shieldPowerUpPrefab;
            poolTag = "Shield";
        }
        else
        {
            powerUpPrefab = doubleJumpPowerUpPrefab;
            poolTag = "DoubleJump";
        }

        if (powerUpPrefab == null)
            return;

        Vector3 powerUpPos = new Vector3(spawnX, boosterYPosition, 0f);
        GameObject powerUp = objectPooler?.SpawnFromPool(poolTag, powerUpPos, Quaternion.identity);
        if (powerUp == null)
            powerUp = Instantiate(powerUpPrefab, powerUpPos, Quaternion.identity);
    }

    private bool IsPositionSafe(Vector3 position)
    {
        float posX = position.x;
        float posY = position.y;
        for (int i = 0; i < activeSpawnPositions.Count; i++)
        {
            Vector3 activePos = activeSpawnPositions[i];
            float dx = posX - activePos.x;
            float dy = posY - activePos.y;
            if ((dx * dx + dy * dy) < safeZoneSqr)
                return false;
        }
        return true;
    }

    private void CleanupSpawnPositions()
    {
        float cleanupX = cachedTransform.position.x - cleanupDistance;
        for (int i = activeSpawnPositions.Count - 1; i >= 0; i--)
        {
            if (activeSpawnPositions[i].x < cleanupX)
                activeSpawnPositions.RemoveAt(i);
        }
    }
}
