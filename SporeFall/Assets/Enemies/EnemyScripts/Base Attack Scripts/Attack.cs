// Ignore Spelling: cooldown

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : ScriptableObject
{
    // Base ScriptableObject for all boss attacks
    [Header("Base Attack Settings")]
    [SerializeField] protected float range = 5f;
    [SerializeField] protected float cooldown = 2f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackDelay = 0.5f;
    [Tooltip("Time the boss must wait after attacking before moving")]
    [SerializeField] protected float recoveryTime = 0.5f;
    [SerializeField] protected string animationTrigger = "Attack";

    // Optional VFX and SFX references
    [Header("Effects")]
    [SerializeField] protected GameObject attackVFXPrefab;
    [SerializeField] protected AudioClip attackSFX;

    public float Range => range;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float AttackDelay => attackDelay;
    public float RecoveryTime => recoveryTime;
    private float lastUseTime;
    public bool CanUse(float distanceToTarget)
    {
        //Debug.Log("Last Use Time: " + lastUseTime + "Current Time: " + Time.time);
        // Are we in range, and is the current time greater then the the time stamp of when it was last used plus the cooldown time
        return distanceToTarget <= range && Time.time >= lastUseTime + cooldown;
    }

    public abstract IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target);

    protected void StartCooldown()
    {
        Debug.Log("Starting Cooldown");
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
