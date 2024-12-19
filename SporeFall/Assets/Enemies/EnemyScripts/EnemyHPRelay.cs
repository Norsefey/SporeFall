using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPRelay : Damageable
{
    [SerializeField] EnemyHP mainHP;
    [SerializeField] float damageMultiplier = 1;

    public override void TakeDamage(float damage)
    {
        mainHP.TakeDamage(damage * damageMultiplier);
    }
    public void KnockBack(Vector3 attackerPosition, float knockbackMultiplier)
    {
        if(mainHP.isActiveAndEnabled)
            StartCoroutine(mainHP.KnockBack(attackerPosition, knockbackMultiplier));
    }
    protected override void Die()
    {
        throw new System.NotImplementedException();
    }

    protected override void UpdateUI()
    {
        throw new System.NotImplementedException();
    }
}
