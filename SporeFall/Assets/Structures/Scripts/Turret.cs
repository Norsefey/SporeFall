using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Turret : MonoBehaviour
{
    public float detectionRange = 20f;       // How far the turret can detect enemies
    public float rotationSpeed = 5f;         // Speed of the turret rotation
    public LayerMask enemyLayerMask;         // Define what layer the enemies are in
    public Transform firePoint;              // Where the turret fires from
    public GameObject bulletPrefab;          // Bullet prefab for shooting
    public float fireRate = 1f;              // How often the turret fires
    public float raycastRange = 100f;        // Maximum range of raycast

    private Transform nearestEnemy;          // The nearest enemy detected
    private float fireCooldown = 0f;
    [SerializeField] Transform turretGuns;

    [Header("Firing Sound")]
    public AudioClip firingSound;            // Assign the firing sound clip in the Inspector
    private AudioSource audioSource;         // To play the firing sound
    void Start()
    {
        // Initialize the AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (nearestEnemy == null || !IsEnemyInRange(nearestEnemy))
        {
            FindNearestEnemy();
        }
        
        if (nearestEnemy != null)
        {
            RotateTurretTowardsEnemy();
            CheckLineOfSightAndFire();
        }

        // Cooldown management
        fireCooldown -= Time.deltaTime;
    }

    // Find the closest enemy within range
    void FindNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRange, enemyLayerMask);

        float shortestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemy in enemiesInRange)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy.transform;
            }
        }

        nearestEnemy = closestEnemy;
    }

    // Check if the current enemy is within detection range
    bool IsEnemyInRange(Transform enemy)
    {
        return enemy != null && Vector3.Distance(transform.position, enemy.position) <= detectionRange;
    }

    // Rotate the turret smoothly towards the nearest enemy (only on the y-axis)
    void RotateTurretTowardsEnemy()
    {
        Vector3 direction = nearestEnemy.position - transform.position;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        turretGuns.rotation = Quaternion.Slerp(turretGuns.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // Check line of sight using raycasting and fire at the enemy if visible
    void CheckLineOfSightAndFire()
    {
        RaycastHit hit;
        Vector3 enemyPos = (nearestEnemy.position - firePoint.position) + Vector3.up;
        
        if (Physics.Raycast(firePoint.position, enemyPos, out hit))
        {
            if (hit.transform == nearestEnemy)
            {
                if (fireCooldown <= 0f)
                {
                    Fire();
                    fireCooldown = 1f / fireRate;
                }
            }
        }
    }

    // Fire a bullet towards the enemy and play the firing sound
    void Fire()
    {
        // Play firing sound
        if (audioSource != null && firingSound != null)
        {
            audioSource.PlayOneShot(firingSound);
        }
        if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out ProjectilePool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {bulletPrefab.name}");
            return;
        }
        // Get projectile from pool
        ProjectileBehavior projectile = pool.Get(
        firePoint.position,
            Quaternion.LookRotation(firePoint.forward));

        if (projectile != null)
        {
            ProjectileData data = new()
            {
                Direction = firePoint.forward,
                Speed = 25,
                Damage = 20,
                Lifetime = .5f,
                UseGravity = false,
                ArcHeight = 0,
                CanBounce = false,
                MaxBounces = 0,
                BounceDamageMultiplier = 0
            };
            projectile.Initialize(data, pool);
        }
/*        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * 15000);*/

      
    }

    // Visualize detection range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
