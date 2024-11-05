using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBulletBehavior : MonoBehaviour
{
    public float deathTime = 2;
    [SerializeField] GameObject bulletResidue;
    public float dmg = 20f;
    public string enemyTag = "Enemy";
    public AudioClip bulletSound; // Assign the bullet sound clip in the inspector
    [Range(0f, 1f)] public float bulletSoundVolume = 0.5f; // Set the default volume in the inspector

    private void Update()
    {
        Destroy(gameObject, deathTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(enemyTag))
        {
            // Try to get the EnemyHealth script on the object and deal damage
            if (collision.gameObject.TryGetComponent<Damageable>(out var hp))
            {
                hp.TakeDamage(dmg);  // Apply damage to the enemy
            }
        }

        // Instantiate the bullet residue effect
        Instantiate(bulletResidue, transform.position, Quaternion.identity);

        // Create a temporary GameObject to play the bullet sound
        GameObject audioPlayer = new GameObject("BulletSound");
        AudioSource audioSource = audioPlayer.AddComponent<AudioSource>();
        audioSource.clip = bulletSound;
        audioSource.volume = bulletSoundVolume; // Set the volume to the specified level
        audioSource.Play();

        // Destroy the audio player object after the sound finishes
        Destroy(audioPlayer, bulletSound.length);

        // Destroy the bullet object
        Destroy(gameObject);
    }
}
