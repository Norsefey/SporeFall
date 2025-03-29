using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafeState : IEnemyState
{
    private BaseEnemy enemy;

    public StrafeState(BaseEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        enemy.agent.isStopped = false;
        enemy.agent.stoppingDistance /= 2;
    }
    public void UpdateState()
    {
        if (enemy.currentTarget != null)
        {
            // Move to strafe position
            enemy.agent.SetDestination(enemy.strafeTarget);

            // Look at target while strafing
            Vector3 lookDirection = (enemy.GetTargetPosition() - enemy.transform.position).normalized;
            lookDirection.y = 0;
            enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

            // If we're close to the strafe target, calculate a new one
            if (Vector3.Distance(enemy.transform.position, enemy.strafeTarget) < 1f)
            {
                enemy.CalculateStrafePosition();
            }
        }
    }
    public void ExitState()
    {
        enemy.agent.stoppingDistance *= 2; // Restore original stopping distance
    }


}
