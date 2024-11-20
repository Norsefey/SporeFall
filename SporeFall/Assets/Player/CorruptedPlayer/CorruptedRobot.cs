using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedPlayer : BaseEnemy
{
    public PlayerManager myPlayer;
    [Header("Drops")]
    [SerializeField] GameObject myceliaDropPrefab;
    [SerializeField] private float myceliaDropAmount = 5;
    [SerializeField] GameObject[] weaponDropPrefab;
    [SerializeField] private float dropChance = 20;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override float EvaluateAttackPriority(Attack attack, float distanceToTarget)
    {
        float priority = 1f;

        // priority logic based on attack type and conditions
        if (attack is MeleeAttack && distanceToTarget <= attack.Range)
        {
            priority *= 1.5f;
        }
        else if (attack is RangedAttack && distanceToTarget >= stoppingDistance)
        {
            priority *= 1.5f;
        }
        else if (attack is AoeAttack && currentTarget != train)
        {
            // if not attacking train, and target is near train, do an AOE attack to damage both
            float distanceBetweenTargets = Vector3.Distance(train.transform.position, currentTarget.position);
            if (distanceBetweenTargets <= 10)
            {
                priority *= 1.5f;
            }
        }

        return priority;
    }

    public override void Die()
    {
        // return life to player// default target is the controller which is a child of the player manager
        myPlayer.pHealth.IncreaseLife();
        SpawnDrop();
        GameManager.Instance.WaveManager.RemoveRobert(gameObject);
        base.Die();
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
