using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveUI wUI;
    public PauseMenu pauseMenu;
    public bool paused = false;

    [Header("References")]
    [SerializeField] private GameObject bossPrefab; // Boss enemy prefab for the final wave
    public TrainHandler train; // Reference to the player transform for positioning
    public Transform[] payloadPath;
    
    [Header("Waves")]
    public List<Wave> waves = new(); // List of waves to configure
    private Wave currentWave;
    public int currentWaveIndex = 0;
    [SerializeField] private int maxEnemiesOnField = 300;

    private int enemiesAlive = 0; // how many active enemies
    private int enemiesSpawned = 0;// total amount of enemies spawned so far
    public int deadEnemies = 0; // enemies killed
    public enum WavePhase
    {
        NotStarted,
        Started,
        Departing,
        Moving
    }
    public WavePhase wavePhase;

    [Header("Moving to Next Wave")]
    public float departTime = 30;
    private float timer = 0;

    // Move UI To own Script
    [Header("UI Stuff")]
    // test Ui
    public TMP_Text waveUI;
    public TMP_Text bossText;
    // Test particles
    public GameObject explosionPrefab;

    private void Start()
    {
        Debug.Log("WaveMan Is awake");

        currentWaveIndex = 0;

        currentWave = waves[currentWaveIndex];
        StartCoroutine(MoveToWaveLocation(0));
    }

    private void Update()
    {
        switch (wavePhase)
        {
            case WavePhase.NotStarted:
                if(waveUI != null)
                    waveUI.text = "Push Button to Start: " + currentWave.waveName;
                break;
            case WavePhase.Started:
                if (waveUI != null)
                    waveUI.text = "Enemies Left: " + (currentWave.totalEnemies - ( enemiesSpawned - enemiesAlive)).ToString();
                break;
            case WavePhase.Departing:
                timer -= Time.deltaTime;
                if (waveUI != null && timer > 0)
                    waveUI.text = "Wave Cleared! Departing in: " + (timer).ToString("F0") + "\n Push Button To skip Wait";
                else if(waveUI != null)
                    waveUI.text = "Firing Cannon!!";
                break;
            case WavePhase.Moving:
                if (waveUI != null)
                    waveUI.text = "Moving to next Area";
                break;
        }
    }
    private IEnumerator StartWave()
    {
        wavePhase = WavePhase.Started;
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

            int spawnCount = Random.Range(currentWave.minIntervalSpawn, currentWave.maxIntervalSpawn);
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
        // Spawn the payload
        train.SpawnPayload(payloadPath);
        
        Invoke(nameof(SpawnBoss), 2f);
    }
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
                    SpawnEnemy(selectedEnemy.enemyPrefab, spawnPoint);
                    selectedEnemy.SpawnedCount++;
                }
            }
        }
        else
        {
            // Spawn single enemy
            SpawnEnemy(selectedEnemy.enemyPrefab, spawnPoint);
            selectedEnemy.SpawnedCount++;
        }
    }
    // Helper method to spawn a single enemy
    private void SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        BaseEnemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation)
            .GetComponent<BaseEnemy>();
        enemy.transform.SetParent(transform);
        enemy.OnEnemyDeath += OnEnemyDeath;
        enemy.AssignDefaultTarget(train, train.transform);

        enemiesAlive++;
        enemiesSpawned++;
    }
    private void SpawnBoss()
    {
        Debug.Log("Add Functionality");
        Transform spawnPoint = currentWave.spawnLocations[0];
        BaseEnemy boss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<BaseEnemy>();
        
        boss.transform.SetParent(transform);
       
        if (bossText != null)
            bossText.text = "<color=red>Boss Has Spawned</color>";
        // once we start the Boss script add an OnEnemyDeath Event
        boss.OnEnemyDeath += OnBossDeath;
        boss.AssignDefaultTarget(train, train.Payload.transform);

        enemiesAlive++;
        enemiesSpawned++;
    }
    private void WaveCleared()
    {
        Debug.Log("Wave Cleared!");
        deadEnemies = 0;
        timer = departTime;
        wavePhase = WavePhase.Departing;
        Invoke(nameof(Depart), departTime);
    }
    private void OnEnemyDeath()
    {
        enemiesAlive--;
        deadEnemies++;
        if (currentWave.isFinalWave == false)
        {
            wUI.DisplayWaveProgress(deadEnemies);
        }
        
        if (enemiesSpawned == currentWave.totalEnemies && enemiesAlive <= 0)
        {
           WaveCleared();
        }
    }
    private void OnBossDeath()
    {
        enemiesAlive--;
        // speed up payload
        train.Payload.IncreaseSpeed();
        if (enemiesSpawned == currentWave.totalEnemies && enemiesAlive <= 0)
        {
            Debug.Log("Boss defeated!");
            WaveCleared();
        }
    }
    private void Depart()
    {
        StartCoroutine(MoveToWaveLocation(train.cannonFireTime));
    }
    public IEnumerator DestroyShroomPod(float waitTime)
    {
        yield return new WaitForSeconds(waitTime - 1);
        SpawnExplosion(currentWave.ShroomPod.transform.position);
        Destroy(currentWave.ShroomPod);
    }
    public void SkipDepartTime()
    {
        // since we are skipping, we no longer need to invoke, prevents repeat
        CancelInvoke(nameof(Depart));
        //player.RemoveButtonAction();
        Depart();
    }
    public void OnStartWave()
    {
        if (currentWave.isFinalWave)
        {
            StartFinalWave();
            StartCoroutine(StartWave());
        }
        else
        {
            StartCoroutine(StartWave());
        }
    }
    private IEnumerator MoveToWaveLocation(float waitTime)
    {
        if (waitTime > 0)
        {// At start wait time will be zero, and we dont want to do this stuff at start
            train.SetFiringState();
            // Destroy pod will spawn in an explosion so made it into Coroutine
            StartCoroutine(DestroyShroomPod(waitTime));
        }

        yield return new WaitForSeconds(waitTime);
        Debug.Log("Moving Train");

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
        while (time < 1f)
        {
            time += Time.deltaTime * train.trainMoveSpeed;

            train.transform.position = Vector3.Lerp(startPosition, targetPosition, time);

            yield return null;
        }

        // Ensuring the final position is set precisely after the movement
        train.transform.position = targetPosition;
        train.SetParkedState();
        
        wavePhase = WavePhase.NotStarted;
    }
    public void SpawnExplosion(Vector3 pos)
    {
        Instantiate(explosionPrefab, pos, Quaternion.identity);
    }
}
