// Ignore Spelling: Melee
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Attack", menuName = "Enemy/Attacks/Melee Attack")]
public class MeleeAttack : Attack
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackArc = 90f;
    [SerializeField] private LayerMask targetLayers;

    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target)
    {
        // Begin attack sequence
        enemy.SetIsAttacking(true);
        if(enemy.Animator != null)
            enemy.Animator.SetTrigger(animationTrigger);
        Coroutine trackingCoroutine = enemy.StartCoroutine(TrackTarget(enemy, target));

        // Wait for wind-up
        yield return new WaitForSeconds(attackDelay);
        // Stop tracking once the delay is complete
        if (trackingCoroutine != null)
        {
            enemy.StopCoroutine(trackingCoroutine);
        }
        // Perform the attack
        Vector3 attackOrigin = enemy.firePoint.position;
        Collider[] hits = Physics.OverlapSphere(attackOrigin, range, targetLayers);

        foreach (Collider hit in hits)
        {
            // Check if target is within attack arc
            Vector3 directionToTarget = (hit.transform.position - attackOrigin).normalized;
            float angle = Vector3.Angle(enemy.transform.forward, directionToTarget);

            if (angle <= attackArc * 0.5f)
            {
                if (hit.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                    SpawnVFX(hit.transform.position, enemy.transform.rotation);

                }
            }
        }

        PlaySFX(enemy.AudioSource);
        StartCooldown();
        //Debug.Log("Melee Attack!!");
        // Recovery period
        yield return new WaitForSeconds(recoveryTime);
        enemy.SetIsAttacking(false);
    }

    // track the target during attack charge-up
    private IEnumerator TrackTarget(BaseEnemy enemy, Transform target)
    {
        while (target != null)
        {
            // Smoothly rotate to face the target
            Vector3 directionToTarget = (target.position - enemy.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);

            yield return null;
        }
    }
}
