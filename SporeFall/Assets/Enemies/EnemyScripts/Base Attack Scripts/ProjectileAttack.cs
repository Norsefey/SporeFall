using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Projectile Attack", menuName = "Enemy/Attacks/Projectile Attack")]
public class ProjectileAttack : RangedAttack
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float projectileArcHeight = 0f; // For arcing projectiles
    [SerializeField] private bool canBounce = false;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float bounceDamageMultiplier = 0.7f; // Reduce damage with each bounce

    [Header("Multiple Projectile Settings")]
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 0f;

    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target)
    {
        enemy.SetIsAttacking(true);
        if(enemy.Animator != null)
            enemy.Animator.SetTrigger(animationTrigger);

        Coroutine trackingCoroutine = enemy.StartCoroutine(TrackTarget(enemy, target));

        yield return new WaitForSeconds(attackDelay);

        // Stop tracking once the delay is complete
        if (trackingCoroutine != null)
        {
            enemy.StopCoroutine(trackingCoroutine);
        }

        if (target != null)
        {
            Vector3 spawnPosition = enemy.firePoint.position + enemy.transform.forward;
            Vector3 targetPosition = GetPredictedTargetPosition(target, spawnPosition);

            // Calculate spread angles for multiple projectiles
            float startAngle = -spreadAngle * 0.5f;
            float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0;

            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 direction = (targetPosition - spawnPosition).normalized;
                if (spreadAngle > 0)
                {
                    float currentAngle = startAngle + (angleStep * i);
                    direction = Quaternion.Euler(0, currentAngle, 0) * direction;
                }

                ProjectileBehavior projectile = null;
                if (PoolManager.Instance != null)
                {
                    if (!PoolManager.Instance.projectilePool.TryGetValue(projectilePrefab, out ProjectilePool pool))
                    {
                        Debug.LogError($"No pool found for enemy prefab: {projectilePrefab.name}");

                        yield return null;
                    }

                     projectile = pool.Get(spawnPosition, Quaternion.LookRotation(direction));

                    if (projectile)
                    {
                        ProjectileData data = new()
                        {
                            Direction = direction,
                            Speed = projectileSpeed,
                            Damage = damage,
                            Lifetime = projectileLifetime,
                            UseGravity = useGravity,
                            ArcHeight = projectileArcHeight,
                            CanBounce = canBounce,
                            MaxBounces = maxBounces,
                            BounceDamageMultiplier = bounceDamageMultiplier
                        };

                        projectile.Initialize(data, pool);
                    }
                }
                else
                {
                    // If Pool Is missing Simply Spawn A projectile
                    projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction)).GetComponent<ProjectileBehavior>();

                    ProjectileData data = new()
                    {
                        Direction = direction,
                        Speed = projectileSpeed,
                        Damage = damage,
                        Lifetime = projectileLifetime,
                        UseGravity = useGravity,
                        ArcHeight = projectileArcHeight,
                        CanBounce = canBounce,
                        MaxBounces = maxBounces,
                        BounceDamageMultiplier = bounceDamageMultiplier
                    };
                    projectile.Initialize(data, null);
                }
            }
        }

        SpawnVFX(enemy.firePoint.position, enemy.transform.rotation);
        PlaySFX(enemy.AudioSource);
        StartCooldown();

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

