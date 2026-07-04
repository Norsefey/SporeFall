// Ignore Spelling: Shroom

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class to hold enemy spawn data
[System.Serializable]
public class EnemySpawnData
{
    public string name;
    public bool mustSpawnOutside;
    public int spawnPointIndex; // -1 = random
    public bool spawnAsGroup;    // Whether to spawn as a group
    public int totalToSpawn;     // Total number of this enemy type to spawn
    public int groupSize;        // How many enemies per group (if spawning as group)
    private int spawnedCount;
    [Header("Variants To Spawn")]
    public GameObject[] enemyVariants;
    public GameObject EnemyToSpawn
    {
        get { return enemyVariants[Random.Range(0, enemyVariants.Length)]; }
    }
    public int SpawnedCount   // Track how many we've spawned
    {
        get { return spawnedCount; }
        set { spawnedCount = value; }
    }
    
    public int minLevel = 1; // Minimum level for this enemy type
    public int maxLevel = 1; // Maximum level for this enemy type
    public float spawnWeight = 1.0f; // Base weight for spawn probability calculation
}
[System.Serializable]
public class Wave
{
    public string waveName;
    [Header("Wave Type")]
    public bool isFinalWave = false;

    [Header("Train Park Location")]
    public Transform trainLocation;

    [Header("Enemy Settings")]
    public GameObject ShroomPod;
    public Transform[] presetSpawnPoints;
    public BoxCollider outSideSpawnZone;

    [Header("Regular Wave Enemies")]
    public List<EnemySpawnData> enemySpawnData = new();

    [Header("Spawn Settings")]
    public float minIntervalTime;
    public float maxIntervalTime;
    public int minToSpawnAtOnce;
    public int maxToSpawnAtOnce;

    [Header("Final Wave Settings")]
    [Tooltip("Only configure if this is the final wave")]
    public FinalWaveSettings finalWaveSettings;
    public int totalEnemies
    {
        get
        {
            if (!isFinalWave)
            {
                int total = 0;
                foreach (var spawnData in enemySpawnData)
                {
                    total += spawnData.totalToSpawn;
                }
                return total;
            }
            else
            {
                return finalWaveSettings.GetTotalEnemies();
            }
        }
    }
}

[System.Serializable]
public class WaveDefinition
{
    public string label = "Wave";
    public Transform[] presetSpawnPoints;
    public BoxCollider outSideSpawnZone;
    public List<SpawnGroup> groups = new();
}

[System.Serializable]
public class SpawnGroup
{
    public EnemyController enemyPrefab;
    public int count = 10;
    public int level = 1;
    public float spawnInterval = 0.3f;   // seconds between enemies in this group
    public float delayAfterGroup = 2f;    // pause before the next group
}
