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
            if (enemyCollider == null || enemyCollider.transform == null) continue;

            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distance <= fireRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyCollider.transform;
            }
        }

        targetEnemy = closestEnemy;
        hasTarget = targetEnemy != null;
    }

    // Check if the current enemy is within detection range
    private bool IsTargetValid()
    {

        if (targetEnemy == null || targetEnemy.GetComponent<EnemyHPRelay>().IsDead())
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, targetEnemy.position);
        bool isInRange = distance <= fireRange;

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
        Vector3 flatDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);
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

        ProjectileBehavior projectile;
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
                data.targetedDirection = true; // Enable targeted direction for mortars
                data.TargetPosition = targetPosition; // Set the target position
                data.ArcHeight = Vector3.Distance(firePoint.position, targetPosition) * 0.5f; // Dynamic arc height

                // Initialize the projectile
                projectile.Initialize(data, pool);
            }
        }
        else
        {
            projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity).GetComponent<ProjectileBehavior>();

            if (projectile != null)
            {
                // Set up the ProjectileData for arcing trajectory
                ProjectileData data = bulletData;
                data.Direction = shootDirection;
                data.targetedDirection = true; // Enable targeted direction for mortars
                data.TargetPosition = targetPosition; // Set the target position
                data.ArcHeight = Vector3.Distance(firePoint.position, targetPosition) * 0.5f; // Dynamic arc height

                // Initialize the projectile
                projectile.Initialize(data, null);
            }
        }
    }
}
