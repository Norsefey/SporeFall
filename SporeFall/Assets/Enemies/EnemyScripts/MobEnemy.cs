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
        var mycelia = Instantiate(myceliaDropPrefab, transform.position, Quaternion.identity).GetComponent<MyceliaPickup>();
        mycelia.Setup(myceliaDropAmount);

        if (train != null)
        {
            // so we can remove it if player doesn't pick it up, set as child of drops holder
            mycelia.transform.SetParent(train.dropsHolder, true);
        }

        if (weaponDropPrefab.Length != 0)
        {
            float randomChance = Random.Range(0, 100);
            if (randomChance <= dropChance)
            {
                int dropIndex = Random.Range(0, weaponDropPrefab.Length);
                var weapon = Instantiate(weaponDropPrefab[dropIndex], transform.position, Quaternion.identity);
                // so we can remove it if player doesn't pick it up, set as child of drops holder
                weapon.transform.SetParent(train.dropsHolder, true);
            }
        }
    }
}
