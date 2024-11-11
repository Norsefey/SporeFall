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

    protected override float EvaluateAttackPriority(Attack attack, float distanceToTarget)
    {
        float priority = 1f;

        if (attack is ExplosiveAttack && distanceToTarget <= attack.Range)
        {
            priority *= 2f;
        }

        return priority;
    }

    private void SpawnDrop()
    {
        // Get Drop from pool
        if (!PoolManager.Instance.dropsPool.TryGetValue(myceliaDropPrefab, out DropsPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {myceliaDropPrefab.name}");
            return;
        }
        DropsPoolBehavior myceliaDrop = pool.Get(transform.position, transform.rotation);
        myceliaDrop.Initialize(pool);


        /*myceliaDrop.TryGetComponent<MyceliaPickup, out mycelia>();
        mycelia.Setup(myceliaDropAmount);*/
        if(myceliaDrop.TryGetComponent<MyceliaPickup>(out var mycelia))
        {
            // Assign random amount to mycelia drop
            float worth = Mathf.Round(Random.Range(minMyceliaWorth, maxMyceliaWorth));
            mycelia.Setup(minMyceliaWorth);
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

                //var weapon = Instantiate(weaponDropPrefab[dropIndex], transform.position, Quaternion.identity);
                // so we can remove it if player doesn't pick it up, set as child of drops holder
                //weapon.transform.SetParent(train.dropsHolder, true);
            }
        }
    }
}
