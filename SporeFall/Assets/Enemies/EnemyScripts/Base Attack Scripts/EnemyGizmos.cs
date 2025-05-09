using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseEnemy))]
public class EnemyGizmos : MonoBehaviour
{
    private BaseEnemy enemy;

    private void Awake()
    {
        enemy = GetComponent<BaseEnemy>();
    }

    private void OnDrawGizmos()
    {
        if (enemy == null)
            enemy = GetComponent<BaseEnemy>();

        DrawAttackGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (enemy == null)
            enemy = GetComponent<BaseEnemy>();

        // Draw more detailed gizmos when selected
        DrawAttackGizmos(true);
    }

    private void DrawAttackGizmos(bool selected = false)
    {
        if (enemy == null || enemy.AvailableAttacks == null)
            return;

        // Draw gizmos for each attack
        foreach (Attack attack in enemy.AvailableAttacks)
        {
            if (attack is MeleeAttack meleeAttack)
            {
                // Draw melee attack gizmos
                meleeAttack.DrawGizmosForEnemy(enemy);
            }
            else if (selected)
            {
                // Draw basic attack range for other attack types when selected
                Gizmos.color = new Color(0, 0.8f, 1f, 0.3f);
                Vector3 origin = enemy.firePoint != null ? enemy.firePoint.position : enemy.transform.position;
                Gizmos.DrawWireSphere(origin, attack.Range);
            }
        }
    }
}
