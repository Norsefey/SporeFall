using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EndlessWaveManager : MonoBehaviour
{
    public PlayerInputManager inputManager;

    [Header("Events")]
    public UnityEvent<int> onWaveNumberChanged;
    public UnityEvent<int> onEnemyDefeated;
    public UnityEvent onBossSpawned;
    public UnityEvent<int> onBossDefeated;
    public UnityEvent onWaveDowntimeStarted; // for downtime start
    public UnityEvent<float> onWaveDowntimeProgress; // for downtime progress
    public UnityEvent onWaveDowntimeEnded;


    [Header("References")]
    [SerializeField] private List<GameObject> bossPrefabs; // Multiple boss prefabs for variety
    private Transform playerTransform;
    [Header("Wave Settings")]
    [Range(0, 1)]
    [SerializeField] private float outsideSpawnChance = 0.45f;
    [SerializeField] private int maxEnemiesOnField = 300;
    [SerializeField] private float baseDifficulty = 1.0f;
    [SerializeField] private float difficultyIncreasePerWave = 0.1f;
    [SerializeField] private float difficultyIncreaseOverTime = 0.05f;
    [SerializeField] private float difficultyTimeInterval = 60f; // Increase difficulty every minute
    
    [Header("Wave Downtime Settings")]
    [SerializeField] private float waveDowntimeDuration = 10f; // Duration of downtime between waves in seconds
    [SerializeField] private bool enableWaveDowntime = true; // Toggle for enabling/disabling downtime
    public float DowntimeDuration => waveDowntimeDuration;
    [Header("NavMesh Settings")]
    [SerializeField] private float navMeshSampleDistance = 5f; // Distance to sample when finding valid NavMesh positions
    [SerializeField] private LayerMask groundLayer; // Layer for the ground

    [Header("Enemy Spawn Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private int initialSpawnCount = 20;
    [SerializeField] private float initialSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private int initialMaxEnemiesPerSpawn = 3;
    [SerializeField] private int maxEnemiesPerSpawn = 10;

    [Header("Boss Settings")]
    [SerializeField] private float timeBetweenBossSpawns = 300f; // 5 minutes
    [SerializeField] private float minTimeBetweenBossSpawns = 180f; // 3 minutes
    [SerializeField] private float bossSpawnTimeReduction = 10f; // Reduce boss spawn time by 10 seconds per wave
    [SerializeField] private float bossHealthMultiplier = 1.5f; // Boss health multiplier for each subsequent boss
    [SerializeField] private float bossDamageMultiplier = 1.25f;

    [Header("Spawn Zones")]
    [SerializeField] private Transform[] presetSpawnPoints;
    [SerializeField] private Collider outsideSpawnZone;

    // Wave State
    private float currentDifficulty;
    public float CurrentDifficulty { get { return currentDifficulty; } }

    private int currentWaveNumber = 0;
    private float enemySpawnInterval;
    private int maxEnemiesPerSpawnCurrent;
    private int totalSpawnCountForCurrentWave;
    private float bossSpawnTimer;
    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private int deadEnemies = 0;
    private int bossesDefeated = 0;
    private bool isBossActive = false;
    private BaseEnemy activeBoss = null;
    private float difficultyTimer = 0f;
    private float downtimeTimer = 0f;

    // Track wave-specific enemies
    private int enemiesSpawnedThisWave = 0;
    private bool waveSpawningComplete = false;
    // Object Pooling
    [SerializeField] private int initialPoolSize = 50;
    private Dictionary<GameObject, EnemyObjectPool> enemyPools;
    private Dictionary<GameObject, EnemyObjectPool> bossPools;

    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();
    private Coroutine spawnCoroutine;
    private Coroutine downtimeCoroutine;

    public enum WaveState
    {
        NotStarted,
        InProgress,
        BossFight,
        Downtime
    }

    public WaveState currentState = WaveState.NotStarted;

    private void Start()
    {
        inputManager.onPlayerJoined += OnPlayerJoined;
        InitializePools();
        ResetWaveParameters();

        // Set initial difficulty
        currentDifficulty = baseDifficulty;
        bossSpawnTimer = timeBetweenBossSpawns;
    }

    private void Update()
    {
        if (currentState != WaveState.NotStarted)
        {
            // Update boss spawn timer
            if (!isBossActive && currentState != WaveState.Downtime)
            {
                bossSpawnTimer -= Time.deltaTime;
                if (bossSpawnTimer <= 0)
                {
                    SpawnBoss();
                    ResetBossSpawnTimer();
                }
            }

            // Update difficulty over time (not during downtime)
            if (currentState != WaveState.Downtime)
            {
                difficultyTimer += Time.deltaTime;
                if (difficultyTimer >= difficultyTimeInterval)
                {
                    IncreaseDifficultyOverTime();
                    difficultyTimer = 0;
                }
            }

            // Update downtime progress for UI
            if (currentState == WaveState.Downtime)
            {
                float progress = 1f - (downtimeTimer / waveDowntimeDuration);
                onWaveDowntimeProgress?.Invoke(progress);
            }
            // Check if wave is complete
            if (currentState == WaveState.InProgress && waveSpawningComplete && enemiesAlive == 0)
            {
                // All enemies have been defeated, move to next wave
                NextWave();
            }
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (currentState == WaveState.NotStarted && currentWaveNumber == 0)
            Invoke(nameof(StartEndlessWaves), 3);

        inputManager.onPlayerJoined -= OnPlayerJoined;
    }

    #region Wave Management

    public void StartEndlessWaves()
    {
        if (currentState == WaveState.NotStarted)
        {
            currentState = WaveState.InProgress;
            currentWaveNumber = 1;

            deadEnemies = 0;
            enemiesSpawnedThisWave = 0;
            waveSpawningComplete = false;
            onWaveNumberChanged?.Invoke(currentWaveNumber);

            spawnCoroutine = StartCoroutine(SpawnEnemiesForWave());
        }
    }
    private void ResetWaveParameters()
    {
        enemySpawnInterval = initialSpawnInterval;
        maxEnemiesPerSpawnCurrent = initialMaxEnemiesPerSpawn;
        totalSpawnCountForCurrentWave = initialSpawnCount;
        enemiesSpawnedThisWave = 0;
        waveSpawningComplete = false;
        // Reset spawn counts for all enemy types
        foreach (var enemyType in enemyTypes)
        {
            enemyType.SpawnedCount = 0;
        }
    }
    private void NextWave()
    {
        if (enableWaveDowntime)
        {
            StartDowntime();
        }
        else
        {
            AdvanceToNextWave();
        }
    }
    private void StartDowntime()
    {
        // Stop enemy spawning during downtime
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Set state to downtime
        currentState = WaveState.Downtime;
        downtimeTimer = waveDowntimeDuration;

        // Notify listeners about downtime start
        onWaveDowntimeStarted?.Invoke();

        // Start downtime coroutine
        downtimeCoroutine = StartCoroutine(DowntimeCoroutine());
    }
    private IEnumerator DowntimeCoroutine()
    {
        while (downtimeTimer > 0)
        {
            downtimeTimer -= Time.deltaTime;
            yield return null;
        }

        // End downtime
        onWaveDowntimeEnded?.Invoke();

        // Advance to next wave
        AdvanceToNextWave();
    }
    private void AdvanceToNextWave()
    {
        currentWaveNumber++;
        currentDifficulty += difficultyIncreasePerWave;

        // Increase spawn rate and count as difficulty increases
        enemySpawnInterval = Mathf.Max(minSpawnInterval, initialSpawnInterval / currentDifficulty);
        maxEnemiesPerSpawnCurrent = Mathf.Min(maxEnemiesPerSpawn,
            initialMaxEnemiesPerSpawn + Mathf.FloorToInt(currentDifficulty / 2));
        totalSpawnCountForCurrentWave = initialSpawnCount + (currentWaveNumber * 10);

        // Reset enemy count for new wave
        deadEnemies = 0;
        enemiesSpawnedThisWave = 0;
        waveSpawningComplete = false;

        onWaveNumberChanged?.Invoke(currentWaveNumber);

        // Reset enemy type spawn counts
        foreach (var enemyType in enemyTypes)
        {
            enemyType.SpawnedCount = 0;
        }

        foreach(Structure structure in GameManager.Instance.activeStructures)
        {
            structure.UpdateEndlessStats();
        }

        // Restart enemy spawning if we were in downtime
        if (currentState == WaveState.Downtime)
        {
            currentState = WaveState.InProgress;
            spawnCoroutine = StartCoroutine(SpawnEnemiesForWave());
        }
    }
    private void IncreaseDifficultyOverTime()
    {
        currentDifficulty += difficultyIncreaseOverTime;

        // Update enemy spawn parameters based on new difficulty
        enemySpawnInterval = Mathf.Max(minSpawnInterval, initialSpawnInterval / currentDifficulty);
        maxEnemiesPerSpawnCurrent = Mathf.Min(maxEnemiesPerSpawn,
            initialMaxEnemiesPerSpawn + Mathf.FloorToInt(currentDifficulty / 2));
    }
    private void ResetBossSpawnTimer()
    {
        // Reduce time between boss spawns as game progresses
        float newTime = timeBetweenBossSpawns - (bossesDefeated * bossSpawnTimeReduction);
        bossSpawnTimer = Mathf.Max(minTimeBetweenBossSpawns, newTime);
    }
    public void SkipDowntime()
    {
        if (currentState == WaveState.Downtime)
        {
            if (downtimeCoroutine != null)
            {
                StopCoroutine(downtimeCoroutine);
                downtimeCoroutine = null;
            }

            // End downtime immediately
            onWaveDowntimeEnded?.Invoke();

            // Advance to next wave
            AdvanceToNextWave();
        }
    }

    #endregion

    #region Enemy Spawning
    private IEnumerator SpawnEnemiesForWave()
    {
        waveSpawningComplete = false;
        enemiesSpawnedThisWave = 0;

        while (currentState == WaveState.InProgress && enemiesSpawnedThisWave < totalSpawnCountForCurrentWave)
        {
            if (enemiesAlive < maxEnemiesOnField)
            {
                int spawnCount = Mathf.Min(
                    Random.Range(1, maxEnemiesPerSpawnCurrent + 1),
                    totalSpawnCountForCurrentWave - enemiesSpawnedThisWave
                );

                for (int i = 0; i < spawnCount; i++)
                {
                    if (enemiesAlive < maxEnemiesOnField && enemiesSpawnedThisWave < totalSpawnCountForCurrentWave)
                    {
                        SpawnRandomEnemy();
                        enemiesSpawnedThisWave++;
                    }
                }
            }

            yield return new WaitForSeconds(enemySpawnInterval);
        }

        // Mark wave spawning as complete
        waveSpawningComplete = true;

        // Check if all enemies are already defeated
        if (enemiesAlive == 0)
        {
            NextWave();
        }
    }
    private void SpawnRandomEnemy()
    {
        // Weight enemy selection based on difficulty
        List<EnemySpawnData> availableEnemies = GetAvailableEnemies();
        if (availableEnemies.Count == 0) return;

        // Select enemy based on difficulty
        EnemySpawnData selectedEnemy = GetEnemyBasedOnDifficulty(availableEnemies);

        Vector3 spawnPoint;
        bool spawningOutside = false;

        // Determine spawn location
        if (selectedEnemy.mustSpawnOutside)
        {
            spawnPoint = GetSpawnPointWithinZone();
            spawningOutside = true;
        }
        else if (selectedEnemy.spawnPointIndex != -1)
        {
            spawnPoint = presetSpawnPoints[selectedEnemy.spawnPointIndex].position;
        }
        else
        {
            if (Random.value < outsideSpawnChance)
            {
                spawnPoint = GetSpawnPointWithinZone();
                spawningOutside = true;
            }
            else
            {
                int spawnPointIndex = Random.Range(0, presetSpawnPoints.Length);
                if (spawnPointIndex > 3) spawningOutside = true;
                spawnPoint = presetSpawnPoints[spawnPointIndex].position;
            }
        }

        // Handle group spawning
        if (selectedEnemy.spawnAsGroup)
        {
            for (int i = 0; i < selectedEnemy.groupSize; i++)
            {
                Vector3 spawnPointOffset = new Vector3(
                    spawnPoint.x + (i * 2),
                    spawnPoint.y,
                    spawnPoint.z + Random.Range(-1, 1)
                );
                SpawnEnemy(selectedEnemy.EnemyToSpawn, spawnPointOffset, spawningOutside);
                selectedEnemy.SpawnedCount++;
            }
        }
        else
        {
            SpawnEnemy(selectedEnemy.EnemyToSpawn, spawnPoint, spawningOutside);
            selectedEnemy.SpawnedCount++;
        }
    }
    private List<EnemySpawnData> GetAvailableEnemies()
    {
        List<EnemySpawnData> available = new List<EnemySpawnData>();

        foreach (var enemy in enemyTypes)
        {
            // Add enemies that are appropriate for current difficulty
            if (enemy.minDifficultyToSpawn <= currentDifficulty)
            {
                available.Add(enemy);
            }
        }

        return available;
    }
    private EnemySpawnData GetEnemyBasedOnDifficulty(List<EnemySpawnData> availableEnemies)
    {
        // Calculate total weight
        float totalWeight = 0;
        foreach (var enemy in availableEnemies)
        {
            float weight = CalculateSpawnWeight(enemy);
            totalWeight += weight;
        }

        // Pick random enemy based on weight
        float randomValue = Random.Range(0, totalWeight);
        float weightSum = 0;

        foreach (var enemy in availableEnemies)
        {
            float weight = CalculateSpawnWeight(enemy);
            weightSum += weight;

            if (randomValue <= weightSum)
            {
                return enemy;
            }
        }

        // Fallback
        return availableEnemies[0];
    }
    private float CalculateSpawnWeight(EnemySpawnData enemy)
    {
        // Higher difficulty enemies become more common as difficulty increases
        float difficultyFactor = currentDifficulty - enemy.minDifficultyToSpawn + 1f;
        return enemy.spawnWeight * difficultyFactor;
    }
    private void SpawnBoss()
    {
        if (isBossActive) return;

        currentState = WaveState.BossFight;
        isBossActive = true;

        // Select a random boss from available bosses
        if (bossPrefabs.Count == 0)
        {
            Debug.LogError("No boss prefabs assigned to EndlessWaveManager!");
            return;
        }

        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];

        // Find a good spawn position for the boss
        Vector3 spawnPosition;
        if (presetSpawnPoints.Length > 0)
        {
            spawnPosition = presetSpawnPoints[0].position;
        }
        else
        {
            // If no preset points, spawn at a reasonable distance from player
            Vector3 playerPos = playerTransform != null ? playerTransform.position : transform.position;
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            Vector3 potentialPos = playerPos + direction * 15f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialPos, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                spawnPosition = playerPos + new Vector3(15f, 0, 15f);
            }
        }

        // Get boss from object pool
        BaseEnemy boss = bossPools[bossPrefab].Get(spawnPosition, Quaternion.identity);
        if (boss.GetComponent<NavMeshAgent>() != null)
        {
            boss.GetComponent<NavMeshAgent>().Warp(spawnPosition); // Ensure boss is properly placed on NavMesh
        }

        // Assign target - now using method that doesn't depend on TrainHandler
        boss.SetTarget(playerTransform);
        boss.OnEnemyDeath += OnBossDeath;

        // Scale boss health based on number of bosses defeated
        float healthMultiplier = 1f + (bossHealthMultiplier * bossesDefeated);
        float damageMultiplier = 1f + (bossDamageMultiplier * bossesDefeated);
        boss.SetHealthMultiplier(healthMultiplier);
        boss.SetDamageMultiplier(damageMultiplier);

        activeBoss = boss;
        enemiesAlive++;

        // Notify listeners about boss spawn
        onBossSpawned?.Invoke();

        // Spawn boss squad
        StartCoroutine(SpawnBossSquad());
    }
    private IEnumerator SpawnBossSquad()
    {
        // Get stronger enemies for the boss squad
        List<EnemySpawnData> squadEnemies = enemyTypes
            .Where(e => e.minDifficultyToSpawn >= currentDifficulty * 0.75f)
            .Take(3)
            .ToList();

        if (squadEnemies.Count == 0)
        {
            squadEnemies = enemyTypes.Take(3).ToList();
        }

        // Spawn squad members
        int squadSize = Mathf.FloorToInt(5 + currentDifficulty);

        for (int i = 0; i < squadSize; i++)
        {
            EnemySpawnData squadMember = squadEnemies[Random.Range(0, squadEnemies.Count)];
            Vector3 spawnPoint;
            bool spawningOutside = false;

            // Determine spawn location
            if (squadMember.mustSpawnOutside)
            {
                spawnPoint = GetSpawnPointWithinZone();
                spawningOutside = true;
            }
            else if (squadMember.spawnPointIndex != -1)
            {
                spawnPoint = presetSpawnPoints[squadMember.spawnPointIndex].position;
            }
            else
            {
                if (Random.value < outsideSpawnChance)
                {
                    spawnPoint = GetSpawnPointWithinZone();
                    spawningOutside = true;
                }
                else
                {
                    int spawnPointIndex = Random.Range(0, presetSpawnPoints.Length);
                    if (spawnPointIndex > 3) spawningOutside = true;
                    spawnPoint = presetSpawnPoints[spawnPointIndex].position;
                }
            }

            SpawnEnemy(
                squadMember.EnemyToSpawn,
                spawnPoint,
                spawningOutside
            );

            yield return new WaitForSeconds(0.2f);
        }
    }
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPoint, bool spawningOutside)
    {
        if (enemyPools == null) return;

        if (!enemyPools.TryGetValue(enemyPrefab, out EnemyObjectPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {enemyPrefab.name}");
            return;
        }

        SetRandomPlayerTarget();
        // Look direction - point toward center/player if outside spawning
        Vector3 lookTarget = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 lookDirection = (lookTarget - spawnPoint).normalized;
        if (lookDirection.sqrMagnitude < 0.001f) lookDirection = -Vector3.forward;
        Quaternion rotation = Quaternion.LookRotation(lookDirection);

        BaseEnemy enemy = pool.Get(spawnPoint, rotation);

        // Ensure NavMeshAgent is properly placed
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPoint); // This helps ensure proper NavMesh placement

            // If somehow still off NavMesh, try to find closest NavMesh point
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnPoint, out hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }

        // Set target player instead of train
        enemy.SetTarget(playerTransform);
        enemy.OnEnemyDeath += OnEnemyDeath;

        // Scale enemy health based on difficulty
        enemy.SetHealthMultiplier(Mathf.Sqrt(currentDifficulty));
        enemy.SetDamageMultiplier(Mathf.Sqrt(currentDifficulty));

        // Play rise animation for enemies spawning outside
        if (spawningOutside)
        {
            enemy.TriggerRiseAnimation();
        }

        activeEnemies.Add(enemy);
        enemiesAlive++;
        enemiesSpawned++;
    }
    private void OnEnemyDeath(BaseEnemy enemy)
    {
        enemiesAlive--;
        deadEnemies++;
        Debug.Log("Removing Dead Enemy: " +enemy.name);
        activeEnemies.Remove(enemy);

        // Notify listeners about enemy defeated
        onEnemyDefeated?.Invoke(deadEnemies);

        if (enemyPools.TryGetValue(enemy.gameObject, out EnemyObjectPool pool))
        {
            pool.Return(enemy);
        }

        // Check if we should move to next wave (all enemies spawned and none alive)
        if (currentState == WaveState.InProgress && waveSpawningComplete && enemiesAlive == 0)
        {
            NextWave();
        }
    }
    private void OnBossDeath(BaseEnemy boss)
    {
        enemiesAlive--;
        bossesDefeated++;
        isBossActive = false;
        activeBoss = null;

        // Return boss to pool
        if (bossPools.TryGetValue(boss.gameObject, out EnemyObjectPool pool))
        {
            pool.Return(boss);
        }

        // Notify listeners about boss defeated
        onBossDefeated?.Invoke(bossesDefeated);

        // Reset to normal wave
        currentState = WaveState.InProgress;
        ResetBossSpawnTimer();
        
        // Increase difficulty after boss is defeated
        currentDifficulty += difficultyIncreasePerWave * 2;

        // Check if all regular enemies are dead before advancing to next wave
        if (enemiesAlive == 0)
        {
            NextWave();
        }
        else
        {
            // Wait for remaining enemies to be defeated before starting next wave
            waveSpawningComplete = true;
        }
    }
    protected Vector3 GetSpawnPointWithinZone()
    {
        Bounds zoneBounds = outsideSpawnZone.bounds;
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate random position within zone bounds
            float offsetX = Random.Range(-zoneBounds.extents.x, zoneBounds.extents.x);
            float offsetZ = Random.Range(-zoneBounds.extents.z, zoneBounds.extents.z);
            Vector3 randomPoint = zoneBounds.center + new Vector3(offsetX, 0, offsetZ);

            // Raycast down to find ground
            RaycastHit groundHit;
            if (Physics.Raycast(randomPoint + Vector3.up * 10f, Vector3.down, out groundHit, 20f, groundLayer))
            {
                randomPoint = groundHit.point;
            }

            // Check if point is on NavMesh with a larger sampling distance
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                if (!Physics.CheckSphere(hit.position, 0.5f, LayerMask.GetMask("Obstacle")))
                {
                    return hit.position;
                }
            }
        }

        // If no valid point found after max attempts, try around player position
        Vector3 playerPosition = playerTransform != null ? playerTransform.position : transform.position;
        for (int i = 0; i < 10; i++)
        {
            Vector3 direction = Random.insideUnitSphere * 10f;
            direction.y = 0;
            Vector3 potentialPoint = playerPosition + direction;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialPoint, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // Last resort fallback
        if (presetSpawnPoints.Length > 0)
        {
            int index = Random.Range(0, presetSpawnPoints.Length);
            return presetSpawnPoints[index].position;
        }

        // Absolute last resort - just return player position + offset
        Debug.LogWarning("EndlessWaveManager: Failed to find valid spawn point - using player position with offset");
        return (playerTransform != null ? playerTransform.position : transform.position) + new Vector3(5f, 0f, 5f);
    }
    private void SetRandomPlayerTarget()
    {
        int index = Random.Range(0, GameManager.Instance.players.Count);

        playerTransform = GameManager.Instance.players[index].pController.transform;
    }
    #endregion

    #region Object Pooling

    private void InitializePools()
    {
        enemyPools = new Dictionary<GameObject, EnemyObjectPool>();
        bossPools = new Dictionary<GameObject, EnemyObjectPool>();

        GameObject poolParent = new GameObject("Pool_Enemies");
        poolParent.transform.SetParent(transform, false);

        // Initialize enemy pools
        foreach (var enemyType in enemyTypes)
        {
            GameObject enemyPrefab = enemyType.EnemyToSpawn;
            if (!enemyPools.ContainsKey(enemyPrefab))
            {
                enemyPools.Add(
                    enemyPrefab,
                    new EnemyObjectPool(enemyPrefab, poolParent.transform, initialPoolSize)
                );
            }
        }

        // Initialize boss pools
        foreach (var bossPrefab in bossPrefabs)
        {
            if (!bossPools.ContainsKey(bossPrefab))
            {
                bossPools.Add(
                    bossPrefab,
                    new EnemyObjectPool(bossPrefab, poolParent.transform, 2) // Usually only need a few bosses
                );
            }
        }
    }

    #endregion

    #region Public Methods
    public void PauseWaves()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
    public void ResumeWaves()
    {
        if (currentState == WaveState.InProgress && !waveSpawningComplete)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemiesForWave());
        }
    }
    public void KillAllEnemies()
    {
        // Kill all active enemies
        foreach (BaseEnemy enemy in activeEnemies.ToList())
        {
            enemy.Die();
        }

        // Clear active enemies
        activeEnemies.Clear();

        // Reset enemy count
        enemiesAlive = 0;
        enemiesSpawned = 0;

        // If in boss fight, kill boss too
        if (isBossActive && activeBoss != null)
        {
            activeBoss.Die();
        }
    }
    // Method to adjust downtime duration at runtime
    public void SetDowntimeDuration(float duration)
    {
        waveDowntimeDuration = Mathf.Max(0f, duration);
    }

    // Method to toggle downtime feature
    public void ToggleDowntime(bool enable)
    {
        enableWaveDowntime = enable;
    }
    #endregion
}

