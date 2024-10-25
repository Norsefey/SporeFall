using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlowBoss : BaseBoss
{
    protected override float EvaluateAttackPriority(Attack attack, float distanceToTarget)
    {
        float priority = 1f;

        // priority logic based on attack type and conditions
        if (attack is MeleeAttack && health.CurrentHP < health.CurrentHP * 0.3f)
        {
            priority *= 1.5f;
        }
        else if (attack is RangedAttack && distanceToTarget > minApproachDis * 2)
        {
            priority *= 1.2f;
        }
        else if (attack is AoeAttack)
        {
            // Check if both player and payload are within AOE range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float distanceToPayload = Vector3.Distance(transform.position, payload.position);
            if (distanceToPlayer <= attack.Range && distanceToPayload <= attack.Range)
            {
                priority *= 2f;
            }
        }

        return priority;
    }
}
