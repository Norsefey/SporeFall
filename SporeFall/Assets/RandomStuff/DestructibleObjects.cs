using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObjects : Damageable
{
    public event Action<GameObject> OnDeath;

    private void OnEnable()
    {
        targetType = TargetType.Enemy;
        _health = maxHealth;
    }
    protected override float OnReceiveDamage(float amount)
    {
        _health -= amount;
        if (_health <= 0f)
            Die();

        return amount;
    }
    protected override void Die()
    {
        OnDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }
}
