using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoHP : Damageable
{
    [SerializeField] private OctoBoss mainBody;

    public override void TakeDamage(float damage)
    {
        // Apply damage multiplier based on vulnerability
        float damageMultiplier = mainBody.CalculateDamageMultiplier();
        float modifiedDamage = damage * damageMultiplier;

        mainBody.PlayHitSoundFX();

        // Apply damage
        base.TakeDamage(modifiedDamage);

        // Log damage reduction if applicable
        if (damageMultiplier < 1.0f)
        {
            Debug.Log($"Damage reduced: {damage} → {modifiedDamage} (Multiplier: {damageMultiplier})");
        }
        else if (damageMultiplier > 1.0f)
        {
            Debug.Log($"Damage increased: {damage} → {modifiedDamage} (Multiplier: {damageMultiplier})");
        }
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
