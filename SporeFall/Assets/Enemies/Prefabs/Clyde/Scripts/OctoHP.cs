using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoHP : Damageable
{
    [SerializeField] private OctoBoss mainBody;

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
        float damageMultiplier = mainBody.CalculateDamageMultiplier();
        float modifiedDamage = amount * damageMultiplier;

        mainBody.PlayHitSoundFX();

        _health -= modifiedDamage;
        if (_health <= 0f) Die();
        return amount;
    }

    protected override void Die()
    {
        if (mainBody != null)
        {
            mainBody.Die();
        }
        else
        {
            Debug.LogError("Octo Body HP has no reference to TentacleEnemy!");
            gameObject.SetActive(false);
        }
    }
}
