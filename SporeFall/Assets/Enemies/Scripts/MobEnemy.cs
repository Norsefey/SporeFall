using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobEnemy : BaseEnemy
{
    [Header("Drops")]
    [SerializeField] GameObject[] weaponDropPrefab;
    [SerializeField] private float dropChance = 20;
    public override void Die()
    {
        SpawnDrop();
        base.Die();
    }
    private void SpawnDrop()
    {
        SpawnMyceliaDrop();
        TrySpawnWeaponDrop();
    }
    private void TrySpawnWeaponDrop()
    {
        // Check if we have any weapons to drop and if we pass the random chance check
        if (PoolManager.Instance == null || weaponDropPrefab.Length == 0 || Random.Range(0f, 100f) > dropChance)
        {
            return;
        }

        // Select a random weapon from the array
        int dropIndex = Random.Range(0, weaponDropPrefab.Length);
        GameObject selectedWeaponPrefab = weaponDropPrefab[dropIndex];

        // Get the appropriate pool for this weapon
        if (!PoolManager.Instance.dropsPool.TryGetValue(selectedWeaponPrefab, out DropsPool weaponPool))
        {
            Debug.LogError($"No pool found for weapon prefab: {selectedWeaponPrefab.name}");
            return;
        }

        // Spawn the weapon drop slightly above the enemy position to prevent clipping
        Vector3 dropPosition = transform.position;
        DropsPoolBehavior weaponDrop = weaponPool.Get(dropPosition, transform.rotation);
        weaponDrop.Initialize(weaponPool);  // Initialize with the correct weapon pool

        Debug.Log($"{gameObject.name} spawned weapon: {selectedWeaponPrefab.name}");
    }
}
