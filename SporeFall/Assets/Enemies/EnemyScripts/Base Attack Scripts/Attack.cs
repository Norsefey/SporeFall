// Ignore Spelling: cooldown

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : ScriptableObject
{
    // Base ScriptableObject for all boss attacks
    [Header("Base Attack Settings")]
    [Tooltip("Priority of the attack. Higher values indicate higher priority.")]
    public float priority = 1f;
    [Tooltip("How close to target to do attack")]
    [SerializeField] protected float range = 5f;
    [Tooltip("How Long until Attack can be used again")]
    [SerializeField] protected float cooldown = 2f;
    [Tooltip("When to do Damage. Gives time for a wind up animation")]
    [SerializeField] protected float attackDelay = 0.5f;
    [Tooltip("Time to wait after attacking before moving again. ")]
    [SerializeField] protected float recoveryTime = 0.5f;
    [Tooltip("Name of trigger to activate correct animation")]
    [SerializeField] protected string animationTrigger = "Attack";
    
    [Header("Damage Settings")]
    [Tooltip("Base damage of the attack.")]
    [SerializeField] protected float damage = 10f;
    [Tooltip("Variance in damage. Allows for randomization within a range.")]
    [SerializeField] protected float damageVariance = 0f;

    [Header("Corruption Settings")]
    [Tooltip("Base corruption applied by the attack.")]
    [SerializeField] protected float corruption = 0f;
    [Tooltip("Variance in corruption. Allows for randomization within a range.")]
    [SerializeField] protected float corruptionVariance = 0f;

    [Header("Effects")]
    [Tooltip("Visual effect prefab to spawn when the attack is executed.")]
    [SerializeField] protected GameObject attackVFXPrefab;
    [Tooltip("Sound effect to play when the attack is executed.")]
    [SerializeField] protected AudioClip attackSFX;

    public float Range => range;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float AttackDelay => attackDelay;
    public float RecoveryTime => recoveryTime;
    private float lastUseTime = 0;
    public bool CanUse(float distanceToTarget)
    {
        //Debug.Log("Last Use Time: " + lastUseTime + "Current Time: " + Time.time + (Time.time >= lastUseTime + cooldown));
        //Debug.Log("In Range: " + (distanceToTarget <= range));
        return distanceToTarget <= range && Time.time >= lastUseTime + cooldown;
    }
    public abstract IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target, float damageModifier, float corruptionModifier);
    protected void StartCooldown()
    {
        lastUseTime = Time.time;
    }
    public void ResetCooldown()
    {
        lastUseTime = 0;
    }
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
}
