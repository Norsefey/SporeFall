using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

            if(damageable is PlayerHP)
            {
                PlayerHP playerHP = (PlayerHP)damageable;
                playerHP.IncreaseCorruption(corruptionDamage);
            }
            // Apply damage
            ApplyDamage(damageable, damage);

            if (damageable is PlayerHP)
            {
                PlayerHP playerHP = (PlayerHP)damageable;

                playerHP.IncreaseCorruption(corruptionDamage);
            }
        }
    }
}
