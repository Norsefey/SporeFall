using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeGun : Weapon
{
    public float chargeTime = 2f;
    private float chargeTimer = 0f;
    private bool isCharging = false;

    public override void Fire()
    {
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeTime)
            {
                Shoot();
                isCharging = false;
                chargeTimer = 0f;
            }
        }
        else if (Input.GetButtonDown("Fire"))
        {
            StartCharging();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        Debug.Log(weaponName + " is charging...");
    }
    private void Shoot()
    {
        // Shooting logic for charged shot
        Debug.Log(weaponName + " fired a charged shot!");
        magazineCount--;
    }
}
