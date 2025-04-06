using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedPlayer : BaseEnemy
{
    [Header("Robert References")]
    public PlayerManager myPlayer;

    [SerializeField] EnemyHP myEnemyHP;

    [Header("Drops")]
    [SerializeField] GameObject myceliaDropPrefab;
    [SerializeField] private float myceliaDropAmount = 100;
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
        else if (attack is AoeAttack && currentTarget != null && train != null)
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
    protected override void Update()
    {
        if (isAttacking)
        {
            myEnemyHP.flinchable = false;
        }
        else if (!myEnemyHP.flinchable)
        {
            myEnemyHP.flinchable = true;

        }
        base.Update();
    }
    public override void Die()
    {
        // return life to player
        myPlayer.pHealth.IncreaseLife();
        SpawnDrop();
        GameManager.Instance.waveManager.RemoveRobert(gameObject);
        base.Die();
    }
    private void SpawnDrop()
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
            mycelia.Setup(myceliaDropAmount);  // Fixed to use the calculated worth instead of minMyceliaWorth
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
