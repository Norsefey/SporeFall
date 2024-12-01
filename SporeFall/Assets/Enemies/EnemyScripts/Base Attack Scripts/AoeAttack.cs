using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New AOE Attack", menuName = "Enemy/Attacks/AOE Attack")]
public class AoeAttack : Attack
{
    [Header("AOE Attack Settings")]
    [SerializeField] private float aoeRadius = 5f;
    [SerializeField] private float damageMultiplierAtCenter = 1.5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool damageOverTime = false;
    [SerializeField] private float dotDuration = 3f;
    [SerializeField] private float dotTickRate = 0.5f;

    public override IEnumerator ExecuteAttack(BaseEnemy boss, Transform target)
    {
        boss.SetIsAttacking(true);
        if (boss.Animator != null)
            boss.Animator.SetTrigger(animationTrigger);

        yield return new WaitForSeconds(attackDelay);

        Vector3 aoeCenter = boss.transform.position;
        Collider[] hits = Physics.OverlapSphere(aoeCenter, aoeRadius, targetLayers);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Damageable>(out var damageable))
            {
                float distanceFromCenter = Vector3.Distance(aoeCenter, hit.transform.position);
                float damageMultiplier = Mathf.Lerp(damageMultiplierAtCenter, 1f, distanceFromCenter / aoeRadius);

                if (damageOverTime)
                {
                    boss.StartDOTEffect(damageable, damage * damageMultiplier, dotDuration, dotTickRate);
                }
                else
                {
                    damageable.TakeDamage(damage * damageMultiplier);
                }
            }
        }

        SpawnVFX(aoeCenter, boss.transform.rotation);
        PlaySFX(boss.AudioSource);
        StartCooldown();

        yield return new WaitForSeconds(recoveryTime);
        boss.SetIsAttacking(false);
    }

    private IEnumerator ApplyDOTDamage(Damageable target, float totalDamage)
    {
        float elapsedTime = 0f;
        float damagePerTick = totalDamage * (dotTickRate / dotDuration);

        while (elapsedTime < dotDuration)
        {
            target.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(dotTickRate);
            elapsedTime += dotTickRate;
        }
    }
}
