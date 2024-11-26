using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : MonoBehaviour
{
    [HideInInspector]
    public float damagePerSecond, range, tickRate;      // Damage per second dealt to enemies
    public LayerMask enemyLayer;            // The layer that enemies are on

    private float tickTimer = 0f;
    [Header("Audio Settings")]
    public AudioClip flameSound;            // Sound for the flamethrower
    [Range(0f, 1f)] public float flameVolume = 0.5f; // Volume control for the flame sound

    private AudioSource flameAudioSource;
    private PlayParticles pp;

    private void Start()
    {
        pp = GetComponent<PlayParticles>();

        // Set up the AudioSource for the flame sound
        flameAudioSource = gameObject.AddComponent<AudioSource>();
        flameAudioSource.clip = flameSound;
        flameAudioSource.volume = flameVolume;
        flameAudioSource.loop = true; // Loop the sound while enemies are in range
    }

    void Update()
    {
        tickTimer += Time.deltaTime;

        // Apply damage at intervals (tickRate)
        if (tickTimer >= tickRate)
        {
            bool enemiesInRange = DamageEnemiesInRange();
            tickTimer = 0f; // Reset timer

            // Play or stop the flame sound based on enemies being in range
            if (enemiesInRange && !flameAudioSource.isPlaying)
            {
                flameAudioSource.Play();
            }
            else if (!enemiesInRange && flameAudioSource.isPlaying)
            {
                flameAudioSource.Stop();
            }
        }
    }

    // Apply damage to all enemies within the range and return true if any enemies were hit
    bool DamageEnemiesInRange()
    {
        bool hasHitEnemies = false;

        // Get all colliders within the range
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, enemyLayer);
        foreach (Collider enemy in enemiesInRange)
        {
            if(enemy.CompareTag("HeadShot"))
                 continue;

            hasHitEnemies = true;

            // Assuming the enemy has a script with a method to take damage
            enemy.GetComponent<Damageable>()?.TakeDamage(damagePerSecond * tickRate);
            Debug.Log("hit");
            pp.PlayEffects();
            
        }

        return hasHitEnemies;
    }

    // Optional: Visualize the damage area in the Unity Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
