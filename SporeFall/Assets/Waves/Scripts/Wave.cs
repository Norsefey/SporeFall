// Ignore Spelling: Shroom

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// New class to hold enemy spawn data
[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int totalToSpawn;     // Total number of this enemy type to spawn
    public bool spawnAsGroup;    // Whether to spawn as a group
    public int groupSize;        // How many enemies per group (if spawning as group)
    private int spawnedCount;
    public int SpawnedCount   // Track how many we've spawned
    {
        get { return spawnedCount; }
        set { spawnedCount = value; }
    }
}
[System.Serializable]
public class Wave
{
    public string waveName; // Optional, for easier identification
    [Header("Is Final Wave")]
    public bool isFinalWave = false; // Flag to identify if this is a boss wave
    [Header("Train Park Location")]
    public Transform trainLocation; // The location of train for this wave

    [Header("Enemy Settings")]
    public GameObject ShroomPod; // pod blocking path forward
    public Transform[] spawnLocations;

    public List<EnemySpawnData> enemySpawnData = new();

    [Header("Spawn Settings")]
    public float minIntervalTime; // Time interval between each spawn
    public float maxIntervalTime;
    public int minIntervalSpawn; // Number of enemies to spawn per interval
    public int maxIntervalSpawn;

    // Calculate total enemies for the wave
    public int totalEnemies
    {
        get
        {
            int total = 0;
            foreach (var spawnData in enemySpawnData)
            {
                total += spawnData.totalToSpawn;
            }
            return total;
        }
    }
}
