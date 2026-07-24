using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoHP : Damageable
{
    private void OnEnable()
    {
        targetType = TargetType.Structure;
        _health = maxHealth;
        EnemyTargetRegistry.Instance?.Register(this);
    }

    private void OnDisable()
    {
        EnemyTargetRegistry.Instance?.Unregister(this);
    }

    protected override float OnReceiveDamage(float amount)
    {
        _health -= amount;
        if (_health <= 0f) Die();
        return amount;
    }

    protected override void Die()
    {
        
    }
}
