using UnityEngine;
using TMPro;

public class StructureHP : Damageable
{
    public Structure structure;

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

    // Handle death and destroy the parent object
    protected override void Die()
    {
        structure.ReturnToPool();
    }
}