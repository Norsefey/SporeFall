using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardProjectile : BaseProjectile
{
    protected override void HandleImpact(Collider collider)
    {
        if (collider.TryGetComponent<Damageable>(out var damageable))
        {
            ApplyDamage(damageable, currentDamage);
        }
    }
}
