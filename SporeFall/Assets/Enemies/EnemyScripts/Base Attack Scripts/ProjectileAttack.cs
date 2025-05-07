using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileTrajectoryType
{
    Standard,
    Arc,
    RandomDirections
}
[CreateAssetMenu(fileName = "New Projectile Attack", menuName = "Enemy/Attacks/Projectile Attack")]
public class ProjectileAttack : RangedAttack
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private bool useGravity = false;

    [Header("Trajectory Settings")]
    [SerializeField] private ProjectileTrajectoryType trajectoryType = ProjectileTrajectoryType.Standard;
    [SerializeField] private float maxArchHeight, minArchHeight;
    private float projectileArcHeight = 2f; // For arcing projectiles

    [SerializeField] private float randomDirectionConeAngle = 45f; // For random directions (cone spread)

    [SerializeField] private bool canBounce = false;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float bounceDamageMultiplier = 0.7f; // Reduce damage with each bounce

    [Header("Multiple Projectile Settings")]
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float timeBetweenProjectiles = 0.1f; // Delay between each projectile
    [SerializeField] private float spreadAngle = 0f;
    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target)
    {
        enemy.SetIsAttacking(true);

        if (enemy.Animator != null)
            enemy.Animator.SetTrigger(animationTrigger);

        Coroutine trackingCoroutine = enemy.StartCoroutine(TrackTarget(enemy, target));
        yield return new WaitForSeconds(attackDelay);

        if (target != null)
            enemy.StartCoroutine(FireProjectiles(enemy, target));

        yield return new WaitForSeconds(timeBetweenProjectiles * projectileCount);

        if (trackingCoroutine != null)
            enemy.StopCoroutine(trackingCoroutine);

        SpawnVFX(enemy.firePoint.position, enemy.transform.rotation);
        PlaySFX(enemy.AudioSource);
        StartCooldown();

        yield return new WaitForSeconds(recoveryTime);
        enemy.SetIsAttacking(false);
    }
    private IEnumerator FireProjectiles(BaseEnemy enemy, Transform target)
    {

        float startAngle = -spreadAngle * 0.5f;
        float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0;

        for (int i = 0; i < projectileCount; i++)
        {
            if (target == null) yield break;
            Vector3 spawnPosition = enemy.firePoint.position + enemy.transform.forward;

            // Recalculate direction and position each time
            Vector3 dynamicTargetPosition = GetPredictedTargetPosition(target, spawnPosition);
            Vector3 baseDirection = (dynamicTargetPosition - spawnPosition).normalized;

            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = CalculateDirection(baseDirection, currentAngle, spawnPosition, dynamicTargetPosition, out Vector3 adjustedTarget);

            SpawnProjectile(spawnPosition, direction, adjustedTarget);

            if (timeBetweenProjectiles > 0 && i < projectileCount - 1)
                yield return new WaitForSeconds(timeBetweenProjectiles);
        }
    }
    private Vector3 CalculateDirection(Vector3 baseDirection, float currentAngle, Vector3 spawnPosition, Vector3 targetPosition, out Vector3 adjustedTarget)
    {
        Vector3 direction = baseDirection;
        adjustedTarget = targetPosition;

        switch (trajectoryType)
        {
            case ProjectileTrajectoryType.Standard:
            case ProjectileTrajectoryType.Arc:
                direction = Quaternion.Euler(0, currentAngle, 0) * baseDirection;
                float distance = Vector3.Distance(spawnPosition, targetPosition);
                adjustedTarget = spawnPosition + direction * distance;
                if (trajectoryType == ProjectileTrajectoryType.Arc) useGravity = true;
                break;

            case ProjectileTrajectoryType.RandomDirections:
                direction = Random.insideUnitSphere;
                direction.y = Mathf.Abs(direction.y);
                direction = Vector3.Slerp(baseDirection, direction.normalized, randomDirectionConeAngle / 180f);
                adjustedTarget = spawnPosition + direction * Vector3.Distance(spawnPosition, targetPosition);
                break;
        }

        return direction.normalized;
    }
    private void SpawnProjectile(Vector3 spawnPosition, Vector3 direction, Vector3 targetPosition)
    {
        projectileArcHeight = Random.Range(minArchHeight, maxArchHeight);

        float arcHeight = trajectoryType == ProjectileTrajectoryType.Arc ? projectileArcHeight : 0f;

        BaseProjectile projectile = null;
        if (PoolManager.Instance != null &&
            PoolManager.Instance.projectilePool.TryGetValue(projectilePrefab, out ProjectilePool pool))
        {
            projectile = pool.Get(spawnPosition, Quaternion.LookRotation(direction));
            projectile?.Initialize(CreateProjectileData(direction, targetPosition, arcHeight), pool);
        }
        else
        {
            projectile = GameObject.Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction)).GetComponent<BaseProjectile>();
            projectile?.Initialize(CreateProjectileData(direction, targetPosition, arcHeight), null);
        }
    }
    private ProjectileData CreateProjectileData(Vector3 direction, Vector3 target, float arcHeight)
    {
        return new ProjectileData
        {
            Direction = direction,
            Speed = projectileSpeed,
            Damage = damage,
            Lifetime = projectileLifetime,
            UseArcTrajectory = trajectoryType == ProjectileTrajectoryType.Arc,
            UseGravity = useGravity,
            ArcHeight = arcHeight,
            CanBounce = canBounce,
            MaxBounces = maxBounces,
            BounceDamageMultiplier = bounceDamageMultiplier,
            TargetPosition = target,
        };
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

