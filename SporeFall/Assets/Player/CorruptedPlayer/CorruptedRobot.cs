using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedPlayer : BaseEnemy
{
    public PlayerManager myPlayer;
    protected override float EvaluateAttackPriority(Attack attack, float distanceToTarget)
    {
        float priority = 1f;

        // priority logic based on attack type and conditions
        if (attack is MeleeAttack && distanceToTarget <= attack.Range)
        {
            priority *= 1.5f;
        }
        else if (attack is RangedAttack && distanceToTarget >= stoppingDistance)
        {
            priority *= 1.5f;
        }
        else if (attack is AoeAttack && currentTarget != train)
        {
            // if not attacking train, and target is near train, do an AOE attack to damage both
            float distanceBetweenTargets = Vector3.Distance(train.transform.position, currentTarget.position);
            if (distanceBetweenTargets <= 10)
            {
                priority *= 1.5f;
            }
        }

        return priority;
    }

    public override void Die()
    {
        // return life to player// default target is the controller which is a child of the player manager
        myPlayer.pHealth.IncreaseLife();
        base.Die();
    }
}
