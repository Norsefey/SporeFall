using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Turret : MonoBehaviour
{
    // These public variables are set by TurretBehavior
    //[HideInInspector]
    public float 
        damage, 
        detectionRange, 
        rotationSpeed, 
        fireRate, 
        fireRange, 
        fireCooldown;
    //[HideInInspector]
    public ProjectileData bulletData;

    [Header("References")]
    [SerializeField] private Transform turretGuns;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Audio")]
    [SerializeField] private AudioClip firingSound;
    [Range(0f, 1f)]
    [SerializeField] private float flameVolume = 0.5f;

    // Private variables
    private Transform targetEnemy;
    private float nextFireTime;
    private AudioSource audioSource;
    private bool hasTarget;

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

        if (hasTarget)
        {
            TrackTarget();
            TryShoot();
        }
    }

    // Find the closest enemy within range
    private void FindTarget()
    {
        // Find all enemies in range
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRange, enemyLayerMask);

        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distance < closestDistance)
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
        if (targetEnemy == null) return false;

        float distance = Vector3.Distance(transform.position, targetEnemy.position);
        return distance <= detectionRange;
    }

    // Rotate the turret smoothly towards the nearest enemy (only on the y-axis)
    private void TrackTarget()
    {
        Vector3 targetDirection = targetEnemy.position - transform.position;
        targetDirection.y = 0; // Keep rotation only on Y axis

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            turretGuns.rotation = Quaternion.Slerp(
                turretGuns.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Check line of sight using raycasting and fire at the enemy if visible
    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        // Check if we have clear line of sight
        Vector3 directionToTarget = (targetEnemy.position - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, directionToTarget, out RaycastHit hit))
        {
            if (hit.transform != targetEnemy) return; // Something is blocking our shot
        }

        Shoot();
        nextFireTime = Time.time + (1f / fireRate);
    }

    // Fire a bullet towards the enemy and play the firing sound
    private void Shoot()
    {
        // Play sound effect
        if (audioSource != null && firingSound != null)
        {
            audioSource.PlayOneShot(firingSound);
        }

        // Get projectile from pool
        if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out ProjectilePool pool))
        {
            Debug.LogError($"No pool found for projectile prefab: {bulletPrefab.name}");
            return;
        }

        ProjectileBehavior projectile = pool.Get(
            firePoint.position,
            Quaternion.LookRotation(firePoint.forward)
        );

        if (projectile != null)
        {
            // since it rotate we set the direction here
            bulletData.Direction = firePoint.forward;
            projectile.Initialize(bulletData, pool);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
