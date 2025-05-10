using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndlessWaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> bossPrefabs; // Multiple boss prefabs for variety

    [Header("Wave Settings")]
    [Range(0, 1)]
    [SerializeField] private float outsideSpawnChance = 0.45f;
    [SerializeField] private int maxEnemiesOnField = 300;
    [SerializeField] private float baseDifficulty = 1.0f;
    [SerializeField] private float difficultyIncreasePerWave = 0.1f;
    [SerializeField] private float difficultyIncreaseOverTime = 0.05f;
    [SerializeField] private float difficultyTimeInterval = 60f; // Increase difficulty every minute

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

    [Header("Spawn Zones")]
    [SerializeField] private Transform[] presetSpawnPoints;
    [SerializeField] private Collider outsideSpawnZone;

    // Wave State
    private float currentDifficulty;
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

    // Object Pooling
    [SerializeField] private int initialPoolSize = 50;
    private Dictionary<GameObject, EnemyObjectPool> enemyPools;
    private Dictionary<GameObject, EnemyObjectPool> bossPools;
    public GameObject explosionPrefab;

    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();
    private Coroutine spawnCoroutine;

    public enum WaveState
    {
        NotStarted,
        InProgress,
        BossFight
    }

    public WaveState currentState = WaveState.NotStarted;

    private void Start()
    {
        InitializePools();
        ResetWaveParameters();

        // Set initial difficulty
        currentDifficulty = baseDifficulty;
        bossSpawnTimer = timeBetweenBossSpawns;

        Invoke(nameof(StartEndlessWaves), 5);
    }

    private void Update()
    {
        if (currentState != WaveState.NotStarted)
        {
            // Update boss spawn timer
            if (!isBossActive)
            {
                bossSpawnTimer -= Time.deltaTime;
                if (bossSpawnTimer <= 0)
                {
                    SpawnBoss();
                    ResetBossSpawnTimer();
                }
            }

            // Update difficulty over time
            difficultyTimer += Time.deltaTime;
            if (difficultyTimer >= difficultyTimeInterval)
            {
                IncreaseDifficultyOverTime();
                difficultyTimer = 0;
            }

            // Update UI
            //waveUI.DisplayWaveProgress(deadEnemies);
        }
    }

    #region Wave Management

    public void StartEndlessWaves()
    {
        if (currentState == WaveState.NotStarted)
        {
            currentState = WaveState.InProgress;
            currentWaveNumber = 1;

            deadEnemies = 0;
            //train.ToggleForceField(true);
            //waveUI.DisplayWaveStart();

            spawnCoroutine = StartCoroutine(SpawnEnemiesEndlessly());
        }
    }

    private void ResetWaveParameters()
    {
        enemySpawnInterval = initialSpawnInterval;
        maxEnemiesPerSpawnCurrent = initialMaxEnemiesPerSpawn;
        totalSpawnCountForCurrentWave = initialSpawnCount;

        // Reset spawn counts for all enemy types
        foreach (var enemyType in enemyTypes)
        {
            enemyType.SpawnedCount = 0;
        }
    }

    private void NextWave()
    {
        currentWaveNumber++;
        currentDifficulty += difficultyIncreasePerWave;

        // Increase spawn rate and count as difficulty increases
        enemySpawnInterval = Mathf.Max(minSpawnInterval, initialSpawnInterval / currentDifficulty);
        maxEnemiesPerSpawnCurrent = Mathf.Min(maxEnemiesPerSpawn,
            initialMaxEnemiesPerSpawn + Mathf.FloorToInt(currentDifficulty / 2));
        totalSpawnCountForCurrentWave = initialSpawnCount + (currentWaveNumber * 10);

        // Update UI for new wave
        deadEnemies = 0;
        //waveUI.DisplayWaveStart();

        // Reset enemy type spawn counts
        foreach (var enemyType in enemyTypes)
        {
            enemyType.SpawnedCount = 0;
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

    #endregion

    #region Enemy Spawning

    private IEnumerator SpawnEnemiesEndlessly()
    {
        while (currentState == WaveState.InProgress)
        {
            if (enemiesAlive < maxEnemiesOnField)
            {
                int spawnCount = Random.Range(1, maxEnemiesPerSpawnCurrent + 1);
                for (int i = 0; i < spawnCount; i++)
                {
                    if (enemiesAlive < maxEnemiesOnField)
                    {
                        SpawnRandomEnemy();
                    }
                }
            }

            yield return new WaitForSeconds(enemySpawnInterval);
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
        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
        Vector3 spawnPosition = presetSpawnPoints[0].position; // Spawn boss at first preset point

        // Get boss from object pool
        BaseEnemy boss = bossPools[bossPrefab].Get(spawnPosition, Quaternion.identity);
        //boss.AssignTrain(train);
        boss.OnEnemyDeath += OnBossDeath;

        // Scale boss health based on number of bosses defeated
        float healthMultiplier = 1f + (bossHealthMultiplier * bossesDefeated);
        boss.SetHealthMultiplier(healthMultiplier);

        activeBoss = boss;
        enemiesAlive++;

        // Display boss UI
        //waveUI.DisplayBossStart();

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
            int spawnPointIndex = Random.Range(1, 4); // Use points 1-3 for squad

            SpawnEnemy(
                squadMember.EnemyToSpawn,
                presetSpawnPoints[spawnPointIndex].position,
                false
            );

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPoint, bool spawningOutside)
    {
        if (enemyPools == null) return;

        if (!enemyPools.TryGetValue(enemyPrefab, out EnemyObjectPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {enemyPrefab.name}");
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(-Vector3.forward);

        BaseEnemy enemy = pool.Get(spawnPoint, rotation);
        //enemy.AssignTrain(train);
        enemy.OnEnemyDeath += OnEnemyDeath;

        // Scale enemy health based on difficulty
        enemy.SetHealthMultiplier(Mathf.Sqrt(currentDifficulty));

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

        // Update UI
        //waveUI.DisplayWaveProgress(deadEnemies);

        if (enemyPools.TryGetValue(enemy.gameObject, out EnemyObjectPool pool))
        {
            pool.Return(enemy);
        }

        activeEnemies.Remove(enemy);

        // Check if we've reached the spawn count for this wave
        if (!isBossActive && deadEnemies >= totalSpawnCountForCurrentWave)
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

        // Update UI
        //waveUI.DisplayBossDefeated();

        // Reset to normal wave
        currentState = WaveState.InProgress;
        ResetBossSpawnTimer();

        // Increase difficulty after boss is defeated
        currentDifficulty += difficultyIncreasePerWave * 2;

        // Increase wave number
        NextWave();
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

            // Check if point is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                if (!Physics.CheckSphere(hit.position, 0.5f, LayerMask.GetMask("Obstacle")))
                {
                    return hit.position;
                }
            }
        }

        // Fallback if no valid position found
        int index = Random.Range(4, presetSpawnPoints.Length);
        return presetSpawnPoints[index].position;
    }

    #endregion

    #region Object Pooling

    private void InitializePools()
    {

        enemyPools = new Dictionary<GameObject, EnemyObjectPool>();
        bossPools = new Dictionary<GameObject, EnemyObjectPool>();
        GameObject poolParent = new GameObject($"Pool_Enemies");
        poolParent.transform.SetParent(transform, false);

        // Initialize pools for all enemy types across all waves
        foreach (var enemyType in enemyTypes)
        {
            foreach (var enemy in enemyType.enemyVariants)
            {
                if (!enemyPools.ContainsKey(enemy))
                {
                    enemyPools.Add(
                        enemy,
                        new EnemyObjectPool(enemy, poolParent.transform, initialPoolSize)
                    );
                }
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

    public void SpawnExplosion(Vector3 position)
    {
        Instantiate(explosionPrefab, position, Quaternion.identity);
    }

    public void PauseWaves()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    public void ResumeWaves()
    {
        if (currentState == WaveState.InProgress)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemiesEndlessly());
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

    #endregion
}
