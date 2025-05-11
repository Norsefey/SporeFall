using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeGun : Weapon
{
    [Space(6), Header("Charge Variables")]
    public float maxChargeTime = 2f; // Max time to fully charge
    public float minChargeMultiplier = 1f; // Minimum power for a shot
    public float maxChargeMultiplier = 3f; // Maximum power for a fully charged shot
    public float baseChargeSpeed = 10;
    [HideInInspector]
    public float chargeAmount = 0f;
    private bool isCharging = false;
    [Header("Multiple Projectile Settings")]
    [SerializeField] private int maxProjectileCount = 5; // Maximum number of projectiles
    [SerializeField] private float spreadAngle = 15f; // Spread angle for multiple projectiles

    [Header("Charge Audio Settings")]
    public AudioClip chargeSound; // Charging sound clip
    [Range(0f, 1f)] public float chargeSoundVolume = 0.5f; // Volume for charging sound

    // Called while holding down the fire button to accumulate charge
    public void Charge()
    {
        if (bulletCount <= 0 && !IsReloading)
        {
            StartReload();
        }
        else if (bulletCount <= 0 || IsReloading)
        {
            return;
        }

        if (!isCharging)
        {
            isCharging = true;
            chargeAmount = 0f; // Reset charge

            // Play the charging sound
            if (chargeSound != null && !player.pController.audioSource.isPlaying)
            {
                PlaySFX(chargeSound, true);
            }
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

        // Stop the charging sound
        if (player.pController.audioSource.isPlaying)
        {
            player.pController.audioSource.Stop();
        }

        // Calculate the charge multiplier
        float chargeMultiplier = Mathf.Lerp(minChargeMultiplier, maxChargeMultiplier, chargeAmount);

        // Play the firing sound
        PlaySFX(fireSound, false);

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
        // Precise calculation of projectile count
        int currentProjectileCount = Mathf.Max(1, Mathf.RoundToInt(chargeAmount * (maxProjectileCount - 1) + 1));

        Vector3 baseDirection = player.pCamera.myCamera.transform.forward;

        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            player.pController.RotateOnFire();
        }

        ProjectilePool pool = null;

        if (PoolManager.Instance != null)
        {
            if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out pool))
            {
                Debug.LogError($"No pool found for Bullet prefab: {bulletPrefab.name}");
                return;
            }
        }
        // Fire multiple projectiles with spread
        for (int i = 0; i < currentProjectileCount; i++)
        {
            Vector3 shootDirection = GetSpreadDirection(baseDirection, currentProjectileCount, i);

            BaseProjectile projectile;

            if (pool != null)
            {
                // Get projectile from pool
                 projectile = pool.Get(firePoint.position, Quaternion.LookRotation(shootDirection));
            }
            else
            {
                projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection)).GetComponent<BaseProjectile>();
            }
          

            if (projectile != null)
            {
                ProjectileData data = new()
                {
                    Direction = shootDirection,
                    Damage = damage * chargeMultiplier, // Spread damage across projectiles
                    Speed = projectileData.Speed,
                    Lifetime = projectileData.Lifetime,
                    UseGravity = projectileData.UseGravity,
                    ArcHeight = projectileData.ArcHeight,
                    CanBounce = projectileData.CanBounce,
                    MaxBounces = projectileData.MaxBounces,
                    BounceDamageMultiplier = projectileData.BounceDamageMultiplier,
                };
                projectile.Initialize(data, pool);
                Debug.Log($"{weaponName} fired a charged projectile with power: {chargeMultiplier}, Projectile: {i + 1}/{currentProjectileCount}");
            }
            else
            {
                Debug.Log("No Projectile");
            }
        }
    }
    // Fire hitscan with charge multiplier affecting its damage
    private void FireHitscan(float chargeMultiplier)
    {
        PlaySFX(fireSound, false);
        // Calculate shoot direction with spread
        Vector3 shootDirection = GetSpreadDirection(player.pCamera.myCamera.transform.forward);
        // Rotate player if not aiming
        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            player.pController.RotateOnFire();
        }
        else
        {
            // Ensure the weapon is aligned with the shoot direction
            transform.forward = shootDirection;
        }
        VFXPoolingBehavior vfx = null;
        if (PoolManager.Instance != null)
        {
            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(bulletPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {bulletPrefab.name}");
                return;
            }
            vfx = pool.Get(firePoint.position, Quaternion.LookRotation(shootDirection));
            vfx.Initialize(pool);

            vfx.transform.localScale = Vector3.one * (1 + chargeMultiplier);
        }
        else
        {
            // No pool spawn and enabled VFX
            vfx = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection)).GetComponent<VFXPoolingBehavior>();
            vfx.gameObject.SetActive(true);
        }


        Ray ray = new(player.pCamera.myCamera.transform.position, shootDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, hitScanDistance, hitLayers)) // Range of the hitscan weapon
        {
            //Debug.Log("Hit" + hit.transform.gameObject.name);

            // Apply damage to the hit object
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("HeadShot"))
            {
                if (hit.transform.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage * chargeMultiplier);
                }
            }
            vfx.MoveToLocation(hit.point, 50);
        }
        else
        {
            vfx.MoveForward();
        }

        if (bulletCount <= 0 && !IsReloading)
        {// auto reload if player tries shooting at 0 ammo clip
            PlaySFX(reloadSound, false);
            StartReload();
        }
    }
    private Vector3 GetSpreadDirection(Vector3 baseDirection, int projectileCount, int projectileIndex)
    {
        if (projectileCount <= 1)
            return baseDirection;

        // Calculate dynamic spread angle based on charge amount
        float dynamicSpreadAngle = spreadAngle * chargeAmount;

        // Calculate spread
        float spreadStep = dynamicSpreadAngle / (projectileCount - 1);
        float offsetAngle = -dynamicSpreadAngle / 2f + spreadStep * projectileIndex;

        // Rotate the base direction
        Quaternion spreadRotation = Quaternion.AngleAxis(offsetAngle, Vector3.up);
        return spreadRotation * baseDirection;
    }
}
