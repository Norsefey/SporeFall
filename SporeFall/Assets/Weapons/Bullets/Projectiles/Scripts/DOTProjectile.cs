using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTProjectile : BaseProjectile
{
    [Header("DOT Zone Settings")]
    [SerializeField] private GameObject dotZonePrefab;
    [SerializeField] private float dotDuration = 5f;
    [SerializeField] private float dotTickRate = 1f;
    [SerializeField] private float dotDamagePerTick = 10f;
    [SerializeField] private float dotRadius = 3f;
    [SerializeField] private float dotCorruptionPerTick = 0f;

    protected override void HandleImpact(Collision collision)
    {
        // Apply initial damage if needed
        if (collision.collider.TryGetComponent<Damageable>(out var damageable))
        {
            ApplyDamage(damageable, currentDamage);
        }

        // Create DOT zone
        CreateDOTZone();
    }

    private void CreateDOTZone()
    {
        if (dotZonePrefab != null)
        {
            GameObject zoneObject = Instantiate(dotZonePrefab, transform.position, Quaternion.identity);

            if (zoneObject.TryGetComponent<DamageOverTimeZone>(out var dotZone))
            {
                dotZone.Initialize(dotDuration, dotTickRate, dotDamagePerTick,
                                  dotCorruptionPerTick, dotRadius, hitLayers);
            }
        }
    }

    // Visualize DOT radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, dotRadius);
    }
}
