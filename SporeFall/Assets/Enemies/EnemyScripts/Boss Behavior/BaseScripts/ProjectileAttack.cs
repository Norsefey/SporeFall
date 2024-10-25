using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Attack", menuName = "Boss/Attacks/Projectile Attack")]
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

    public override IEnumerator ExecuteAttack(BaseBoss boss, Transform target)
    {
        boss.SetIsAttacking(true);
        if(boss.Animator != null)
            boss.Animator.SetTrigger(animationTrigger);

        yield return new WaitForSeconds(attackDelay);

        if (target != null)
        {
            Vector3 spawnPosition = boss.firePoint.position + boss.transform.forward;
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

                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
                ProjectileBehavior projectileComp = projectile.GetComponent<ProjectileBehavior>();

                if (projectileComp != null)
                {
                    ProjectileData data = new ProjectileData
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

                    projectileComp.Initialize(data);
                }
            }
        }

        SpawnVFX(boss.transform.position, boss.transform.rotation);
        PlaySFX(boss.AudioSource);
        StartCooldown();

        yield return new WaitForSeconds(recoveryTime);
        boss.SetIsAttacking(false);
    }
}

