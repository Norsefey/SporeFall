using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hitscan Attack", menuName = "Boss/Attacks/Hitscan Attack")]
public class HitscanAttack : RangedAttack
{
    
        [Header("Hitscan Settings")]
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool penetratingShot = false;
        [SerializeField] private int maxPenetrations = 3;
        [SerializeField] private float penetrationDamageMultiplier = 0.7f;

        [Header("Multiple Shot Settings")]
        [SerializeField] private int shotCount = 1;
        [SerializeField] private float spreadAngle = 0f;

        [Header("Beam Effects")]
        [SerializeField] private GameObject beamVFXPrefab;
        [SerializeField] private float beamDuration = 0.1f;


    public override IEnumerator ExecuteAttack(BaseBoss boss, Transform target)
    {
        boss.SetIsAttacking(true);
        if (boss.Animator != null)
            boss.Animator.SetTrigger(animationTrigger);

        yield return new WaitForSeconds(attackDelay);

        if (target != null)
        {
            Vector3 attackOrigin = boss.transform.position + boss.transform.forward + Vector3.up;
            Vector3 targetPosition = GetPredictedTargetPosition(target, attackOrigin);
            Vector3 baseDirection = (targetPosition - attackOrigin).normalized;

            // Calculate spread angles for multiple shots
            float startAngle = -spreadAngle * 0.5f;
            float angleStep = shotCount > 1 ? spreadAngle / (shotCount - 1) : 0;

            for (int i = 0; i < shotCount; i++)
            {
                Vector3 direction = baseDirection;
                if (spreadAngle > 0)
                {
                    float currentAngle = startAngle + (angleStep * i);
                    direction = Quaternion.Euler(0, currentAngle, 0) * baseDirection;
                }

                if (penetratingShot)
                {
                    FirePenetratingRay(attackOrigin, direction, boss);
                }
                else
                {
                    FireSingleRay(attackOrigin, direction, boss);
                }
            }
        }

        SpawnVFX(boss.transform.position, boss.transform.rotation);
        PlaySFX(boss.AudioSource);
        StartCooldown();

        yield return new WaitForSeconds(recoveryTime);
        boss.SetIsAttacking(false);
    }
    private void FireSingleRay(Vector3 origin, Vector3 direction, BaseBoss boss)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, targetLayers))
        {
            if (hit.collider.TryGetComponent<Damageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }

            SpawnBeamEffect(origin, hit.point);
        }
        else
        {
            SpawnBeamEffect(origin, origin + direction * range);
        }
    }

    private void FirePenetratingRay(Vector3 origin, Vector3 direction, BaseBoss boss)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, range, targetLayers)
            .OrderBy(h => h.distance)
            .ToArray();

        Vector3 lastHitPoint = origin;
        float currentDamage = damage;
        int penetrations = 0;

        foreach (RaycastHit hit in hits)
        {
            if (penetrations >= maxPenetrations) break;

            if (hit.collider.TryGetComponent<Damageable>(out var damageable))
            {
                damageable.TakeDamage(currentDamage);
                currentDamage *= penetrationDamageMultiplier;
                penetrations++;
            }

            lastHitPoint = hit.point;
        }

        SpawnBeamEffect(origin, lastHitPoint);
    }
    private void SpawnBeamEffect(Vector3 start, Vector3 end)
    {
        if (beamVFXPrefab != null)
        {
            GameObject beam = Instantiate(beamVFXPrefab);
            
            if (beam.TryGetComponent<LineRenderer>(out var lineRenderer))
            {
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
                Destroy(beam, beamDuration);
            }
        }
    }
}
