using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hitscan Attack", menuName = "Enemy/Attacks/Hitscan Attack")]
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


    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target, float damageModifier)
    {
        damage *= damageModifier;

        enemy.SetIsAttacking(true);
        if (enemy.Animator != null)
            enemy.Animator.SetTrigger(animationTrigger);
        SpawnVFX(enemy.firePoint.position, enemy.transform.rotation);

        Coroutine trackingCoroutine = enemy.StartCoroutine(TrackTarget(enemy, target));

        yield return new WaitForSeconds(attackDelay);

        // Stop tracking once the delay is complete
        if (trackingCoroutine != null)
        {
            enemy.StopCoroutine(trackingCoroutine);
        }

        if (target != null)
        {
            Vector3 attackOrigin = enemy.firePoint.position;
            Vector3 targetPosition = GetPredictedTargetPosition(target, attackOrigin);
            Vector3 baseDirection = (targetPosition - attackOrigin).normalized;
            yield return new WaitForSeconds(.05f);
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
                    FirePenetratingRay(attackOrigin, direction, enemy);
                }
                else
                {
                    FireSingleRay(attackOrigin, direction, enemy);
                }
            }
        }

        PlaySFX(enemy.AudioSource);
        StartCooldown();

        yield return new WaitForSeconds(recoveryTime);
        enemy.SetIsAttacking(false);
    }
    private void FireSingleRay(Vector3 origin, Vector3 direction, BaseEnemy boss)
    {
        // for a single target ray we only want the layer that target is on
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
            Debug.Log("NO Hit Beam");
            SpawnBeamEffect(origin, origin + direction * range);
        }
    }
    private void FirePenetratingRay(Vector3 origin, Vector3 direction, BaseEnemy boss)
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
