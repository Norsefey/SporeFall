using System.Collections;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private EnemyController eController;
    [SerializeField] private EnemyDefenseController defenseController;

    [Header("Flinching")]
    [SerializeField] private float flinchModifier = 1.0f;
    public bool flinchable = true;
    private bool isFlinching = false;

    [Header("Death")]
    [SerializeField] protected GameObject deathVFXPrefab;
    [SerializeField] protected GameObject myceliaDropPrefab;

    private void Awake()
    {
        if(eController == null)
            eController = transform.parent.GetComponent<EnemyController>();

        _health = maxHealth;
    }
    private void OnEnable()
    {
        targetType = TargetType.Enemy;
        _health = maxHealth;
    }
    protected override float OnReceiveDamage(float amount)
    {
        float finalDamage = defenseController.ProcessIncomingHit(amount);

        if (finalDamage <= 0f) return 0;  // dodged or fully blocked

        _health -= finalDamage;

        if (flinchable)
        {
            // Calculate flinch probability (higher HP = higher chance to flinch)
            float healthRatio = CurrentHealth / maxHealth; // 1 at full HP, 0 at 0 HP
            float flinchChance = healthRatio * flinchModifier; // Apply the modifier

            flinchChance = Mathf.Clamp01(flinchChance);

            if (isActiveAndEnabled && !isFlinching && Random.value < flinchChance)
            {
                StartCoroutine(Flinch());
            }
        }

        Debug.Log($"Took {amount} Damage: HP: {_health}");

        if (_health <= 0f) Die();
        return amount;
    }
    protected override void Die()
    {
        SpawnDeathVFX(eController.transform.position, Quaternion.Euler(-90, 0, 0));
        SpawnMyceliaDrop();
        TrySpawnWeaponDrop();
        base.Die();
    }
    public IEnumerator Flinch()
    {

        if (!flinchable || isFlinching || eController.EnemyAnimator == null)
            yield break;

        isFlinching = true;

        if (eController.CurrentState == EnemyState.Attacking)
            //manager.CancelAttack();

        eController.EnemyAnimator.Animator.SetTrigger("Flinch");
        eController.TransitionTo(EnemyState.Idle);

        yield return new WaitForSeconds(1);

        // Re-enable NavMeshAgent
        eController.TransitionTo(EnemyState.Moving);
        eController.EnemyAnimator.Animator.ResetTrigger("Flinch");
        isFlinching = false;
    }

    protected virtual void SpawnDeathVFX(Vector3 position, Quaternion rotation)
    {
        if (deathVFXPrefab != null && PoolManager.Instance != null)
        {
            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(deathVFXPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for VFX prefab: {deathVFXPrefab.name}");
                return;
            }
            VFXPoolingBehavior vfx = pool.Get(position, rotation);
            vfx.Initialize(pool);
        }
    }
    public void SpawnMyceliaDrop()
    {
        if (myceliaDropPrefab == null || PoolManager.Instance == null)
            return;
        // Get mycelia drop from pool
        if (!PoolManager.Instance.dropsPool.TryGetValue(myceliaDropPrefab, out DropsPool myceliaPool))
        {
            Debug.LogError($"No pool found for mycelia prefab: {myceliaDropPrefab.name}");
            return;
        }

        DropsPoolBehavior myceliaDrop = myceliaPool.Get(eController.transform.position, transform.rotation);
        myceliaDrop.Initialize(myceliaPool);

        if (myceliaDrop.TryGetComponent<MyceliaPickup>(out var mycelia))
        {
            Debug.Log($"Setting up mycelia drop with amount: {eController.Stats.MyceliaDropAmount}");   

            mycelia.Setup(eController.Stats.MyceliaDropAmount);
        }
    }
    private void TrySpawnWeaponDrop()
    {
        // Check if we have any weapons to drop and if we pass the random chance check
        if (PoolManager.Instance == null || eController.Stats.WeaponDropPrefabs.Length == 0 || Random.Range(0f, 100f) > eController.Stats.WeaponDropChance)
        {
            return;
        }

        // Select a random weapon from the array
        int dropIndex = Random.Range(0, eController.Stats.WeaponDropPrefabs.Length);
        GameObject selectedWeaponPrefab = eController.Stats.WeaponDropPrefabs[dropIndex];

        // Get the appropriate pool for this weapon
        if (!PoolManager.Instance.dropsPool.TryGetValue(selectedWeaponPrefab, out DropsPool weaponPool))
        {
            Debug.LogError($"No pool found for weapon prefab: {selectedWeaponPrefab.name}");
            return;
        }

        // Spawn the weapon drop slightly above the enemy position to prevent clipping
        Vector3 dropPosition = transform.position;
        dropPosition.y -= 1.5f;
        DropsPoolBehavior weaponDrop = weaponPool.Get(dropPosition, transform.rotation);
        weaponDrop.Initialize(weaponPool);  // Initialize with the correct weapon pool

        Debug.Log($"{gameObject.name} spawned weapon: {selectedWeaponPrefab.name}");
    }
}
