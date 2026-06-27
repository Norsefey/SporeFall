using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPRelay : Damageable
{
    [SerializeField] float damageMultiplier = 1;
    [SerializeField] Damageable mainHP;


    private void OnEnable()
    {
        targetType = TargetType.Enemy;
        _health = maxHealth;
    }

    private void OnDisable()
    {

    }
    protected override float OnReceiveDamage(float amount)
    {
        mainHP.ReceiveDamage(amount * damageMultiplier);
        return amount;
    }

    protected override void Die()
    {
       
    }

    public bool IsDead()
    {
        return !mainHP.IsAlive;
    }
}
