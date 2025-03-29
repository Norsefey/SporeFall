using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IEnemyState
{
    private BaseEnemy enemy;
    public AttackState(BaseEnemy enemy)
    {
        this.enemy = enemy;
    }
    public void EnterState()
    {
        enemy.agent.isStopped = true;
    }
    public void UpdateState()
    {
        float distanceToTarget = enemy.GetDistanceToTarget();

        // Get best attack
        Attack bestAttack = enemy.GetBestAttack(distanceToTarget);

        if (bestAttack != null && enemy.currentTarget != null && enemy.currentTarget.gameObject.activeSelf)
        {
            // Calculate direction to the target for rotation
            Vector3 direction = (enemy.GetTargetPosition() - enemy.transform.position).normalized;
            direction.y = 0;

            // Rotate towards the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, 5 * Time.deltaTime);

            enemy.StartCoroutine(bestAttack.ExecuteAttack(enemy, enemy.currentTarget));
        }
    }
    public void ExitState()
    {
    }

  
}
