using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainRelayHP : Damageable
{
    [SerializeField] TrainHP mainHp;
    public override void TakeDamage(float damage)
    {
        mainHp.TakeDamage(damage);
        Debug.Log($"Train Took: {damage} Damage");
    }

    protected override void Die()
    {
    }

    protected override void UpdateHPUI()
    {
    }
}
