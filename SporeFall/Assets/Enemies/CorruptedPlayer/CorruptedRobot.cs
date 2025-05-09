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
    [SerializeField] GameObject[] weaponDropPrefab;
    [SerializeField] private float dropChance = 20;
    protected override void Awake()
    {
        base.Awake();
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
        if (SavedSettings.firstRobertKill == false && Tutorial.Instance != null)
        {
            Tutorial.Instance.RobertKillPrompts();
            SavedSettings.firstRobertKill = true;
        }
        GameManager.Instance.waveManager.RemoveRobert(gameObject);
        base.Die();
    }
    public void AssignDefaultTargets(TrainHandler train, Transform playerTarget)
    {
        base.AssignTrain(train);
        currentTarget = playerTarget;
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
            mycelia.Setup();
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
