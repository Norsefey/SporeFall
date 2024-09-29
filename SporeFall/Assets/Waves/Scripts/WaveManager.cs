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
    public float trainMoveSpeed = 5f; // Speed of the smooth movement to wave location
    public float departTime = 30;
    private float timer = 0;
    [Header("UI Stuff")]
    // test Ui
    public TMP_Text waveUI;
    public TMP_Text bossText;
    private void Start()
    {
        currentWave = waves[currentWaveIndex];
        StartCoroutine(MoveToWaveLocation());
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
        
        Invoke("SpawnBoss", 2f);
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
        boss.GetComponent<Sherman>().OnEnemyDeath += OnEnemyDeath;
    }
    private void WaveCleared()
    {
        Debug.Log("Wave Cleared!");
        waveActive = false;

        timer = departTime;
        wavePhase = WavePhase.Departing;
        Invoke("Depart", departTime);
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
            // speed up payload
        }
    }
    private void Depart()
    {
        Destroy(currentWave.ShroomPod);
        currentWaveIndex++;
        currentWave = waves[currentWaveIndex];
        StartCoroutine(MoveToWaveLocation());
    }
    public void SkipDepartTime()
    {
        CancelInvoke("Depart");
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
    private IEnumerator MoveToWaveLocation()
    {
        wavePhase = WavePhase.Moving;
        player.DisableControl();
        train.SetMovingState();
        Vector3 startPosition = train.transform.position;
        Vector3 targetPosition = currentWave.trainLocation.position;

       /* Vector3 cameraStartPosition = cameraTransform.position;
        Vector3 cameraTargetPosition = wave.waveLocation.position + new Vector3(0, 10, -10); // Adjust camera offset
*/
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * trainMoveSpeed;

            train.transform.position = Vector3.Lerp(startPosition, targetPosition, time);
            //cameraTransform.position = Vector3.Lerp(cameraStartPosition, cameraTargetPosition, time);

            yield return null;
        }

        // Ensuring the final position is set precisely after the movement
        train.transform.position = targetPosition;
        train.SetParkedState();
        player.EnableControl();
        wavePhase = WavePhase.NotStarted;

        //cameraTransform.position = cameraTargetPosition;
    }

}
