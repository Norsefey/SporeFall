using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerProjectile : BaseProjectile
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] entitiesToSpawn;
    [SerializeField] int minLevel, maxLevel;

    protected override void HandleImpact(Collider collider)
    {
        SpawnEntity();
    }

    private void SpawnEntity()
    {
        if (entitiesToSpawn.Length == 0 || GameManager.Instance == null ||
            GameManager.Instance.waveManager == null)
            return;

        int index = Random.Range(0, entitiesToSpawn.Length);
        int level = Random.Range(minLevel, maxLevel + 1);
        GameManager.Instance.waveManager.SpawnEnemy(entitiesToSpawn[index], level, transform.position, true);
    }
}
