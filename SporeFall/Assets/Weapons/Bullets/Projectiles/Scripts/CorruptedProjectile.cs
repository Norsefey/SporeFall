using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedProjectile : BaseProjectile
{

    protected override void HandleImpact(Collider collider)
    {
        if (collider == null)
            return;

        if (collider.transform.TryGetComponent<Damageable>(out var damageable))
        {
            if (damageable == null)
                return;

            // Apply damage
            ApplyDamage(damageable, currentCorruption);

            // Apply corruption if target can hold it
            if (damageable.canHoldCorruption)
            {
                damageable.IncreaseCorruption(currentCorruption);
            }
        }
    }
}
