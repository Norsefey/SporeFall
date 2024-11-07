using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstGun : Weapon
{
    [Space(6), Header("Burst Variables")]
    public int burstCount = 3; // Number of shots per burst
    private bool isFiringBurst = false; // Prevents firing another burst while one is ongoing
    private bool triggerHeld = false; // Tracks if the player is holding the fire input
    [SerializeField] private float betweenShotInterval = 0.2f;
    [Header("Audio Settings")]
    public AudioClip fireSound; // Assign the burst firing sound in the Inspector
    [Range(0f, 1f)] public float fireSoundVolume = 0.5f; // Volume control for the burst firing sound

    private void Update()
    {
        // This Update function ensures the weapon doesn't fire while the fire button is held
        if (!triggerHeld && isFiringBurst)
        {
            // The burst has finished, reset the isFiringBurst flag
            isFiringBurst = false;
        }
    }

    public override void Fire()
    {
        if (bulletCount <= 0 && !IsReloading)
        {
            // Reload if the player tries firing with 0 magazine
            StartReload();
        }
        
        if (isFiringBurst || triggerHeld || bulletCount <= 0 || IsReloading)
        {
            return; // Only fire if we aren't in the middle of a burst and the player isn't holding the fire button
        }

        // Start the burst firing coroutine
        StartCoroutine(FireBurst());
        triggerHeld = true; // Mark that the player has pressed the fire button
    }

    // Coroutine to handle burst firing over time
    private IEnumerator FireBurst()
    {
        Debug.Log("Burst Firing");
        isFiringBurst = true;

        for (int i = 0; i < burstCount && bulletCount > 0; i++)
        {
            if (bulletCount <= 0 && !IsReloading)
            {
                StartReload();
            }
            else
            {
                if (isHitScan)
                {
                    FireHitscan(player.pCamera.myCamera);
                }
                else
                {
                    FireProjectile(firePoint, player.pCamera.myCamera);
                }
            }
        }
        // Play the firing sound
        if (fireSound != null)
        {
            // Create a temporary GameObject to play the sound
            GameObject audioPlayer = new GameObject("BurstGunFireSound");
            AudioSource audioSource = audioPlayer.AddComponent<AudioSource>();
            audioSource.clip = fireSound;
            audioSource.volume = fireSoundVolume; // Set the volume
            audioSource.Play();

            // Destroy the audio player object after the sound finishes
            Destroy(audioPlayer, fireSound.length);
        }
        bulletCount--; // Decrease the bullet count

        // Wait for the player to release the fire button
        yield return new WaitUntil(() => !triggerHeld);

        yield return new WaitForSeconds(betweenShotInterval);
    }

    public void OnFireReleased()
    {
        // Called when the fire button is released
        StartCoroutine(BetweenShotDelay());
    }

    private IEnumerator BetweenShotDelay()
    {
        // Add a delay between shots to avoid spamming
        yield return new WaitForSeconds(betweenShotInterval);
        triggerHeld = false;
    }
}
