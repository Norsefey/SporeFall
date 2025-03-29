using UnityEngine;
using TMPro;

public class StructureHP : Damageable
{
    public Structure structure;
    void Start()
    {
        currentHP = maxHP;
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
}