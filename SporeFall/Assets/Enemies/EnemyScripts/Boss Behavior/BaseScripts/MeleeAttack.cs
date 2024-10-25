// Ignore Spelling: Melee
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Attack", menuName = "Boss/Attacks/Melee Attack")]
public class MeleeAttack : Attack
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackArc = 90f;
    [SerializeField] private LayerMask targetLayers;

    public override IEnumerator ExecuteAttack(BaseBoss boss, Transform target)
    {
        // Begin attack sequence
        boss.SetIsAttacking(true);
        if(boss.Animator != null)
            boss.Animator.SetTrigger(animationTrigger);

        // Wait for wind-up
        yield return new WaitForSeconds(attackDelay);

        // Perform the attack
        Vector3 attackOrigin = boss.transform.position;
        Collider[] hits = Physics.OverlapSphere(attackOrigin, range, targetLayers);

        foreach (Collider hit in hits)
        {
            // Check if target is within attack arc
            Vector3 directionToTarget = (hit.transform.position - attackOrigin).normalized;
            float angle = Vector3.Angle(boss.transform.forward, directionToTarget);

            if (angle <= attackArc * 0.5f)
            {
                if (hit.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }
            }
        }

        SpawnVFX(attackOrigin, boss.transform.rotation);
        PlaySFX(boss.AudioSource);
        StartCooldown();

        // Recovery period
        yield return new WaitForSeconds(recoveryTime);
        boss.SetIsAttacking(false);
    }
}
