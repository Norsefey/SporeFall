using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobEnemy : BaseEnemy
{
    [Header("Drops")]
    [SerializeField] GameObject myceliaDropPrefab;
    [SerializeField] private float minMyceliaWorth = 5;
    [SerializeField] private float maxMyceliaWorth = 10;
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
    private void SpawnMyceliaDrop()
    {
        if (myceliaDropPrefab == null || PoolManager.Instance == null)
            return;
        // Get mycelia drop from pool
        if (!PoolManager.Instance.dropsPool.TryGetValue(myceliaDropPrefab, out DropsPool myceliaPool))
        {
            Debug.LogError($"No pool found for mycelia prefab: {myceliaDropPrefab.name}");
            return;
        }

        DropsPoolBehavior myceliaDrop = myceliaPool.Get(transform.position, transform.rotation);
        myceliaDrop.Initialize(myceliaPool);

        if (myceliaDrop.TryGetComponent<MyceliaPickup>(out var mycelia))
        {
            float worth = Mathf.Round(Random.Range(minMyceliaWorth, maxMyceliaWorth));
            mycelia.Setup(damageModifier);
        }
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
