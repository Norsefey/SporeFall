using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FinalWaveSettings
{
    [Header("Boss Squad Configuration")]
    [Tooltip("Define the specific types of enemies in the boss squad")]
    public EnemySpawnData[] bossSquadComposition;

    [Header("Post-Boss Settings")]
    [Tooltip("Number of enemies to spawn after boss is defeated")]
    public int postBossHordeSize = 30;
    [Tooltip("Enemy types that can appear in the post-boss horde")]
    public List<EnemySpawnData> hordeEnemyTypes = new();

    public int GetTotalEnemies()
    {
        int squadTotal = 0;
        foreach (var spawnData in bossSquadComposition)
        {
            squadTotal += spawnData.totalToSpawn;
        }
        return 1 + squadTotal; // Boss + squad + horde
    }
}
