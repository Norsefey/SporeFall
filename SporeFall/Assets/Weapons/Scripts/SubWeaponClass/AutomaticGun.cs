using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Weapon
{
    private float nextFireTime = 0f;
    private float lastFireTime = -999f;  // Initialize to a negative value to handle first shot
    private bool isInBurst = false;      // Track if we have been firing to apply spread
    private const float burstResetTime = 0.5f;  // Time after which we consider firing to have reset
    [Header("Automatic Variables")]
    public float fireRate = 5; // how fast the bullets come out

    public override void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            // Calculate time since last fire
            float timeSinceLastFire = Time.time - lastFireTime;
            
            if (timeSinceLastFire > burstResetTime)
            {
                isInBurst = false;  // Reset burst state if enough time has passed
            }

            // First shot in a sequence is accurate, subsequent shots use spread
            useSpread = isInBurst;

            base.Fire(); // Call the base fire logic

            isInBurst = true;  // Mark that we're now firing
            lastFireTime = Time.time;
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
