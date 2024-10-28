using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainRelayHP : Damageable
{
    [SerializeField] TrainHP mainHp;
    public override void TakeDamage(float damage)
    {
        mainHp.TakeDamage(damage);
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
