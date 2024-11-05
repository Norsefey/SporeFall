using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Weapon
{
    private float nextFireTime = 0f;
    [Header("Automatic Variables")]
    public float fireRate; // how fast the bullets come out

    [Header("Audio Settings")]
    public AudioClip fireSound; // Assign the gun's firing sound in the Inspector
    [Range(0f, 1f)] public float fireSoundVolume = 0.5f; // Volume control for the firing sound

    public override void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            base.Fire(); // Call the base fire logic

            // Play the firing sound
            if (fireSound != null)
            {
                // Create a temporary GameObject to play the sound
                GameObject audioPlayer = new GameObject("GunFireSound");
                AudioSource audioSource = audioPlayer.AddComponent<AudioSource>();
                audioSource.clip = fireSound;
                audioSource.volume = fireSoundVolume; // Set the volume
                audioSource.Play();

                // Destroy the audio player object after the sound finishes
                Destroy(audioPlayer, fireSound.length);
            }

            // Control fire rate
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
