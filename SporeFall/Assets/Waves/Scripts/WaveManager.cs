using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager player;
    public GameObject bossPrefab; // Boss enemy prefab for the final wave
    public TrainHandler train; // Reference to the player transform for positioning
    
    [Header("Waves")]
    public List<Wave> waves = new(); // List of waves to configure
    private Wave currentWave;
    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private bool waveActive = false;
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
    [Header("UI Stuff")]
    // test Ui
    public TMP_Text waveUI;
    public TMP_Text bossText;
    // Test particles
    public GameObject explosionPrefab;
    private void Start()
    {
        currentWave = waves[currentWaveIndex];
        StartCoroutine(MoveToWaveLocation(0));
    }

    private void Update()
    {
        switch (wavePhase)
        {
            case WavePhase.NotStarted:
                waveUI.text = "Push Button to Start: " + currentWave.waveName;
                break;
            case WavePhase.Started:
                waveUI.text = "Enemies Left: " + enemiesAlive.ToString();
                break;
            case WavePhase.Departing:
                timer -= Time.deltaTime;
                waveUI.text = "Wave Cleared! Departing in: " + (timer).ToString("F0") + "\n Push Button To skip Wait";
                break;
            case WavePhase.Moving:
                waveUI.text = "Moving to next Area";
                break;
        }
    }
    private IEnumerator StartWave()
    {
        wavePhase = WavePhase.Started;
        waveActive = true;
        enemiesSpawned = 0;
        enemiesAlive = currentWave.totalEnemies;

        while (enemiesSpawned < currentWave.totalEnemies)
        {
            int spawnCount = Random.Range(currentWave.minIntervalSpawn, currentWave.maxIntervalSpawn);
            for (int i = 0; i < spawnCount; i++)
            {
                if (enemiesSpawned < currentWave.totalEnemies)
                {
                    SpawnEnemy();
                    enemiesSpawned++;
                }
            }
            float spawnInterval = Random.Range(currentWave.minIntervalTime,currentWave.maxIntervalTime);
            yield return new WaitForSeconds(spawnInterval);   
        }
    }
    private void StartFinalWave()
    {
        waveActive = true;

        // Spawn the payload
        train.SpawnPayload(currentWave.spawnLocations[0].position);
        
        Invoke(nameof(SpawnBoss), 2f);
    }
    private void SpawnEnemy()
    {
        // choose at random from a curated selection of enemies to spawn in
        int enemyIndex = Random.Range(0, currentWave.enemyPrefabs.Length);
        GameObject enemyToSpawn = currentWave.enemyPrefabs[enemyIndex];

        Transform[] spawnPoints =  currentWave.spawnLocations;
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        GameObject enemy = Instantiate(enemyToSpawn, spawnPoint.position, spawnPoint.rotation);
        // once we start the enemy script add an OnEnemyDeath Event
        enemy.GetComponent<Sherman>().OnEnemyDeath += OnEnemyDeath; // Assuming each enemy has an OnEnemyDeath event
    }
    private void SpawnBoss()
    {
        Transform spawnPoint = currentWave.spawnLocations[0];
        GameObject boss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);

        enemiesAlive++;
        bossText.text = "<color=red>Boss Has Spawned</color>" + "\n 999999";
        // once we start the Boss script add an OnEnemyDeath Event
        boss.GetComponent<Sherman>().OnEnemyDeath += OnBossDeath;
    }
    private void WaveCleared()
    {
        Debug.Log("Wave Cleared!");
        waveActive = false;

        timer = departTime;
        wavePhase = WavePhase.Departing;
        Invoke(nameof(Depart), departTime);
    }
    private void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0 || !waveActive)
        {
           WaveCleared();
        }
    }
    private void OnBossDeath()
    {
        enemiesAlive--;

        if (enemiesAlive <= 0 && waveActive)
        {
            Debug.Log("Boss defeated!");
            WaveCleared();
            // speed up payload
            train.Payload.IncreaseSpeed();
        }
    }
    private void Depart()
    {
        StartCoroutine(MoveToWaveLocation(train.cannonFireTime));
    }
    private IEnumerator DestroyShroomPod(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SpawnExplosion(currentWave.ShroomPod.transform.position);
        Destroy(currentWave.ShroomPod);
    }
    public void SkipDepartTime()
    {
        // since we are skipping, we no longer need to invoke, prevents repeat
        CancelInvoke(nameof(Depart));
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
        // switch to train camera
        player.DisableControl();
        train.SetFiringState();
        // Destroy pod will spawn in an explosion so made it into Coroutine
        if (waitTime > 0)
           StartCoroutine(DestroyShroomPod(waitTime));

        yield return new WaitForSeconds(waitTime + .2f);
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
        player.EnableControl();
        wavePhase = WavePhase.NotStarted;
    }
    private void SpawnExplosion(Vector3 pos)
    {
        Instantiate(explosionPrefab, pos, Quaternion.identity);
    }
}
