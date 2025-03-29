using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private BaseEnemy enemy;
    public ChaseState(BaseEnemy enemy)
    {
        this.enemy = enemy;
    }
    public void EnterState()
    {
        enemy.agent.isStopped = false;
    }
    public void UpdateState()
    {
        float distanceToTarget = enemy.GetDistanceToTarget();

        // Check if we're outside the stopping distance
        if (distanceToTarget > enemy.agent.stoppingDistance)
        {
            // Set up agent for movement
            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(enemy.GetTargetPosition());
        }
        else
        {
            // We're within attack range, transition to attack state
            enemy.agent.isStopped = true;
        }
    }
    public void ExitState()
    {
       
    }

   
}
