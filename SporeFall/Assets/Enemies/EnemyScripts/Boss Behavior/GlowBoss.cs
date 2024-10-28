using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlowBoss : BaseEnemy
{
    protected override float EvaluateAttackPriority(Attack attack, float distanceToTarget)
    {
        float priority = 1f;

        // priority logic based on attack type and conditions
        if (attack is MeleeAttack && health.CurrentHP < health.CurrentHP * 0.3f || distanceToTarget <= attack.Range)
        {
            priority *= 1.5f;
        }
        else if (attack is RangedAttack && distanceToTarget > stoppingDistance * 1.4f)
        {
            priority *= 1.2f;
        }
        else if (attack is AoeAttack)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToPlayer <= attack.Range)
            {
                priority *= 2f;
            }
        }

        return priority;
    }

    public override void Die()
    {
        // Speed up payload
        train.Payload.IncreaseSpeed();
        base.Die();
    }
}
