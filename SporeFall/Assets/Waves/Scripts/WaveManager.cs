using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private WaveUI wUI;


    [Header("References")]
    [SerializeField] private GameObject bossPrefab; // Boss enemy prefab for the final wave
    public TrainHandler train; // Reference to the player transform for positioning
    public Transform[] payloadPath;
    [HideInInspector]
    public List<GameObject> roberts = new();
    
    [Header("Waves")]
    public List<Wave> waves = new();
    private Wave currentWave;
    public Wave CurrentWave {  get { return currentWave; } }
    private FinalWaveSettings finalWave; // Reference for when we're in the final wave
    public int currentWaveIndex = 0;
    [SerializeField] private int maxEnemiesOnField = 300;

    [Header("Wave State")]
    private bool isBossDefeated = false;
    private bool isHordeSpawned = false;
    public bool isRobertSpawned = false;
    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    public int deadEnemies = 0;
    public enum WavePhase
    {
        NotStarted,
        Started,
        Departing,
        Moving
    }
    public WavePhase wavePhase;

    [Header("Train")]
    [SerializeField] private float trainDisembarkDelay = 15;

    [Header("Moving to Next Wave")]
    private bool skipped = false;
    public float departTime = 30;
    private float timer = 0;

    [Header("Object Pooling")]
    [SerializeField] private int initialPoolSize = 50;
    private Dictionary<GameObject, EnemyObjectPool> enemyPools;
    private EnemyObjectPool bossPool;

    public GameObject explosionPrefab;

    private void Start()
    {
        InitializePools();

        currentWaveIndex = 0;
        currentWave = waves[currentWaveIndex];
        StartCoroutine(MoveToWaveLocation(0));
    }
    #region Wave State Transitions
    public void OnStartWave()
    {
        if (currentWave.isFinalWave)
        {
            StartFinalWave();
        }
        else
        {
            StartCoroutine(StartWave());
        }
    }
    private IEnumerator StartWave()
    {
        wavePhase = WavePhase.Started;
        train.ToggleForceField(true);
        wUI.DisplayWaveStart();
        enemiesSpawned = 0;

        // Reset spawn counts for new wave
        foreach (var spawnData in currentWave.enemySpawnData)
        {
            spawnData.SpawnedCount = 0;
        }

        while (enemiesSpawned < currentWave.totalEnemies)
        {
            float spawnInterval = Random.Range(currentWave.minIntervalTime, currentWave.maxIntervalTime);
            if (enemiesAlive >= maxEnemiesOnField)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            int spawnCount = Random.Range(currentWave.minToSpawnAtOnce, currentWave.maxToSpawnAtOnce);
            for (int i = 0; i < spawnCount; i++)
            {
                if (enemiesSpawned < currentWave.totalEnemies && enemiesAlive < maxEnemiesOnField)
                {
                    Spawner();
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    private void StartFinalWave()
    {
        wavePhase = WavePhase.Started;
        train.ToggleForceField(true);

        wUI.DisplayWaveStart();
        Tutorial.Instance.StartFinalWaveTutorial();

        // Spawn boss and initial squad instead of payload
        isBossDefeated = false;
        isHordeSpawned = false;
        SpawnBossAndSquad();
    }
    private void WaveCleared()
    {  
        deadEnemies = 0;
        timer = departTime;
        wavePhase = WavePhase.Departing;
        Invoke(nameof(Depart), departTime);
    }
    private void Depart()
    {
        if(isRobertSpawned)
            return;
        train.ToggleForceField(false);
        StartCoroutine(MoveToWaveLocation(train.cannonFireTime));
    }
    #endregion
    public IEnumerator DestroyShroomPod(float waitTime)
    {
        yield return new WaitForSeconds(waitTime - 1);
        SpawnExplosion(currentWave.ShroomPod.transform.position);
        Destroy(currentWave.ShroomPod);
    }
    public void SkipDepartTime()
    {
        if(isRobertSpawned)
            RemoveAllRoberts();
        // since we are skipping, we no longer need to invoke, prevents repeat
        CancelInvoke(nameof(Depart));
        //player.RemoveButtonAction();
        Depart();
    }
    private IEnumerator MoveToWaveLocation(float waitTime)
    {

        if (waitTime > 0)
        {// At start wait time will be zero, and we dont want to do this stuff at start
            train.SetFiringState();
            // Destroy pod will spawn in an explosion so made it into Coroutine
            StartCoroutine(DestroyShroomPod(waitTime));
        }
        // animation wait time
        yield return new WaitForSeconds(waitTime);
        // at the start we have zero wait time, and don't want to go to next index
        if (waitTime > 0)
        {
            currentWaveIndex++;
            currentWave = waves[currentWaveIndex];
        }
        wavePhase = WavePhase.Moving;
        train.SetMovingState();

        Vector3 startPosition = train.transform.position;
        Vector3 targetPosition = currentWave.trainLocation.position;

        float time = 0f;
        while (time < 1f && !skipped)
        {
            time += Time.deltaTime * train.trainMoveSpeed;

            train.transform.position = Vector3.Lerp(startPosition, targetPosition, time);

            yield return null;
        }
        // animation wait time
        if (!skipped)
            train.animations.ParkTrain();

        yield return new WaitForSeconds(trainDisembarkDelay);

        if (!skipped)
        {
            // Ensuring the final position is set precisely after the movement
            train.transform.position = targetPosition;
            SetTrainParkState();
            if (currentWaveIndex >= 0)
                train.animations.OpenUpgradesPanel();
            wavePhase = WavePhase.NotStarted;
        }
    }
    public void SkippParkingAnimation()
    {
        if (train.trainState == TrainHandler.TrainState.Moving)
        {
            StopAllCoroutines();
            train.animations.SkipParkingAnimation();
            // Ensuring the final position is set precisely after the movement
            train.transform.position = currentWave.trainLocation.position;
            if (currentWaveIndex >= 0)
                train.animations.OpenUpgradesPanel();
            wavePhase = WavePhase.NotStarted;

            Invoke(nameof(SetTrainParkState), 2);
        }
    }
    private void SetTrainParkState()
    {
        train.SetParkedState();
    }
    public void SpawnExplosion(Vector3 pos)
    {
        Instantiate(explosionPrefab, pos, Quaternion.identity);
    }
    #region Enemy Spawning
    private void Spawner()
    {
        // Find an enemy type that hasn't reached its spawn limit
        var availableEnemies = currentWave.enemySpawnData
            .Where(data => data.SpawnedCount < data.totalToSpawn)
            .ToList();
        // if none found done spawning
        if (!availableEnemies.Any())
            return;
        // Randomly select from available enemies
        int enemyIndex = Random.Range(0, availableEnemies.Count);
        var selectedEnemy = availableEnemies[enemyIndex];

        Transform[] spawnPoints = currentWave.spawnLocations;
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        if (selectedEnemy.spawnAsGroup)
        {
            // Spawn as a group
            for (int i = 0; i < selectedEnemy.groupSize; i++)
            {
                if (selectedEnemy.SpawnedCount < selectedEnemy.totalToSpawn)
                {
                    Vector3 spawnPointOffset = new Vector3(spawnPoint.position.x + (i * 2), spawnPoint.position.y, spawnPoint.position.z + Random.Range(-1, 1));
                    Transform finalSpawnPoint = spawnPoint;
                    finalSpawnPoint.position = spawnPointOffset;
                    SpawnEnemy(selectedEnemy.EnemyToSpawn, finalSpawnPoint, spawnPointIndex);
                    selectedEnemy.SpawnedCount++; 
                }
            }
        }
        else
        {
            // Spawn single enemy
            SpawnEnemy(selectedEnemy.EnemyToSpawn, spawnPoint, spawnPointIndex);
            selectedEnemy.SpawnedCount++;
        }
    }
    private void InitializePools()
    {
        enemyPools = new Dictionary<GameObject, EnemyObjectPool>();
        GameObject poolParent = new GameObject($"Pool_Enemies");
        poolParent.transform.SetParent(transform);

        // Initialize pools for all enemy types across all waves
        foreach (Wave wave in waves)
        {
            foreach (var spawnData in wave.enemySpawnData)
            {

                foreach(var enemy in spawnData.enemyVariants)
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
        }

        // Initialize boss pool if boss prefab exists
        if (bossPrefab != null)
        {
            bossPool = new EnemyObjectPool(bossPrefab, poolParent.transform, 1); // Usually only need one boss
        }
    }
    public void SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint, int spawnIndex)
    {
        if (enemyPools == null)
            return;

        if (!enemyPools.TryGetValue(enemyPrefab, out EnemyObjectPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {enemyPrefab.name}");
            return;
        }

        BaseEnemy enemy = pool.Get(spawnPoint.position, spawnPoint.rotation);
        enemy.OnEnemyDeath += OnEnemyDeath;
        enemy.AssignDefaultTarget(train, train.transform);
        // enemy is spawning outside the pod, play rise from ground animation
        if(spawnIndex > 2)
            enemy.TriggerRiseAnimation();

        enemiesAlive++;
        enemiesSpawned++;
    }
    private void SpawnBossAndSquad()
    {
        // Spawn the boss
        Transform spawnPoint = currentWave.spawnLocations[0];
        BaseEnemy boss = bossPool.Get(spawnPoint.position, spawnPoint.rotation);
        boss.AssignDefaultTarget(train, GameManager.Instance.players[0].transform);
        boss.transform.SetParent(transform);

        boss.OnEnemyDeath += OnBossDeath;
        boss.AssignDefaultTarget(train, train.transform);

        enemiesAlive++;
        enemiesSpawned++;

        // Spawn the specific squad composition
        StartCoroutine(SpawnSquadSequence());
    }
    private IEnumerator SpawnSquadSequence()
    {
        var finalSettings = currentWave.finalWaveSettings;

        for (int typeIndex = 0; typeIndex < finalSettings.bossSquadComposition.Length; typeIndex++)
        {
            EnemySpawnData squadType = finalSettings.bossSquadComposition[typeIndex];
            int countToSpawn = finalSettings.bossSquadComposition[typeIndex].totalToSpawn;

            for (int i = 0; i < countToSpawn; i++)
            {
                int randomSpawnPoint = Random.Range(0, currentWave.spawnLocations.Length);
                Transform squadSpawnPoint = currentWave.spawnLocations[randomSpawnPoint];

                SpawnEnemy(squadType.EnemyToSpawn, squadSpawnPoint, randomSpawnPoint);

                yield return new WaitForSeconds(0.1f); // Prevent overlap
            }
        }
    }
    private IEnumerator SpawnPostBossHorde()
    {
        isHordeSpawned = true;
        var finalSettings = currentWave.finalWaveSettings;
        int hordeSpawned = 0;

        while (hordeSpawned < finalSettings.postBossHordeSize)
        {
            float spawnInterval = Random.Range(currentWave.minIntervalTime, currentWave.maxIntervalTime);
            if (enemiesAlive >= maxEnemiesOnField)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            // Pick a random enemy type from the horde configuration
            var randomEnemyData = finalSettings.hordeEnemyTypes[
                Random.Range(0, finalSettings.hordeEnemyTypes.Count)];

            int randomSpawnPoint = Random.Range(0, currentWave.spawnLocations.Length);
            Transform spawnPoint = currentWave.spawnLocations[randomSpawnPoint];

            if (randomEnemyData.spawnAsGroup)
            {
                // Spawn as a group
                for (int i = 0; i < randomEnemyData.groupSize; i++)
                {
                    if (randomEnemyData.SpawnedCount < randomEnemyData.totalToSpawn)
                    {
                        SpawnEnemy(randomEnemyData.EnemyToSpawn, spawnPoint, randomSpawnPoint);
                    }
                }
            }
            else
            {
                // Spawn single enemy
                SpawnEnemy(randomEnemyData.EnemyToSpawn, spawnPoint, randomSpawnPoint);
            }
            // horde is endless
            //hordeSpawned++;

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    private void OnBossDeath(BaseEnemy boss)
    {
        enemiesAlive--;
        isBossDefeated = true;

        bossPool.Return(boss);

        // Spawn payload after boss death
        train.SpawnPayload(payloadPath);
        //train.Payload.IncreaseSpeed();
        wUI.DisplayBossProgress();
        wUI.DisplayBossFlag();
        StartCoroutine(SpawnPostBossHorde());
    }
    private void OnEnemyDeath(BaseEnemy enemy)
    {
        enemiesAlive--;
        deadEnemies++;

        if (!(currentWave.isFinalWave))
        {
            wUI.DisplayWaveProgress(deadEnemies);
        }

        if (enemyPools.TryGetValue(enemy.gameObject, out EnemyObjectPool pool))
        {
            pool.Return(enemy);
        }

        // Check wave completion
        if (currentWave.isFinalWave)
        {
            if (isBossDefeated && isHordeSpawned && enemiesAlive <= 0)
            {
                wUI.DisplayWaveFlags();
                wUI.DisplayWaveClear();
                WaveCleared();
            }
        }
        else if (enemiesSpawned == currentWave.totalEnemies && enemiesAlive <= 0)
        {
            wUI.DisplayWaveFlags();
            wUI.DisplayWaveClear();
            WaveCleared();
        }
    }
    public void AddRobert(GameObject robert)
    {
        roberts.Add(robert);
        isRobertSpawned = true;
    }
    public void RemoveRobert(GameObject robert)
    {
        roberts.Remove(robert);

        if(roberts.Count <= 0)
        {
            isRobertSpawned = false;
        }
    }
    private void RemoveAllRoberts()
    {
        int x = roberts.Count;
        for(int i = 0; i < x; i++)
        {
            Destroy(roberts[i]);
        }

        roberts.Clear();
        isRobertSpawned = false;
    }
    #endregion


}
