using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedProjectile : BaseProjectile
{
    [Header("Corruption Settings")]
    [SerializeField] private float corruptionAmount = 10f;

    protected override void HandleImpact(Collider collider)
    {
        if (collider == null)
            return;

        if (collider.transform.TryGetComponent<Damageable>(out var damageable))
        {
            if (damageable == null)
                return;

            // Apply damage
            ApplyDamage(damageable, currentDamage);

            // Apply corruption if target can hold it
            if (damageable.canHoldCorruption)
            {
                damageable.IncreaseCorruption(corruptionAmount);
            }
        }
    }
}
