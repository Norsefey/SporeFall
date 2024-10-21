using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //Debug.Log($"Enemies detected: {enemiesInRange.Length}");

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

        // Flatten the direction on the y-axis by setting y to 0
        direction.y = 0;

        // Calculate the desired rotation towards the enemy
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate towards the enemy on the y-axis
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    // Check line of sight using raycasting and fire at the enemy if visible
    void CheckLineOfSightAndFire()
    {
        RaycastHit hit;
        Vector3 enemyPos = (nearestEnemy.position - firePoint.position) + Vector3.up;
        // Perform a raycast towards the enemy
        if (Physics.Raycast(firePoint.position, enemyPos, out hit))
        {
            // Check if the raycast hits the nearest enemy
            if (hit.transform == nearestEnemy)
            {
                Debug.Log("Firing");
                // Line of sight is clear, fire at the enemy
                if (fireCooldown <= 0f)
                {
                    Fire();
                    fireCooldown = 1f / fireRate;
                }
            }
        }
    }
    // Fire a bullet towards the enemy
    void Fire()
    {   
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * 15000);
        // You can add a script on the bullet to make it move and damage the enemy
        // Example: bullet.GetComponent<Bullet>().SetTarget(nearestEnemy);
    }
    // Visualize detection range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}