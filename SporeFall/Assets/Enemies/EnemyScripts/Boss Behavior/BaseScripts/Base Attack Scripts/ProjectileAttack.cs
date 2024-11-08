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

    [Header("Pool Settings")]
    [SerializeField] protected int initialPoolSize = 20;
    protected ProjectilePool projectilePool;



    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target)
    {
        enemy.SetIsAttacking(true);
        if(enemy.Animator != null)
            enemy.Animator.SetTrigger(animationTrigger);

        yield return new WaitForSeconds(attackDelay);

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

                if(projectilePool == null)
                {
                    // Create a parent object for the pool
                    GameObject poolParent = new GameObject($"Pool_{projectilePrefab.name}");
                    poolParent.transform.SetParent(enemy.transform);

                    // Initialize the projectile pool
                    if (projectilePrefab != null)
                    {
                        projectilePool = new ProjectilePool(projectilePrefab, poolParent.transform, initialPoolSize);
                    }
                }
                // Get projectile from pool
                ProjectileBehavior projectile = projectilePool.Get(
                spawnPosition,
                    Quaternion.LookRotation(direction));                
                if (projectile.TryGetComponent<ProjectileBehavior>(out var projectileComp))
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

                    projectileComp.Initialize(data, projectilePool);
                }
            }
        }

        SpawnVFX(enemy.firePoint.position, enemy.transform.rotation);
        PlaySFX(enemy.AudioSource);
        StartCooldown();

        yield return new WaitForSeconds(recoveryTime);
        enemy.SetIsAttacking(false);
    }
}

