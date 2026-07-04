using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveManagerNew : MonoBehaviour
{
    [Header("Wave Data")]
    public List<WaveDefinition> waves;
    [Tooltip("If true, generates endless waves beyond the defined list.")]
    public bool endlessMode = true;

    [Header("Difficulty Scaling (endless)")]
    [Tooltip("Base enemy level for wave 1.")]
    public int baseLevelWave1 = 1;
    [Tooltip("Extra levels added per wave beyond the defined list.")]
    public int levelIncreasePerWave = 1;
    [Tooltip("How many extra enemies to add per wave in endless mode.")]
    public int enemyCountIncreasePerWave = 2;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 10f;

    // ── runtime ──────────────────────────────────────────────────────────
    private int _currentWave = 0;
    private int _aliveCount = 0;
    private bool _waveInProgress = false;

    public event System.Action<int> OnWaveStarted;
    public event System.Action<int> OnWaveCleared;

    private void Start() => StartCoroutine(RunWaves());

    private IEnumerator RunWaves()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            WaveDefinition waveDef = BuildWaveDefinition(_currentWave);
            yield return StartCoroutine(SpawnWave(waveDef, _currentWave));

            // Wait until all enemies are dead
            yield return new WaitUntil(() => _aliveCount <= 0);

            OnWaveCleared?.Invoke(_currentWave);
            _currentWave++;

            if (!endlessMode && _currentWave >= waves.Count) break;
        }
    }

    private IEnumerator SpawnWave(WaveDefinition def, int waveIndex)
    {
        _waveInProgress = true;
        OnWaveStarted?.Invoke(waveIndex);

        foreach (var group in def.groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                //SpawnEnemy(group.enemyPrefab, group.level, );
                yield return new WaitForSeconds(group.spawnInterval);
            }

            yield return new WaitForSeconds(group.delayAfterGroup);
        }

        _waveInProgress = false;
    }

    public void SpawnEnemy(GameObject enemyPrefab, int level, Vector3 spawnPoint, bool spawningOutside)
    {

        if (!PoolManager.Instance.enemyPool.TryGetValue(enemyPrefab, out EnemyObjectPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {enemyPrefab.name}");
            return;
        }
        Quaternion rotation = Quaternion.LookRotation(-Vector3.forward); // face backwards from world forward

        EnemyController enemy = pool.Get(spawnPoint, rotation);
        

        // enemy is spawning outside the pod, play rise from ground animation
        if (spawningOutside)
            enemy.EnemyAnimator.spawnInGround = true;

        enemy.Initialize(level);
        enemy.OnDied += HandleEnemyDied;
        _aliveCount++;
    }

    private void HandleEnemyDied(EnemyController enemy)
    {
        enemy.OnDied -= HandleEnemyDied;
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
    }

    protected Vector3 GetSpawnPointWithinZone(WaveDefinition def)
    {
        Bounds zoneBounds = def.outSideSpawnZone.bounds;

        // Maximum attempts to find a valid position
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate a random position within the zone bounds
            float offsetX = Random.Range(-zoneBounds.extents.x, zoneBounds.extents.x);
            float offsetZ = Random.Range(-zoneBounds.extents.z, zoneBounds.extents.z);
            Vector3 randomPoint = zoneBounds.center + new Vector3(offsetX, 0, offsetZ);

            // Check if the point is on the NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // Check if the position is clear of obstacles
                if (!Physics.CheckSphere(hit.position, 0.5f, LayerMask.GetMask("Obstacle")))
                {
                    return hit.position;
                }
            }
        }

        // Fallback if no valid position found after max attempts
        Debug.LogWarning("Could not find valid spawn position within " + maxAttempts + " attempts");
        int index = Random.Range(4, def.presetSpawnPoints.Length);
        return def.presetSpawnPoints[index].position;
    }

    // endless
    private WaveDefinition BuildWaveDefinition(int waveIndex)
    {
        // Use authored wave if available
        if (waveIndex < waves.Count)
            return waves[waveIndex];

        // Generate procedural wave for endless mode
        int extraWaves = waveIndex - waves.Count;
        int level = baseLevelWave1 + (waveIndex * levelIncreasePerWave);
        int count = 10 + (extraWaves * enemyCountIncreasePerWave);

        // Reuse the last authored prefab pool as a source
        WaveDefinition last = waves[waves.Count - 1];
        var newDef = new WaveDefinition();
        newDef.groups = new List<SpawnGroup>();

        foreach (var g in last.groups)
        {
            newDef.groups.Add(new SpawnGroup
            {
                enemyPrefab = g.enemyPrefab,
                count = count / last.groups.Count,
                level = level,
                spawnInterval = Mathf.Max(0.1f, g.spawnInterval - extraWaves * 0.02f),
                delayAfterGroup = g.delayAfterGroup
            });
        }

        return newDef;
    }
}
