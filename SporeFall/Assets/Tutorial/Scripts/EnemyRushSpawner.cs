using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRushSpawner : MonoBehaviour
{// No longer in use
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private Wave spawnSettings = new();

    [SerializeField] private int maxEnemiesOnField = 50;
    [SerializeField] private TrainHandler train; // Reference to the train for enemy targeting

    private List<GameObject> currentEnemies = new List<GameObject>();
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private Transform player;

    private IEnumerator SpawnEnemiesRoutine()
    {
        // Wait initial delay before starting spawns
        yield return new WaitForSeconds(initialSpawnDelay);

        // Continue spawning until all enemies are spawned
        while (enemiesSpawned < spawnSettings.totalEnemies)
        {
            // Clean up any destroyed enemies from the list
            currentEnemies.RemoveAll(enemy => enemy == null);

            // Only spawn if we're below max enemies
            if (enemiesAlive < maxEnemiesOnField)
            {
                yield return StartCoroutine(SpawnWaveEnemies());
            }

            // Random interval between spawns
            float spawnInterval = Random.Range(spawnSettings.minIntervalTime, spawnSettings.maxIntervalTime);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SpawnWaveEnemies()
    {
        // Determine how many enemies to spawn this interval
        int enemiesToSpawn = Random.Range(spawnSettings.minToSpawnAtOnce, spawnSettings.maxToSpawnAtOnce + 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // If we've reached max enemies or total enemies to spawn, stop
            if (enemiesAlive >= maxEnemiesOnField || enemiesSpawned >= spawnSettings.totalEnemies)
                break;

            // Select an enemy type to spawn
            EnemySpawnData enemyToSpawn = SelectEnemyToSpawn();

            if (enemyToSpawn != null)
            {
                // Select a spawn location
                Transform spawnLocation = spawnSettings.spawnLocations[Random.Range(0, spawnSettings.spawnLocations.Length)];

                // Spawn the enemy
                if (enemyToSpawn.spawnAsGroup)
                {
                    // Spawn group
                    for (int j = 0; j < enemyToSpawn.groupSize; j++)
                    {
                        if (enemiesAlive >= maxEnemiesOnField || enemiesSpawned >= spawnSettings.totalEnemies)
                            break;

                        SpawnSingleEnemy(enemyToSpawn, spawnLocation);
                    }
                }
                else
                {
                    // Spawn single enemy
                    SpawnSingleEnemy(enemyToSpawn, spawnLocation);
                }
            }
        }

        yield return null;
    }

    private void SpawnSingleEnemy(EnemySpawnData enemyToSpawn, Transform spawnLocation)
    {
        // Get the enemy prefab to spawn
        GameObject enemyPrefab = enemyToSpawn.EnemyToSpawn;

        // Calculate group spawn offset
        Vector3 spawnPosition = spawnLocation.position;
        if (enemyToSpawn.spawnAsGroup)
        {
            Vector3 groupOffset = Random.insideUnitSphere * 2f;
            groupOffset.y = 0; // Keep on same plane
            spawnPosition += groupOffset;
        }

        // Instantiate the enemy
        GameObject spawnedEnemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Try to get BaseEnemy component
        BaseEnemy enemy = spawnedEnemyObject.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            // Assign default target (train)
            enemy.AssignDefaultTarget(train, player);

            // Subscribe to death event
            enemy.OnEnemyDeath += OnEnemyDeath;
        }
        spawnedEnemyObject.SetActive(true);
        // Track enemy
        currentEnemies.Add(spawnedEnemyObject);
        enemiesSpawned++;
        enemiesAlive++;

        // Update spawn count for this enemy type
        enemyToSpawn.SpawnedCount++;
    }

    private void OnEnemyDeath(BaseEnemy deadEnemy)
    {
        enemiesAlive--;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            StartCoroutine(SpawnEnemiesRoutine());
        }
    }
    private EnemySpawnData SelectEnemyToSpawn()
    {
        // Filter out enemy types that have reached their spawn limit
        List<EnemySpawnData> availableEnemies = spawnSettings.enemySpawnData
            .FindAll(data => data.SpawnedCount < data.totalToSpawn);

        // If no enemies available, return null
        if (availableEnemies.Count == 0)
            return null;

        // Randomly select from available enemies
        return availableEnemies[Random.Range(0, availableEnemies.Count)];
    }
}
