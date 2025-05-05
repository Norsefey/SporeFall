using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : BaseProjectile
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    protected override void HandleImpact(Collider hitCollider)
    {
        // Create explosion effect
        CreateExplosionEffect();

        // Apply area damage
        ApplyExplosionDamage();
    }
    private void CreateExplosionEffect()
    {
        if (PoolManager.Instance != null && explosionEffectPrefab != null)
        {
            if (PoolManager.Instance.vfxPool.TryGetValue(explosionEffectPrefab, out VFXPool pool))
            {
                VFXPoolingBehavior explosiveVfx = pool.Get(transform.position, transform.rotation);
                explosiveVfx.Initialize(pool);
            }
            else
            {
                Instantiate(explosionEffectPrefab, transform.position, transform.rotation);
            }
        }
    }
    private void ApplyExplosionDamage()
    {
        // Get all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);

        foreach (var hit in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

            if (hit.TryGetComponent<Damageable>(out var damageable))
            {
                ApplyDamage(damageable, currentDamage * damageMultiplier);
            }
        }
    }
    // Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
