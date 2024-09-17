using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstGun : Weapon
{
    public int burstCount = 3;
    private float nextFireTime = 0f;
    private int shotsFired = 0;

    public override void Fire()
    {
        if (shotsFired < burstCount && magazineCount > 0)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Shoot();
                shotsFired++;
            }
        }
        else
        {
            // Reset burst count after firing all shots
            shotsFired = 0;
        }
    }

    private void Shoot()
    {
        // Shooting logic for burst weapon
        Debug.Log(weaponName + " fired a burst!");
        magazineCount--;
    }
}
