using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstGun : Weapon
{
    [Space(6), Header("Burst Variables")]
    public int burstCount = 3; // Number of shots per burst
    private bool isFiringBurst = false; // Prevents firing another burst while one is ongoing
    private bool triggerHeld = false; // Tracks if the player is holding the fire input

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
        {// reload is player tries firing with 0 magazine
            Reload();
        }else if (isFiringBurst || triggerHeld || bulletCount <= 0 || IsReloading) return; // Only fire if we aren't in the middle of a burst and the player isn't holding the fire button

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
            base.Fire(); // Fire a single shot
        }

        // Wait for the player to release the fire button
        yield return new WaitUntil(() => !triggerHeld);
    }
    public void OnFireReleased()
    {
        // Called when the fire button is released
        triggerHeld = false;
    }
}
