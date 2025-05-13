using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTimeZone : MonoBehaviour
{
    DropsPoolBehavior myPool;
    protected float duration;
    protected float tickRate;
    protected float damagePerTick;
    protected float corruptionPerTick;
    protected float radius;
    private float nextTickTime;
    private float endTime;
    protected LayerMask hitTarget;
    public LayerMask targetToDamage;
    public virtual void Initialize(float duration, float tickRate, float damagePerTick,float corruptPerTick, float radius, LayerMask hitTarget)
    {
        this.duration = duration;
        this.tickRate = tickRate;
        this.damagePerTick = damagePerTick;
        this.radius = radius;
        this.hitTarget = hitTarget;
        corruptionPerTick = corruptPerTick;
        nextTickTime = Time.time;
        endTime = Time.time + duration;

        // Set the trigger collider size
        if (TryGetComponent<SphereCollider>(out var collider))
        {
            collider.radius = radius;
            collider.isTrigger = true;
        }
        transform.GetChild(0).localScale = new Vector3(radius,radius,radius) * 2;

        myPool = GetComponent<DropsPoolBehavior>();
    }

    protected virtual void Update()
    {
        if (Time.time >= endTime)
        {
            if (myPool != null && myPool.pool != null)
            {
                myPool.pool.Return(myPool);
            }
            else
            {
                Debug.Log("No Pool Destroying");
                Destroy(gameObject);
            }
        }

        if (Time.time >= nextTickTime)
        {
            ApplyDamageToTargetsInRange();
            nextTickTime = Time.time + tickRate;
        }
    }

    protected virtual void ApplyDamageToTargetsInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, targetToDamage);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<Damageable>(out var damageable))
            {
                damageable.TakeDamage(damagePerTick);
                damageable.IncreaseCorruption(corruptionPerTick);
            }
        }
    }
}
