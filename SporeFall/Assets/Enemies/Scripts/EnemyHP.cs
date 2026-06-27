using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private EnemyController eController;

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

        if (eController == null)
            Debug.LogError(transform.parent.name + " Main HP has no Manager Reference");
        _health = maxHealth;
    }
    private void OnEnable()
    {
        targetType = TargetType.Enemy;
        _health = maxHealth;
    }

    private void OnDisable()
    {
    }
    protected override float OnReceiveDamage(float amount)
    {
        _health -= amount;

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

        if (_health <= 0f) Die();
        return amount;
    }
    protected override void Die()
    {
        if (eController.CurrentState == EnemyState.Dead) return;

        base.Die();

        eController.TransitionTo(EnemyState.Dead);

        eController.ReleaseCurrentToken();
        SpawnDeathVFX(eController.transform.position, Quaternion.Euler(-90, 0, 0));
        SpawnMyceliaDrop();
        gameObject.SetActive(false); // Deactivate instead of destroy

        // Return to pool via WaveManager / EnemyPool
        eController.ResetForPool();
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
            mycelia.Setup(eController.Stats.MyceliaDropAmount);
        }
    }
}
