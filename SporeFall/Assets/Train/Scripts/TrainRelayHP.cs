using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainRelayHP : Damageable
{
    public TrainHP mainHp;

    public event Action OnRelayHit;

    public override void TakeDamage(float damage)
    {
        mainHp.TakeDamage(damage);
        //Debug.Log($"Train Took: {damage} Damage");
        OnRelayHit?.Invoke();
    }

    protected override void Die()
    {
    }
}
