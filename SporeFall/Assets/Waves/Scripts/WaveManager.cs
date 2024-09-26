using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public List<Wave> waves = new List<Wave>(); // List of waves to configure
    public Transform[] spawnPoints; // Points where enemies will spawn

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private bool waveActive = false;

    public GameObject bossPrefab; // Boss enemy prefab for the final wave
    // Event to let other scripts know that the wave has been cleared
    public delegate void OnWaveCleared();
    public event OnWaveCleared WaveCleared;

    // test Ui
    public TMP_Text waveUI;
    public TMP_Text bossText;
    private void Start()
    {
        // for testing purpose start wave when game begins
        StartNextWave();
    }

    private void Update()
    {
        // for testing purposes
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentWaveIndex++;
            StartNextWave();
        }

        /*// If there are no enemies left, wave is cleared
        if (enemiesAlive <= 0 && waveActive)
        {
            WaveCleared?.Invoke();
            waveActive = false;
        }*/
    }
    public void StartNextWave()
    {// will be called by player when they are ready to start the next wave
        if (currentWaveIndex < waves.Count)
        {
            StartCoroutine(StartWave(waves[currentWaveIndex]));
        }
        else
        {
            Debug.Log("All waves completed!");
        }
    }

    private IEnumerator StartWave(Wave wave)
    {
        waveUI.text = wave.waveName +" Started!";

        waveActive = true;
        enemiesSpawned = 0;
        enemiesAlive = wave.totalEnemies;

        while (enemiesSpawned < wave.totalEnemies)
        {
            int spawnCount = Random.Range(wave.minIntervalSpawn, wave.maxIntervalSpawn);
            for (int i = 0; i < spawnCount; i++)
            {
                if (enemiesSpawned < wave.totalEnemies)
                {
                    SpawnEnemy(wave);
                    enemiesSpawned++;
                }
            }
            float spawnInterval = Random.Range(wave.minIntervalTime,wave.maxIntervalTime);
            yield return new WaitForSeconds(spawnInterval);   
        }
        // will do something different for spawning in boss on final Wave
        if (wave.isFinalWave)
        {
            SpawnBoss();
            // spawn Payload
        }
    }
    private void SpawnEnemy(Wave wave)
    {
        // choose at random from a curated selection of enemies to spawn in
        int enemyIndex = Random.Range(0, wave.enemyPrefabs.Length);
        GameObject enemyToSpawn = wave.enemyPrefabs[enemyIndex];

        // Pick a random spawn point
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        GameObject enemy = Instantiate(enemyToSpawn, spawnPoint.position, spawnPoint.rotation);
        // once we start the enemy script add an OnEnemyDeath Event
        enemy.GetComponent<Sherman>().OnEnemyDeath += OnEnemyDeath; // Assuming each enemy has an OnEnemyDeath event
    }
    private void SpawnBoss()
    {
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];
        GameObject boss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;
        bossText.text = "<color=red>Boss Has Spawned</color>" + "\n 999999";
        // once we start the Boss script add an OnEnemyDeath Event
        boss.GetComponent<Sherman>().OnEnemyDeath += OnEnemyDeath;
    }
    private void OnEnemyDeath()
    {
        enemiesAlive--;
        waveUI.text = "Enemies Left: " + enemiesAlive.ToString();
        if (enemiesAlive <= 0 || !waveActive)
        {
            Debug.Log("Wave Cleared!");
            waveUI.text = "Wave Cleared!";
            WaveCleared?.Invoke();
            waveActive = false;
        }
    }

}
