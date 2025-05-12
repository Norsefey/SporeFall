using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortyControlScript : MonoBehaviour
{
    // These public variables are set by TurretBehavior
    //[HideInInspector]
    public float
        detectionRange,
        fireRate,
        fireRange;
    //[HideInInspector]
    public ProjectileData bulletData;

    [Header("References")]
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
            TryShoot();
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
            if (distance <= fireRange && distance < closestDistance)
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

    // Check if the current enemy is within detection range
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
        bool isInRange = distance <= fireRange;

        if (!isInRange)
        {
            if (showFireDebug)
                Debug.Log($"Target {targetEnemy.name} out of range: {distance:F2}");
            return false;
        }

        return isInRange;
    }

    // Check line of sight using raycasting and fire at the enemy if visible
    private void TryShoot()
    {
        if (targetEnemy == null)
        {
            return;
        }

        float currentTime = Time.unscaledTime;
        float timeSinceLastFire = currentTime - lastFireTime;

        if (timeSinceLastFire < (1f / fireRate))
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.position);
        if (distanceToTarget > fireRange)
        {
            return;
        }

        // Calculate direction to target
        Vector3 directionToTarget = (targetEnemy.position - firePoint.position).normalized;

        // Rotate to face target (only y-axis since it's a mortar)
        Vector3 flatDirection = new(directionToTarget.x, 0, directionToTarget.z);
        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Now fire the projectile
        FireProjectile(directionToTarget, targetEnemy.position);
    }

    protected void FireProjectile(Vector3 shootDirection, Vector3 targetPosition)
    {
        if (audioSource != null && firingSound != null)
        {
            audioSource.PlayOneShot(firingSound);
        }

        lastFireTime = Time.unscaledTime; // Set the last fire time

        BaseProjectile projectile;
        if (PoolManager.Instance != null)
        {
            if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out ProjectilePool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {bulletPrefab.name}");
                return;
            }
            // Get projectile from pool and initialize it
            projectile = pool.Get(
                firePoint.position,
                Quaternion.LookRotation(shootDirection));

            if (projectile != null)
            {
                // Set up the ProjectileData for arcing trajectory
                ProjectileData data = bulletData;
                data.Direction = shootDirection;
                data.TargetPosition = targetPosition; // Set the target position
                data.ArcHeight = Vector3.Distance(firePoint.position, targetPosition) * 0.5f; // Dynamic arc height

                // Initialize the projectile
                projectile.Initialize(data, pool);
            }
        }
        else
        {
            projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity).GetComponent<BaseProjectile>();

            if (projectile != null)
            {
                // Set up the ProjectileData for arcing trajectory
                ProjectileData data = bulletData;
                data.Direction = shootDirection;
                data.TargetPosition = targetPosition; // Set the target position
                data.ArcHeight = Vector3.Distance(firePoint.position, targetPosition) * 0.5f; // Dynamic arc height

                // Initialize the projectile
                projectile.Initialize(data, null);
            }
        }
    }
}
