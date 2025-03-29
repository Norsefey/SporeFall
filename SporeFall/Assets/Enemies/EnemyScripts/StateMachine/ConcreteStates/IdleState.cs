using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IEnemyState
{
    private BaseEnemy enemy;
    public IdleState(BaseEnemy enemy)
    {
        this.enemy = enemy;
    }
    public void EnterState()
    {
        enemy.agent.isStopped = true;
    }
    public void UpdateState()
    {
        if (enemy.currentTarget != null)
        {
            Vector3 lookDirection = (enemy.GetTargetPosition() - enemy.transform.position).normalized;
            lookDirection.y = 0;
            enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
        }
    }
    public void ExitState()
    {
        enemy.agent.isStopped = false;
    }


}
