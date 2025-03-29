using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RetreatState : IEnemyState
{
    private BaseEnemy enemy;

    public RetreatState(BaseEnemy enemy)
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
            Vector3 retreatDirection = enemy.transform.position - enemy.GetTargetPosition();
            Vector3 retreatPosition = enemy.transform.position + retreatDirection.normalized * enemy.agent.stoppingDistance * 2;

            // Quick ground check before NavMesh sampling
            if (Physics.Raycast(retreatPosition + Vector3.up * 5, Vector3.down, out RaycastHit hit, 10f))
            {
                retreatPosition = hit.point;
            }

            if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit navHit, enemy.agent.stoppingDistance * 2, NavMesh.AllAreas))
            {
                enemy.agent.SetDestination(navHit.position);
            }
        }
    }
    public void ExitState()
    {
        enemy.agent.stoppingDistance *= 2; // Restore original stopping distance
    }


}
