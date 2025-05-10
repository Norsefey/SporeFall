using System.Collections;
using UnityEngine;

public class EnemyHP : Damageable
{
    // Health properties
    [SerializeField] private BaseEnemy manager;
    [SerializeField] private float flinchModifier = 1.0f;
    public bool flinchable = true;
    private bool isFlinching = false;

    private void Awake()
    {
        if(manager == null)
            manager = transform.parent.GetComponent<BaseEnemy>();

        if (manager == null)
            Debug.LogError(transform.parent.name + " Main HP has no Manager Reference");
        currentHP = maxHP;
    }
    public override void TakeDamage(float damage)
    {
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

                if (isActiveAndEnabled && !isFlinching && Random.value < flinchChance)
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

        if (!flinchable || isFlinching || manager.Animator == null)
            yield break;

        isFlinching = true;

        if (manager.IsAttacking)
            manager.CancelAttack();

        manager.Animator.SetTrigger("Flinch");
        manager.SetState(EnemyState.Idle);

        yield return new WaitForSeconds(1);

        // Re-enable NavMeshAgent
        manager.SetState(EnemyState.Chase);
        manager.Animator.ResetTrigger("Flinch");
        isFlinching = false;
    }
}
