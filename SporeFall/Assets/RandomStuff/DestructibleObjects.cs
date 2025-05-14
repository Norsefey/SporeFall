using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObjects : Damageable
{
    public event Action<GameObject> OnDeath;

    protected override void Die()
    {
        OnDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }
}
