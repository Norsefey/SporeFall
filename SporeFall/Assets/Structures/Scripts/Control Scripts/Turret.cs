using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Turret : MonoBehaviour
{
    // These public variables are set by TurretBehavior
    [HideInInspector]
    public float 
        detectionRange, 
        rotationSpeed, 
        fireRate, 
        fireRange 
        ;
    //[HideInInspector]
    public ProjectileData bulletData;
    [Header("Turret Settings")]
    [SerializeField] private float minimumFireRange = 1f; // Minimum distance to target

    [Header("References")]
    [SerializeField] private Transform turretGuns;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Audio")]
    [SerializeField] private AudioClip firingSound;
    [Range(0f, 1f)]
    [SerializeField] private float flameVolume = 0.5f;

    // Private variables
    private Transform targetEnemy;
    private AudioSource audioSource;
    private bool hasTarget;
    private float lastFireTime; // Track the last time we fired

    public bool showFireDebug = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = firingSound;
        audioSource.volume = flameVolume;
    }

    void Update()
    {
        if (!hasTarget || !IsTargetValid())
        {
            FindTarget();
        }

        if (hasTarget && Time.timeScale == 1)
        {
            TrackTarget();
            TryShoot();
        }
        else
        {
            //canShoot = false;
        }
    }

    // Find the closest enemy within range
    private void FindTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRange, enemyLayerMask);

        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            // Skip null or invalid colliders
            if (enemyCollider == null || enemyCollider.transform == null || enemyCollider.CompareTag("HeadShot"))
                continue;

            // Get the closest point on the enemy collider to the turret
            Vector3 closestPoint = enemyCollider.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, closestPoint);

            // Check if the enemy is within valid firing range
            if (distance >= minimumFireRange && distance <= fireRange && distance < closestDistance)
            {
                // Check line of sight to the closest point
                Vector3 directionToTarget = (closestPoint - transform.position).normalized;
                if (!Physics.Raycast(transform.position, directionToTarget, distance, obstructionMask))
                {
                    closestDistance = distance;
                    closestEnemy = enemyCollider.transform;

                    if (showFireDebug)
                    {
                        Debug.DrawLine(transform.position, closestPoint, Color.green, 0.1f);
                        Debug.Log($"Valid target: {enemyCollider.name}, Distance: {distance:F2}");
                    }
                }
                else if (showFireDebug)
                {
                    Debug.DrawLine(transform.position, closestPoint, Color.yellow, 0.1f);
                    Debug.Log($"Target obstructed: {enemyCollider.name}");
                }
            }
            else if (showFireDebug)
            {
                Debug.DrawLine(transform.position, closestPoint, Color.red, 0.1f);
                Debug.Log($"Target out of range: {enemyCollider.name}, Distance: {distance:F2}");
            }
        }

        targetEnemy = closestEnemy;
        hasTarget = targetEnemy != null;

        if (hasTarget && showFireDebug)
            Debug.Log($"New target acquired: {targetEnemy.name}");
    }
    // Check if the current enemy is still within detection range and valid
    private bool IsTargetValid()
    {
        if (targetEnemy == null)
        {
            if (showFireDebug)
                Debug.Log("Target is null");
            return false;
        }

        EnemyHPRelay hpRelay = targetEnemy.GetComponent<EnemyHPRelay>();
        if (hpRelay == null || hpRelay.IsDead())
        {
            if (showFireDebug)
                Debug.Log($"Target {targetEnemy.name} is no longer valid (missing HP relay or dead)");
            return false;
        }

        // Get collider component from the target
        if (!targetEnemy.TryGetComponent<Collider>(out var targetCollider))
        {
            targetCollider = targetEnemy.GetComponentInChildren<Collider>();
            if (targetCollider == null)
            {
                if (showFireDebug)
                    Debug.Log($"Target {targetEnemy.name} has no collider");
                return false;
            }
        }

        // Get closest point on the collider to measure accurate distance
        Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
        float distance = Vector3.Distance(transform.position, closestPoint);

        // Check if target is within valid firing range
        bool isInRange = distance >= minimumFireRange && distance <= fireRange;

        if (!isInRange)
        {
            if (showFireDebug)
                Debug.Log($"Target {targetEnemy.name} out of range: {distance:F2}");
            return false;
        }

        // Check line of sight to the closest point
        Vector3 directionToTarget = (closestPoint - transform.position).normalized;
        bool lineOfSight = !Physics.Raycast(transform.position, directionToTarget, distance, obstructionMask);

        if (showFireDebug)
        {
            if (lineOfSight)
                Debug.DrawLine(transform.position, closestPoint, Color.green, 0.1f);
            else
                Debug.DrawLine(transform.position, closestPoint, Color.yellow, 0.1f);

            Debug.Log($"Target {targetEnemy.name} visibility check: {(lineOfSight ? "Visible" : "Obstructed")}");
        }

        return isInRange && lineOfSight;
    }

    // Rotate the turret smoothly towards the nearest enemy (only on the y-axis)
    private void TrackTarget()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
        {
            hasTarget = false;
            return;
        }

        // Get the collider component from the target
        if (!targetEnemy.TryGetComponent<Collider>(out var targetCollider))
        {
            targetCollider = targetEnemy.GetComponentInChildren<Collider>();
            if (targetCollider == null)
            {
                hasTarget = false;
                return;
            }
        }

        // Find the closest point on the target collider
        Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
        Vector3 targetDirection = closestPoint - transform.position;

        // Only rotate if we have a valid direction
        if (targetDirection.magnitude > 0.1f)
        {
            // Create the target rotation based on the direction to the closest point
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Smoothly rotate towards the target point
            turretGuns.rotation = Quaternion.Slerp(
                turretGuns.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (showFireDebug)
            {
                Debug.DrawLine(firePoint.position, closestPoint, Color.blue, 0.1f);
                Debug.DrawRay(firePoint.position, firePoint.forward * 2f, Color.magenta, 0.1f);
            }
        }
    }

    // Check line of sight using raycasting and fire at the enemy if visible
    private void TryShoot()
    {
        // Early return if no target
        if (targetEnemy == null)
        {
            return;
        }

        // Respect fire rate limit
        float currentTime = Time.unscaledTime;
        float timeSinceLastFire = currentTime - lastFireTime;
        if (timeSinceLastFire < (1f / fireRate))
        {
            return;
        }

        // Get the collider component from the target
        if (!targetEnemy.TryGetComponent<Collider>(out var targetCollider))
        {
            targetCollider = targetEnemy.GetComponentInChildren<Collider>();
            if (targetCollider == null)
            {
                return;
            }
        }

        // Find the closest point on the target collider
        Vector3 closestPoint = targetCollider.ClosestPoint(firePoint.position);
        float distanceToTarget = Vector3.Distance(firePoint.position, closestPoint);

        // Check if target is within valid firing range
        if (distanceToTarget < minimumFireRange || distanceToTarget > fireRange)
        {
            if (showFireDebug)
            {
                Debug.DrawLine(firePoint.position, closestPoint, Color.red, 0.1f);
                Debug.Log($"Can't shoot: Target out of range ({distanceToTarget:F2})");
            }
            return;
        }

        // Calculate direction to the closest point
        Vector3 directionToTarget = (closestPoint - firePoint.position).normalized;

/*        // Check alignment - don't shoot unless turret is facing the target
        float aimAccuracy = Vector3.Dot(firePoint.forward, directionToTarget);
        if (aimAccuracy < 0.50f) // About 18 degrees off-center
        {
            if (showFireDebug)
            {
                Debug.DrawRay(firePoint.position, firePoint.forward * distanceToTarget, Color.yellow, 0.1f);
                Debug.DrawRay(firePoint.position, directionToTarget * distanceToTarget, Color.cyan, 0.1f);
                Debug.Log($"Can't shoot: Turret not aligned with target (accuracy: {aimAccuracy:F2})");
            }
            return;
        }*/

        // Perform final raycast to ensure nothing is blocking the shot
        if (showFireDebug)
        {
            Debug.DrawRay(firePoint.position, directionToTarget * distanceToTarget, Color.green, 0.1f);
        }

        if (Physics.Raycast(firePoint.position, directionToTarget, out RaycastHit hit, distanceToTarget))
        {
            // Check if we hit the target or something else
            if (hit.collider != targetCollider && !hit.transform.IsChildOf(targetEnemy.transform) &&
                !targetEnemy.transform.IsChildOf(hit.transform))
            {
                if (showFireDebug)
                {
                    Debug.Log($"Can't shoot: Hit something else ({hit.transform.name})");
                    Debug.DrawLine(firePoint.position, hit.point, Color.red, 0.1f);
                }
                return;
            }

            // All checks passed, we can shoot
            Shoot();
            lastFireTime = currentTime;

            if (showFireDebug)
            {
                Debug.Log($"Shot fired at {targetEnemy.name}!");
                Debug.DrawLine(firePoint.position, hit.point, Color.green, 0.2f);
            }
        }
    }

    // Fire a bullet towards the enemy and play the firing sound
    private void Shoot()
    {
        // Play firing sound if available
        if (audioSource != null && firingSound != null)
        {
            audioSource.PlayOneShot(firingSound);
        }

        // Get target's collider for aiming
        if (!targetEnemy.TryGetComponent<Collider>(out var targetCollider))
        {
            targetCollider = targetEnemy.GetComponentInChildren<Collider>();
        }

        // Get aim direction - by default use firePoint's forward direction
        Vector3 aimDirection = firePoint.forward;

        // If we have a valid collider, aim at the closest point for better accuracy
        if (targetCollider != null)
        {
            Vector3 closestPoint = targetCollider.ClosestPoint(firePoint.position);
            aimDirection = (closestPoint - firePoint.position).normalized;
        }

        // Use object pooling if available
        if (PoolManager.Instance != null)
        {
            if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out ProjectilePool pool))
            {
                if (showFireDebug)
                    Debug.LogWarning($"No projectile pool found for {bulletPrefab.name}");
                return;
            }
            else
            {
                BaseProjectile projectile = pool.Get(firePoint.position, Quaternion.LookRotation(aimDirection));

                if (projectile != null)
                {
                    bulletData.Direction = aimDirection;
                    projectile.Initialize(bulletData, pool);

                    if (showFireDebug)
                        Debug.Log($"Fired pooled projectile in direction {aimDirection}");
                }
            }
        }
        // Otherwise instantiate new projectile
        else
        {
            BaseProjectile projectile = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.LookRotation(aimDirection)
            ).GetComponent<BaseProjectile>();

            bulletData.Direction = aimDirection;
            projectile.Initialize(bulletData, null);

            if (showFireDebug)
                Debug.Log($"Fired instantiated projectile in direction {aimDirection}");
        }

    }

}
