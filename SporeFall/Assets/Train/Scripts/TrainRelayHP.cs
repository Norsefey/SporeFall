using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainRelayHP : Damageable
{
    public TrainHP mainHp;
    public event Action OnRelayHit;

    private void Start()
    {
        targetType = TargetType.TrainWall;
        EnemyTargetRegistry.Instance?.Register(this);
    }

    private void OnEnable()
    {
        targetType = TargetType.TrainWall;
        EnemyTargetRegistry.Instance?.Register(this);
    }
    private void OnDisable()
    {
        EnemyTargetRegistry.Instance?.Unregister(this);
    }
    protected override float OnReceiveDamage(float amount)
    {
        if (mainHp == null) return 0;
        
        OnRelayHit?.Invoke();
        return mainHp.ReceiveDamage(amount);
    }
}
