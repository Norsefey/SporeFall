// Ignore Spelling: Shroom

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public string waveName; // Optional, for easier identification
    public Transform trainLocation; // The location of train for this wave

    [Header("Enemy Settings")]
    public GameObject ShroomPod; // pod blocking path forward
    public int totalEnemies; // Total number of enemies to spawn
    public GameObject[] enemyPrefabs; // Different types of enemies that can spawn in this wave

    [Header("Spawn Settings")]
    public Transform[] spawnLocations;// location for enemies to spawn
    public float minIntervalTime; // Time interval between each spawn
    public float maxIntervalTime;
    public int minIntervalSpawn; // Number of enemies to spawn per interval
    public int maxIntervalSpawn;
    [Header("Is Final Wave")]
    public bool isFinalWave = false; // Flag to identify if this is a boss wave
}
