using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class Weapon : MonoBehaviour
{
    [Header("References")]
    public string weaponName;
    public Sprite weaponSprite;
    public PlayerManager player;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public LayerMask hitLayers;

    [Header("Audio Clips")]
    [SerializeField, Range(0f, 1f)] protected float fireSoundVolume = 0.5f; // Volume control for the firing sound
    [SerializeField] protected AudioClip fireSound; // Assign the gun's firing sound in the Inspector
    [SerializeField] protected AudioClip reloadSound; // Assign the gun's Reload sound in the Inspector
    
    [Header("Corruption")]
    public bool isCorrupted;
    public float corruptionRate = 1.2f;
  
    [Header("Hold Type")]
    public bool isTwoHanded = false;
    public Transform secondHandHold;
  
    [Header("Base Stats")]
    public float damage;
    public bool useSpread = true;
    public float bulletSpreadAngle = 2f; // Angle in degrees for bullet spread
    public float reloadTime = 2f; // Time it takes to reload
    [Header("Pool Settings")]
    [SerializeField] protected int initialPoolSize = 20;
    protected ProjectilePool projectilePool;
    protected GeneralItemsPool vfxPool;
    [Header("Ammo Variables")]
    public int bulletCount;
    public int bulletCapacity;
    public int totalAmmo;
    public bool limitedAmmo = false;

    [Header("Bullet Type")]
    public bool isHitScan; // Whether the weapon is hitscan or projectile based
    public float hitScanDistance = 50;
    [Header("Projectile Settings"), Tooltip("If Bullet is not a Hitscan")]
    [SerializeField] protected float projectileSpeed = 20f;
    [SerializeField] protected float projectileLifetime = 5f;
    [SerializeField] protected bool useGravity = false;
    [SerializeField] protected float projectileArcHeight = 0f; // For arcing projectiles
    [SerializeField] protected bool canBounce = false;
    [SerializeField] protected int maxBounces = 3;
    [SerializeField] protected float bounceDamageMultiplier = 0.7f; // Reduce damage with each bounce
    private bool isReloading;
    public bool IsReloading { get { return isReloading; } }

    protected virtual void Awake()
    {
        // Initialize the projectile pool
        if (bulletPrefab != null)
        {// Create a parent object for the pool
            GameObject poolParent = new GameObject($"Pool_{bulletPrefab.name}");
            Debug.Log("Made Pool: " + weaponName);
            poolParent.transform.SetParent(transform.root);

            if (isHitScan)
            {
                vfxPool = new GeneralItemsPool(bulletPrefab, poolParent.transform, initialPoolSize);
            }
            else
            {
                projectilePool = new ProjectilePool(bulletPrefab, poolParent.transform, initialPoolSize);
            }
        }
    }

    // Fire method to be implemented by subclasses
    public virtual void Fire()
    {
        if (bulletCount <= 0 && !IsReloading)
        {
            PlaySFX(reloadSound, false);
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
            bulletCount--;
        }
    }
    protected void FireProjectile(Transform firePoint, Camera playerCamera)
    {
        PlaySFX(fireSound, false);
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(playerCamera.transform.forward);
        // Rotate player first before shooting
        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
        // Get projectile from pool
        ProjectileBehavior projectile = projectilePool.Get(
            firePoint.position,
            Quaternion.LookRotation(shootDirection));

        if (projectile != null)
        {
            ProjectileData data = new()
            {
                Direction = shootDirection,
                Speed = projectileSpeed,
                Damage = damage,
                Lifetime = projectileLifetime,
                UseGravity = useGravity,
                ArcHeight = projectileArcHeight,
                CanBounce = canBounce,
                MaxBounces = maxBounces,
                BounceDamageMultiplier = bounceDamageMultiplier
            };
            projectile.Initialize(data, projectilePool);
        }
    }
    protected void FireHitscan(Camera playerCamera)
    {
        PlaySFX(fireSound, false);
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(playerCamera.transform.forward);
        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            Debug.Log("Rotating on Fire");
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
   
        Ray ray = new(playerCamera.transform.position, shootDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, hitScanDistance, hitLayers)) // Range of the hitscan weapon
        {
            // Get projectile from pool
            GameObject bullet = vfxPool.Get(hit.point,Quaternion.LookRotation(shootDirection));

            // Apply damage to the hit object
            if (hit.collider.CompareTag("Enemy"))
            {
                if (hit.transform.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }
    // Method to calculate a bullet spread direction
    public Vector3 GetSpreadDirection(Vector3 baseDirection)
    {
        if (useSpread)
        {
            // Randomly offset the direction within a cone defined by bulletSpreadAngle
            float spreadX = Random.Range(-bulletSpreadAngle, bulletSpreadAngle);
            float spreadY = Random.Range(-bulletSpreadAngle, bulletSpreadAngle);
            Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
            return spreadRotation * baseDirection;
        }
        else
        {
            return baseDirection;
        }
    
    }
    public virtual void StartReload()
    {
        if (!IsReloading && (!limitedAmmo) || bulletCount < bulletCapacity )
        {
            StartCoroutine(ReloadCoroutine());
        }
    }
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        if (player.pUI != null)
            player.pUI.AmmoDisplay(this);

        if (limitedAmmo)
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

        isReloading = false;
        if (player.pUI != null)
            player.pUI.AmmoDisplay(this);
    }
    public void CancelReload()
    {
        StopCoroutine(ReloadCoroutine());

        isReloading = false;
        if (player.pUI != null)
            player.pUI.AmmoDisplay(this);
    }
    protected virtual void PlaySFX(AudioClip soundFX, bool loop)
    {
        if (soundFX == null)
            return;
        player.pController.audioSource.volume = fireSoundVolume;
        // Set if looping or not
        player.pController.audioSource.loop = loop;
        // Play the firing sound on the player audio source
        player.pController.audioSource.PlayOneShot(soundFX);

    }
}
