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

    //[Header("Debug")]
    //[SerializeField] private bool showDebug = true;
    //private bool canShoot = false;
    // private string debugStatus = "";

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
    }

    void Update()
    {
        //debugStatus = "Status: ";

        if (!hasTarget || !IsTargetValid())
        {
           // debugStatus += "Searching for target... ";
            FindTarget();
        }

        if (hasTarget && Time.timeScale == 1)
        {
           // debugStatus += "Has target... ";
            TrackTarget();
            TryShoot();
        }
        else
        {
           // debugStatus += "No valid target found.";
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
            if (enemyCollider == null || enemyCollider.transform == null) continue;

            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distance >= minimumFireRange && distance <= fireRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyCollider.transform;
            }
        }

      /*  if (targetEnemy != closestEnemy)
        {
           // debugStatus += $"New target found at distance {closestDistance:F1}... ";
        }*/

        targetEnemy = closestEnemy;
        hasTarget = targetEnemy != null;
    }

    // Check if the current enemy is within detection range
    private bool IsTargetValid()
    {

        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
        {
           // debugStatus += "Target is null... ";
            return false;
        }

        float distance = Vector3.Distance(transform.position, targetEnemy.position);
        bool isInRange = distance >= minimumFireRange && distance <= fireRange;

     /*   if (!isInRange)
        {
           // debugStatus += $"Target out of range (distance: {distance:F1})... ";
        }*/

        return isInRange;
    }

    // Rotate the turret smoothly towards the nearest enemy (only on the y-axis)
    private void TrackTarget()
    {
        if (!targetEnemy.parent.gameObject.activeSelf)
        {
            hasTarget = false;
            return;
        }
        Vector3 targetDirection = targetEnemy.parent.position - transform.position;
        //targetDirection.y = 0; // Keep rotation only on Y axis

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            turretGuns.transform.rotation = Quaternion.Slerp(
                turretGuns.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Check line of sight using raycasting and fire at the enemy if visible
    private void TryShoot()
    {
        //canShoot = false;

        if (targetEnemy == null)
        {
            //debugStatus += "Can't shoot: No target... ";
            return;
        }

        float currentTime = Time.unscaledTime;
        float timeSinceLastFire = currentTime - lastFireTime;

        if (timeSinceLastFire < (1f / fireRate))
        {
            //debugStatus += $"Cooling down ({timeSinceLastFire:F1}s)... ";
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.position);
        if (distanceToTarget < minimumFireRange || distanceToTarget > fireRange)
        {
            //debugStatus += $"Can't shoot: Target distance ({distanceToTarget:F1}) out of range... ";
            return;
        }
        Vector3 directionToTarget = (targetEnemy.position - firePoint.position).normalized;
      /*  // Visualize the raycast in debug mode
        if (showDebug)
        {
            Debug.DrawRay(firePoint.position, directionToTarget * distanceToTarget, Color.red, 0.1f);
        }*/

        if (Physics.Raycast(firePoint.position, directionToTarget, out RaycastHit hit, distanceToTarget, enemyLayerMask))
        {
            if (hit.transform != targetEnemy)
            {
              //  debugStatus += "Can't shoot: Line of sight blocked... ";
                return;
            }

            //canShoot = true;
           // debugStatus += "Shooting! ";
            Shoot();
            lastFireTime = currentTime;
        }
    }

    // Fire a bullet towards the enemy and play the firing sound
    private void Shoot()
    {
        if (audioSource != null && firingSound != null)
        {
            audioSource.PlayOneShot(firingSound);
        }

        if(PoolManager.Instance != null)
        {
            if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out ProjectilePool pool))
            {
                // Debug.LogError($"No pool found for projectile prefab: {bulletPrefab.name}");
                return;
            }
            else
            {
                ProjectileBehavior projectile = pool.Get(firePoint.position, Quaternion.LookRotation(firePoint.forward));

                if (projectile != null)
                {
                    bulletData.Direction = firePoint.forward;
                    projectile.Initialize(bulletData, pool);
                }
            }
        }
        else
        {
            ProjectileBehavior projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(firePoint.forward)).GetComponent<ProjectileBehavior>();

            bulletData.Direction = firePoint.forward;
            projectile.Initialize(bulletData, null);
        }
       
    }

}
