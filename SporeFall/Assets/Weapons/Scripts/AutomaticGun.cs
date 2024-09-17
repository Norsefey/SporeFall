using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Weapon
{
    private float nextFireTime = 0f;

    public override void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            base.Fire(); // Call the base fire logic
            nextFireTime = Time.time + 1f / fireRate; // Control fire rate
        }
    }

}
