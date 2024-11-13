using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Turret : MonoBehaviour
{
    // These public variables are set by TurretBehavior
    //[HideInInspector]
    public float 
        detectionRange, 
        rotationSpeed, 
        fireRate, 
        fireRange, 
        fireCooldown;
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

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    private bool canShoot = false;
    private string debugStatus = "";

    [Header("Audio")]
    [SerializeField] private AudioClip firingSound;
    [Range(0f, 1f)]
    [SerializeField] private float flameVolume = 0.5f;

    // Private variables
    private Transform targetEnemy;
    private AudioSource audioSource;
    private bool hasTarget;
    private float lastFireTime; // Track the last time we fired

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = firingSound;
        audioSource.volume = flameVolume;

        // Validate settings
        if (fireRange <= minimumFireRange)
        {
            Debug.LogWarning($"[Turret] Fire range ({fireRange}) should be greater than minimum fire range ({minimumFireRange})");
        }
    }

    void Update()
    {
        debugStatus = "Status: ";

        if (!hasTarget || !IsTargetValid())
        {
            debugStatus += "Searching for target... ";
            FindTarget();
        }

        if (hasTarget)
        {
            debugStatus += "Has target... ";
            TrackTarget();
            TryShoot();
        }
        else
        {
            debugStatus += "No valid target found.";
            canShoot = false;
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
            if (distance >= minimumFireRange && distance <= fireRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyCollider.transform;
            }
        }

        if (targetEnemy != closestEnemy)
        {
            debugStatus += $"New target found at distance {closestDistance:F1}... ";
        }

        targetEnemy = closestEnemy;
        hasTarget = targetEnemy != null;
    }

    // Check if the current enemy is within detection range
    private bool IsTargetValid()
    {

        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
        {
            debugStatus += "Target is null... ";
            return false;
        }

        float distance = Vector3.Distance(transform.position, targetEnemy.position);
        bool isInRange = distance >= minimumFireRange && distance <= fireRange;

        if (!isInRange)
        {
            debugStatus += $"Target out of range (distance: {distance:F1})... ";
        }

        return isInRange;
    }

    // Rotate the turret smoothly towards the nearest enemy (only on the y-axis)
    private void TrackTarget()
    {
        if (!targetEnemy.gameObject.activeSelf)
        {
            hasTarget = false;
            return;
        }
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
        canShoot = false;

        if (targetEnemy == null)
        {
            debugStatus += "Can't shoot: No target... ";
            return;
        }

        float currentTime = Time.unscaledTime;
        float timeSinceLastFire = currentTime - lastFireTime;

        if (timeSinceLastFire < (1f / fireRate))
        {
            debugStatus += $"Cooling down ({timeSinceLastFire:F1}s)... ";
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.position);
        if (distanceToTarget < minimumFireRange || distanceToTarget > fireRange)
        {
            debugStatus += $"Can't shoot: Target distance ({distanceToTarget:F1}) out of range... ";
            return;
        }
        Vector3 directionToTarget = (targetEnemy.position - firePoint.position).normalized;
        // Visualize the raycast in debug mode
        if (showDebug)
        {
            Debug.DrawRay(firePoint.position, directionToTarget * distanceToTarget, Color.red, 0.1f);
        }

        if (Physics.Raycast(firePoint.position, directionToTarget, out RaycastHit hit, distanceToTarget, enemyLayerMask))
        {
            if (hit.transform != targetEnemy)
            {
                debugStatus += "Can't shoot: Line of sight blocked... ";
                return;
            }

            canShoot = true;
            debugStatus += "Shooting! ";
            Shoot();
            lastFireTime = currentTime;
        }
        else
        {
            debugStatus += "Can't shoot: Raycast missed... ";
        }
    }

    // Fire a bullet towards the enemy and play the firing sound
    private void Shoot()
    {
        if (audioSource != null && firingSound != null)
        {
            audioSource.PlayOneShot(firingSound);
        }

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
            bulletData.Direction = firePoint.forward;
            projectile.Initialize(bulletData, pool);
        }
    }
    private void OnGUI()
    {
        if (!showDebug) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = canShoot ? Color.green : Color.red;
        style.fontSize = 14;
        style.padding = new RectOffset(10, 10, 10, 10);

        GUI.Label(new Rect(10, 10, Screen.width - 20, 30), debugStatus, style);
    }
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw minimum and maximum fire ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minimumFireRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fireRange);

        if (showDebug && hasTarget && targetEnemy != null)
        {
            // Draw line to target
            Gizmos.color = canShoot ? Color.green : Color.red;
            Gizmos.DrawLine(firePoint.position, targetEnemy.position);
        }
    }
}
