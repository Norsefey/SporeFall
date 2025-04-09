using System.Collections;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private BaseEnemy manager;
    [SerializeField] private float flinchModifier = 1.0f;
    public bool flinchable = true;
    private bool flinching = false;

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

            if (flinchable)
            {
                // Calculate flinch probability (higher HP = higher chance to flinch)
                float healthRatio = currentHP / maxHP; // 1 at full HP, 0 at 0 HP
                float flinchChance = healthRatio * flinchModifier; // Apply the modifier

                flinchChance = Mathf.Clamp01(flinchChance);

                if (!flinching && Random.value < flinchChance)
                {
                    StartCoroutine(Flinch());
                }
            }
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
    public IEnumerator Flinch()
    {
        manager.Animator.SetTrigger("Flinch");

        if (!flinchable || flinching)
            yield break;

        Debug.Log("Flinching");
        flinching = true;

        // Disable NavMeshAgent during knockback
        manager.SetState(EnemyState.Idle);

        yield return new WaitForSeconds(1);

        // Re-enable NavMeshAgent
        manager.SetState(EnemyState.Chase);
        /* manager.agent.isStopped = false;
         manager.agent.ResetPath();*/
        manager.Animator.ResetTrigger("Flinch");
        flinching = false;
    }
}
