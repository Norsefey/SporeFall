using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : MonoBehaviour
{
    public float damagePerSecond = 8f;      // Damage per second dealt to enemies
    public float range = 5f;                // Radius of the flamethrower effect
    public LayerMask enemyLayer;            // The layer that enemies are on
    public string enemyTag = "Enemy";       // Tag for enemies
    public float tickRate = 0.5f;           // How often damage is applied (in seconds)

    private float tickTimer = 0f;

    void Update()
    {
        tickTimer += Time.deltaTime;

        // Apply damage at intervals (tickRate)
        if (tickTimer >= tickRate)
        {
            DamageEnemiesInRange();
            tickTimer = 0f; // Reset timer
        }
    }

    // Apply damage to all enemies within the range
    void DamageEnemiesInRange()
    {
        // Get all colliders within the range
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, enemyLayer);

        foreach (Collider enemy in enemiesInRange)
        {
            // Check if the collider has the enemy tag
            if (enemy.CompareTag(enemyTag))
            {
                // Assuming the enemy has a script with a method to take damage
                enemy.GetComponent<Sherman>()?.TakeDamage(damagePerSecond * tickRate);
                Debug.Log("hit");
            }
        }
    }

    // Optional: Visualize the damage area in the Unity Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
