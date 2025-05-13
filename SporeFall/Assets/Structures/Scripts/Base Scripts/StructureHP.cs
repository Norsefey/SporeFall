using UnityEngine;
using TMPro;

public class StructureHP : Damageable
{
    public Structure structure;
    void Start()
    {
        ResetHealth();
        StoreOriginalMaxHealth();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }

    // Handle death and destroy the parent object
    protected override void Die()
    {
        //Debug.Log(gameObject.name + " has died.");
        structure.ReturnToPool();
    }
    public void SetMaxHPNoReset(float multiplier)
    {
        float newMaxHealth = originalMaxHealth * multiplier;
        maxHP = newMaxHealth;
        TakeDamage(0);
    }
}