using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPRelay : Damageable
{
    [SerializeField] float damageMultiplier = 1;
    [SerializeField] Damageable mainHP;

    public override void TakeDamage(float damage)
    {
        mainHP.TakeDamage(damage * damageMultiplier);
    }
    protected override void Die()
    {
       
    }

    public bool IsDead()
    {
        return mainHP.isDead;
    }
}
