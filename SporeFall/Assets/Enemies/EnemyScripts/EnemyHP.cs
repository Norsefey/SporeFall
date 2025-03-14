using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private BaseEnemy manager;
    public bool knockBackable = true;
    private bool beingKnockedBack = false;

    private void Start()
    {
        currentHP = maxHP;
    }
    public override void TakeDamage(float damage)
    {
        Debug.Log("Base took damage");
        base.TakeDamage(damage);

        if (manager != null)
        {
            manager.CheckDamageThreshold(maxHP - currentHP);
            manager.recentDamage.Enqueue(new BaseEnemy.DamageInstance(damage, Time.time));
        }
    }
    protected override void Die()
    {
        // call death on enemy
        if (manager != null)
        {
            StopAllCoroutines();
            manager.Die();
        }
        else
            Destroy(transform.parent.gameObject);
    }
    public IEnumerator KnockBack(Vector3 attackerPosition, float knockbackMultiplier)
    {
        if(!knockBackable || beingKnockedBack)
            yield break;

        beingKnockedBack = true;
        // Calculate knockback direction away from the attacker
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;

        // Disable NavMeshAgent during knockback
        manager.agent.isStopped = true;

        // Apply knockback force
        float elapsedTime = 0f;
        while (elapsedTime < .5f)
        {
            
            // Move the agent manually
            manager.agent.transform.position += knockbackDirection * 1 * knockbackMultiplier * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        beingKnockedBack = false;
        // Re-enable NavMeshAgent
        manager.agent.isStopped = false;
        manager.agent.ResetPath();
    }
}
