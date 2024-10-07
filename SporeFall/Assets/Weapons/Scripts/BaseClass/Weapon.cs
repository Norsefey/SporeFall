using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Weapon : MonoBehaviour
{
    [Header("References")]
    public string weaponName;
    public PlayerManager player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask hitLayers;
    [Header("Corruption")]
    public bool isCorrupted;
    public float corruptionRate = 1.2f;

    [Header("Base Stats")]
    public float damage;
    public float bulletSpreadAngle = 2f; // Angle in degrees for bullet spread
    public float reloadTime = 2f; // Time it takes to reload

    [Header("Ammo Variables")]
    public int bulletCount;
    public int bulletCapacity;
    public int totalAmmo;
    public bool limitedAmmo = false;
    public bool isHitScan; // Whether the weapon is hitscan or projectile based
    public float bulletDistance = 50;
    private bool isReloading;
    // in order to have a private variable public to other scripts, and not be editable in editor we use Get property
    // since this is a Get variable it is capitalized
    public bool IsReloading { get { return isReloading; } }

    // Fire method to be implemented by subclasses
    public virtual void Fire()
    {
        if (bulletCount <= 0 && !IsReloading)
        {
            Reload();
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
            bulletCount--;
        }
    }
    private void FireProjectile(Transform firePoint, Camera playerCamera)
    {
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(playerCamera.transform.forward);

        // Instantiate the projectile and shoot it in the spread direction
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = shootDirection * bulletDistance; // Adjust projectile speed as needed
        if (player.pController.currentState == PlayerMovement.PlayerState.Aiming)
        {
            Debug.Log("Only Rotating Gun");
            transform.forward = shootDirection;
        }
        else
        {
            Debug.Log("Rotating on Fire");
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
        Debug.Log(weaponName + " fired a projectile.");
    }
    private void FireHitscan(Camera playerCamera)
    {
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(playerCamera.transform.forward);
        if (player.pController.currentState == PlayerMovement.PlayerState.Aiming)
        {
            Debug.Log("Only Rotating Gun");
            transform.forward = shootDirection;
        }
        else
        {
            Debug.Log("Rotating on Fire");
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
        Ray ray = new(playerCamera.transform.position, shootDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, bulletDistance, hitLayers)) // Range of the hitscan weapon
        {
            Debug.Log(weaponName + " hit: " + hit.collider.name);
            Instantiate(projectilePrefab, hit.point, Quaternion.LookRotation(hit.normal));
            // Apply damage to the hit object

            // Sherman must die for now
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
            //hit.collider.GetComponent<Sherman>()?.TakeDamage(damage);
        }
    }
    // Method to calculate a bullet spread direction
    public Vector3 GetSpreadDirection(Vector3 baseDirection)
    {
        // Randomly offset the direction within a cone defined by bulletSpreadAngle
        float spreadX = Random.Range(-bulletSpreadAngle, bulletSpreadAngle);
        float spreadY = Random.Range(-bulletSpreadAngle, bulletSpreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
        return spreadRotation * baseDirection;
    }
    public virtual void Reload()
    {
        if (!IsReloading && (!limitedAmmo) || bulletCount < bulletCapacity )
        {
            StartCoroutine(ReloadCoroutine());
        }
    }
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log(weaponName + " is reloading...");

        if(limitedAmmo)
        {
            if (totalAmmo <= 0)
            {
                Debug.Log(weaponName + " has no more Ammo");
                yield return null;
            }
            // Wait for reload time to complete
            yield return new WaitForSeconds(reloadTime);

            int reloadAmount = bulletCapacity - bulletCount;
            if (totalAmmo > reloadAmount)
            {
                // Complete the reload
                bulletCount = bulletCapacity;
                totalAmmo -= reloadAmount;
            }
            else
            {
                // take the final bullets from ammo
                bulletCount = totalAmmo;
                totalAmmo = 0;
            }
        }
        else
        {
            yield return new WaitForSeconds(reloadTime);
            bulletCount = bulletCapacity;
        }


        if (player.pUI != null)
            player.pUI.AmmoDisplay(this);
        isReloading = false;
    }
}
