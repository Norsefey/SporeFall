using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessModeSpawnerProjectile : BaseProjectile
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] entitiesToSpawn;

    protected override void HandleImpact(Collider collider)
    {
        Debug.Log("Egg Hit Ground");
        SpawnEntity();
    }

    private void SpawnEntity()
    {
        if (entitiesToSpawn.Length == 0 || GameManager.Instance == null ||
            GameManager.Instance.endlessWaveManager == null)
            return;

        int index = Random.Range(0, entitiesToSpawn.Length);
        GameManager.Instance.endlessWaveManager.SpawnEnemy(entitiesToSpawn[index], transform.position, true);
    }
}
