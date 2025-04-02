using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPRelay : Damageable
{
    [SerializeField] Damageable mainHP;
    [SerializeField] float damageMultiplier = 1;

    public override void TakeDamage(float damage)
    {
        mainHP.TakeDamage(damage * damageMultiplier);
    }
    protected override void Die()
    {
       
    }
}
