using System.Collections;
using UnityEngine;


public abstract class Weapon : MonoBehaviour
{
    [Header("References")]
    public string weaponName;
    public Sprite weaponSprite;
    public PlayerManager player;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public LayerMask hitLayers;

    [Header("Audio Clips")] // uses audio player on player to play sfx
    [SerializeField, Range(0f, 1f)] protected float fireSoundVolume = 0.5f;
    [SerializeField] protected AudioClip fireSound;
    [SerializeField] protected AudioClip reloadSound;
    
    [Header("Corruption")]
    public bool isCorrupted;
    public float corruptionRate = 1.2f;

    [Header("Hold Type")]// animation hold changes based on hold type
    public int holdType;
    public Transform secondHandHold;
  
    [Header("Base Stats")]
    public float damage;
    public bool useSpread = true;
    public float bulletSpreadAngle = 2f; // Angle in degrees for bullet spread
    public float reloadTime = 2f; // Time it takes to reload
    public float knockBackForce = 0;
    [Header("Ammo Variables")]
    public int bulletCount;
    public int bulletCapacity;
    public int totalAmmo;
    public bool limitedAmmo = false;

    [Header("Bullet Type")]
    public bool isHitScan;
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


    // Fire method to be implemented by subclasses
    public virtual void Fire()
    {
        if (bulletCount <= 0 && !IsReloading)
        {// auto reload if player tries shooting at 0 ammo clip
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
        {// don't rotate player if aiming, rotation is handled in playerMovement script
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
        transform.forward = playerCamera.transform.forward;

        if (!PoolManager.Instance.projectilePool.TryGetValue(bulletPrefab, out ProjectilePool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {bulletPrefab.name}");
            return;
        }
        // Get projectile from pool and initialize it
        ProjectileBehavior projectile = pool.Get(
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
            projectile.Initialize(data, pool);
        }
    }
    protected void FireHitscan(Camera playerCamera)
    {
        PlaySFX(fireSound, false);
        // Apply bullet spread
        Vector3 shootDirection = GetSpreadDirection(playerCamera.transform.forward);
        if (player.pController.currentState != PlayerMovement.PlayerState.Aiming)
        {
            player.pController.RotateOnFire(this.transform, shootDirection);
        }
        transform.forward = playerCamera.transform.forward;
        
        // Get VFX from pool
        if (!PoolManager.Instance.vfxPool.TryGetValue(bulletPrefab, out VFXPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {bulletPrefab.name}");
            return;
        }
        VFXPoolingBehavior vfx = pool.Get(firePoint.position, transform.rotation);
        vfx.Initialize(pool);

        Ray ray = new(playerCamera.transform.position, shootDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, hitScanDistance, hitLayers)) // Range of the hitscan weapon
        {
            Debug.Log("Hit" + hit.transform.gameObject.name);

            // Apply damage to the hit object
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("HeadShot"))
            {
                if (hit.transform.TryGetComponent<EnemyHPRelay>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                    damageable.KnockBack(transform.position, knockBackForce);
                }
            }
            vfx.MoveToLocation(hit.point, 50);
        }
        else
        {
            vfx.MoveForward();
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
        if (!IsReloading && bulletCount < bulletCapacity )
        {
            StartCoroutine(ReloadCoroutine());
        }
    }
    private IEnumerator ReloadCoroutine()
    {
        Debug.Log("Starting Reload");
        isReloading = true;
        if (player.pUI != null)
            player.pUI.AmmoDisplay(this);

        if (limitedAmmo)
        {
            if (totalAmmo <= 0)
            {
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
