using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerProjectile : BaseProjectile
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] entitiesToSpawn;

    protected override void HandleImpact(Collision collision)
    {
        SpawnEntity();
    }

    private void SpawnEntity()
    {
        if (entitiesToSpawn.Length == 0 || GameManager.Instance == null ||
            GameManager.Instance.waveManager == null)
            return;

        int index = Random.Range(0, entitiesToSpawn.Length);
        GameManager.Instance.waveManager.SpawnEnemy(entitiesToSpawn[index], transform.position, true);
    }
}
