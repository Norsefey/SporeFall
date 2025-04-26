using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlowBoss : BaseEnemy
{
    [Header("Drops")]
    [SerializeField] GameObject myceliaDropPrefab;
    [Tooltip("How Many balls to drop")]
    [SerializeField] private int myceliaDropCount = 0;
    [Tooltip("How much each ball is worth")]
    [SerializeField] private float myceliaWorth = 25;

    [SerializeField] GameObject[] weaponDropPrefab;
    [SerializeField] private float dropChance = 5;
    private void SpawnDrop()
    {
        // Get Drop from pool
        if (!PoolManager.Instance.dropsPool.TryGetValue(myceliaDropPrefab, out DropsPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {myceliaDropPrefab.name}");
            return;
        }

        for (int x = 0; x < myceliaDropCount; x++)
        {
            DropsPoolBehavior myceliaDrop = pool.Get(transform.position, transform.rotation);
            myceliaDrop.Initialize(pool);
            /*myceliaDrop.TryGetComponent<MyceliaPickup, out mycelia>();
            mycelia.Setup(myceliaDropAmount);*/
            if (myceliaDrop.TryGetComponent<MyceliaPickup>(out var mycelia))
            {
                mycelia.Setup(myceliaWorth);
            }
        }
        if (weaponDropPrefab.Length != 0)
        {
            float randomChance = Random.Range(0, 100);
            if (randomChance <= dropChance)
            {
                int dropIndex = Random.Range(0, weaponDropPrefab.Length);
                // Get Drop from pool
                if (!PoolManager.Instance.dropsPool.TryGetValue(weaponDropPrefab[dropIndex], out DropsPool WeaponPool))
                {
                    Debug.LogError($"No pool found for enemy prefab: {weaponDropPrefab[dropIndex].name}");
                    return;
                }
                DropsPoolBehavior weaponDrop = WeaponPool.Get(transform.position, transform.rotation);
                weaponDrop.Initialize(pool);
            }
        }
    }
    public override void Die()
    {
        SpawnDrop();
        base.Die();
    }
}
