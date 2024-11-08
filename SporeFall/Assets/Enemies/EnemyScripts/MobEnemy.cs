using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobEnemy : BaseEnemy
{
    [Header("Drops")]
    [SerializeField] GameObject myceliaDropPrefab;
    [SerializeField] private float myceliaDropAmount = 5;
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

        var mycelia = myceliaDrop.GetComponent<MyceliaPickup>();
        mycelia.Setup(myceliaDropAmount);

       /* if (train != null)
        {
            // so we can remove it if player doesn't pick it up, set as child of drops holder
            mycelia.transform.SetParent(train.dropsHolder, true);
        }*/

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
