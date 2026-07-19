using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "New AOE Attack", menuName = "Enemy/Attacks/AOE Attack")]
public class AoeAttack : Attack
{
    public override AttackType AttackType => AttackType.AOE;

    [Header("AOE Attack Settings")]
    [SerializeField] private float aoeRadius = 5f;
    [SerializeField] private float damageMultiplierAtCenter = 1.5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool damageOverTime = false;
    [SerializeField] private float dotDuration = 3f;
    [SerializeField] private float dotTickRate = 0.5f;

    public override void Execute(AttackInstance instance, Damageable target)
    {
        Vector3 aoeCenter = instance.Owner.transform.position;
        Collider[] hits = Physics.OverlapSphere(aoeCenter, aoeRadius, targetLayers);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Damageable>(out var damageable))
            {
                float distanceFromCenter = Vector3.Distance(aoeCenter, hit.transform.position);
                float damageMultiplier = Mathf.Lerp(damageMultiplierAtCenter, 1f, distanceFromCenter / aoeRadius);

                damageable.ReceiveDamage(instance.ScaledDamage);
            }
        }

        SpawnVFX(instance.Owner.transform.position, instance.Owner.transform.rotation);
        PlaySFX(instance.Owner.AudioSource);
    }

    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target, float damageModifier, float corruptionModifier)
    {
        throw new System.NotImplementedException();

        /*  float finalDamage = (baseDamage * damageModifier) + Random.Range(-damageVariance, damageVariance);
          float finalCorruption = (baseCorruption * corruptionModifier) + Random.Range(-corruptionVariance, corruptionVariance);

          enemy.SetIsAttacking(true);
          if (enemy.Animator != null)
              enemy.Animator.SetTrigger(animationTrigger);

          yield return new WaitForSeconds(attackDelay);

          Vector3 aoeCenter = enemy.transform.position;
          Collider[] hits = Physics.OverlapSphere(aoeCenter, aoeRadius, targetLayers);

          foreach (Collider hit in hits)
          {
              if (hit.TryGetComponent<Damageable>(out var damageable))
              {
                  float distanceFromCenter = Vector3.Distance(aoeCenter, hit.transform.position);
                  float damageMultiplier = Mathf.Lerp(damageMultiplierAtCenter, 1f, distanceFromCenter / aoeRadius);

                  if (damageOverTime)
                  {
                      enemy.StartDOTEffect(damageable, finalDamage * damageMultiplier, dotDuration, dotTickRate);
                  }
                  else
                  {
                      damageable.ReceiveDamage(finalDamage * damageMultiplier);
                  }
              }
          }

          SpawnVFX(aoeCenter, enemy.transform.rotation);
          PlaySFX(enemy.AudioSource);

          yield return new WaitForSeconds(recoveryTime);
          enemy.SetIsAttacking(false);*/
    }
}
