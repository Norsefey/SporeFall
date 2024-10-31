using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeGun : Weapon
{
    [Space(6),Header("Charge Variables")]
    public float maxChargeTime = 2f; // Max time to fully charge
    public float minChargeMultiplier = 1f; // Minimum power for a shot
    public float maxChargeMultiplier = 3f; // Maximum power for a fully charged shot
    public float baseChargeSpeed = 10;
    private bool isCharging = false;
    private float chargeAmount = 0f;

    // Called while holding down the fire button to accumulate charge
    public void Charge()
    {
        if (bulletCount <= 0 && !IsReloading)
        {
            StartReload();
        }
        else if (bulletCount <= 0 || IsReloading) 
            return;

        if (!isCharging)
        {
            isCharging = true;
            chargeAmount = 0f; // Reset charge
        }

        // Accumulate charge based on how long the fire button is held
        chargeAmount += Time.deltaTime / maxChargeTime;
        chargeAmount = Mathf.Clamp01(chargeAmount); // Clamp charge to [0,1]
        Debug.Log("Charging: " + (chargeAmount * 100).ToString("F0") + "%");
    }
    // Called when the fire button is released to fire the charged shot
    public void Release()
    {
        if (!isCharging || bulletCount <= 0) return;

        isCharging = false;

        // Calculate the charge multiplier
        float chargeMultiplier = Mathf.Lerp(minChargeMultiplier, maxChargeMultiplier, chargeAmount);
        //Debug.Log("Firing: " + chargeMultiplier + "xDamage");
        // Fire the shot based on the charge multiplier
        if (isHitScan)
        {
            FireHitscan(chargeMultiplier);
        }
        else
        {
            FireProjectile(chargeMultiplier);
        }

        bulletCount--;
    }

    // Fire projectile with charge multiplier affecting its power (damage or speed)
    private void FireProjectile(float chargeMultiplier)
    {
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(player.pCamera.myCamera.transform.forward);
        // Rotate player first before shooting
        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            Debug.Log("Rotating on Fire");
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
        // Instantiate the projectile and shoot it in the spread direction
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        projectile.transform.localScale *= Mathf.Clamp(chargeMultiplier, 0, 3);

        ProjectileBehavior projectileComp = projectile.GetComponent<ProjectileBehavior>();
        if (projectileComp != null)
        {
            ProjectileData data = new()
            {
                Direction = shootDirection,
                Speed = projectileSpeed * chargeMultiplier,// Bullet moves faster the higher the charge
                Damage = damage * chargeMultiplier,
                Lifetime = projectileLifetime,
                UseGravity = useGravity,
                ArcHeight = projectileArcHeight,
                CanBounce = canBounce,
                MaxBounces = maxBounces,
                BounceDamageMultiplier = bounceDamageMultiplier
            };
            projectileComp.Initialize(data);
            Debug.Log(weaponName + " fired a charged projectile with power: " + chargeMultiplier + " For " + damage * chargeMultiplier + " Damage");
        }
    }

    // Fire hitscan with charge multiplier affecting its damage
    private void FireHitscan(float chargeMultiplier)
    {
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(player.pCamera.myCamera.transform.forward);
        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            Debug.Log("Rotating on Fire");
            player.pController.RotateOnFire(this.transform, shootDirection);
        }

        Ray ray = new(player.pCamera.myCamera.transform.position, shootDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, hitScanDistance, hitLayers)) // Range of the hitscan weapon
        {
            Debug.Log(weaponName + " hit: " + hit.collider.name);
            Instantiate(projectilePrefab, hit.point, Quaternion.LookRotation(hit.normal));
            // Apply damage to the hit object
            if (hit.collider.CompareTag("Enemy"))
            {
                if (hit.transform.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage * chargeMultiplier);
                }
            }
        }
    }
}
