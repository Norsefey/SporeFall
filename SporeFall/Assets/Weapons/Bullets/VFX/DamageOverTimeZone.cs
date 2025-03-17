using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTimeZone : MonoBehaviour
{
    protected float duration;
    protected float tickRate;
    protected float damagePerTick;
    protected float radius;
    private float nextTickTime;
    private float endTime;
    protected LayerMask hitTarget;
    public LayerMask targetToDamage;
    public virtual void Initialize(float duration, float tickRate, float damagePerTick, float radius, LayerMask hitTarget)
    {
        this.duration = duration;
        this.tickRate = tickRate;
        this.damagePerTick = damagePerTick;
        this.radius = radius;
        this.hitTarget = hitTarget;

        nextTickTime = Time.time;
        endTime = Time.time + duration;

        // Set the trigger collider size
        if (TryGetComponent<SphereCollider>(out var collider))
        {
            collider.radius = radius;
            collider.isTrigger = true;
        }
        transform.GetChild(0).localScale *= (radius * 2);
    }

    protected virtual void Update()
    {
        if (Time.time >= endTime)
        {
            Destroy(gameObject);
            return;
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
            }
        }
    }
}
