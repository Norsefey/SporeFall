using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave")]
[System.Serializable]
public class Wave : ScriptableObject
{
    public string waveName; // Optional, for easier identification
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Different types of enemies that can spawn in this wave
    public int totalEnemies; // Total number of enemies to spawn
    [Header("Spawn Settings")]
    public float minIntervalTime; // Time interval between each spawn
    public float maxIntervalTime;
    public int minIntervalSpawn; // Number of enemies to spawn per interval
    public int maxIntervalSpawn;
    [Header("Is Final Wave")]
    public bool isFinalWave = false; // Flag to identify if this is a boss wave
}
