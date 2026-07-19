// Ignore Spelling: cooldown

using System.Collections;
using UnityEngine;

public abstract class Attack : ScriptableObject
{
    // Base ScriptableObject for all attacks
    [Header("Selection")]
    [Tooltip("Relative weight for weighted-random attack selection. Higher = chosen more often.")]
    [Min(0.01f)]
    public float selectionWeight = 1f;
    [Tooltip("Optional minimum range before this attack can be selected (e.g. ranged attacks need distance).")]
    public float minSelectRange = 0f;
    [Tooltip("Seconds the enemy waits after this attack before choosing the next one.")]
    public float baseCooldown = 1.5f;

    [Header("Attack Settings")]
    public string attackName;
    [Tooltip("How far attack travels")]
    public float attackRange = 1;
    [Tooltip("Ideal firing distance. Enemy repositions toward this between attacks. " +
                 "Leave at 0 to auto-derive as midpoint of min/max range.")]
    public float preferredRange = 0f;

    [Tooltip("When to do Damage. Gives time for a wind up animation")]
    [SerializeField] public float attackDelay = 0.5f;
    [Tooltip("Time to wait after attacking before moving again. ")]
    [SerializeField] public float recoveryTime = 0.5f;
    [Tooltip("Name of trigger to activate correct animation")]
    [SerializeField] public string animationTrigger = "Attack";
    
    [Header("Damage Settings")]
    [Tooltip("Base damage of the attack.")]
    [SerializeField] public float baseDamage = 10f;
    [Tooltip("Variance in damage. Allows for randomization within a range.")]
    [SerializeField] public float damageVariance = 0f;
    
    [Header("Corruption Settings")]
    [Tooltip("Base corruption applied by the attack.")]
    public float baseCorruption = 0f;
    [Tooltip("Variance in corruption. Allows for randomization within a range.")]
     public float corruptionVariance = 0f;

    [Header("Effects")]
    [Tooltip("Visual effect prefab to spawn when the attack is executed.")]
    [SerializeField] protected GameObject attackVFXPrefab;
    [Tooltip("Sound effect to play when the attack is executed.")]
    [SerializeField] protected AudioClip attackSFX;

    public abstract AttackType AttackType { get; }
    public abstract IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target, float damageModifier, float corruptionModifier);
    public abstract void Execute(AttackInstance instance, Damageable target);
    protected virtual void SpawnVFX(Vector3 position, Quaternion rotation)
    {
        if (attackVFXPrefab != null && PoolManager.Instance != null)
        {
            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(attackVFXPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {attackVFXPrefab.name}");
                return;
            }
            VFXPoolingBehavior vfx = pool.Get(position, rotation);
            vfx.Initialize(pool);

            /*GameObject vfx = Instantiate(attackVFXPrefab, position, rotation);
            Destroy(vfx, 2f); // Incase it doesnt auto destroy*/
        }
    }
    protected virtual void PlaySFX(AudioSource audioSource)
    {
        if (attackSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSFX);
        }
    }

    public float GetPreferredRange()
            => preferredRange > 0f ? preferredRange : (minSelectRange + attackRange) * 0.5f;
}

public enum AttackType
{
    Melee,
    RangedProjectile,
    RangedHitScan, 
    AOE,
    MovementAttack
}
