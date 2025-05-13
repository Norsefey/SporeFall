using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrowerGun : Weapon
{
    [Header("Flamethrower Specific")]
    [SerializeField] private ParticleSystem flameEffect;
    [SerializeField] private float damageTickRate = 0.2f;
    [SerializeField] private float maxFlameDistance = 8f;
    [SerializeField] private float flameRadius = 1.5f;
    [SerializeField] private float fuelConsumptionRate = 5f; // Units of ammo used per second

    private bool isFiring = false;
    private List<Damageable> currentTargets = new List<Damageable>();
    private Coroutine damageCoroutine;
    private Coroutine fuelConsumptionCoroutine;

    private void Awake()
    {
        // Initialize flame effect if not set in inspector
        if (flameEffect == null)
        {
            flameEffect = GetComponentInChildren<ParticleSystem>();
        }

        isHitScan = false; // We'll handle our own custom hit detection
        useSpread = true;
        bulletSpreadAngle = 5f; // Wider spread for flames
    }

    public override void Fire()
    {
        // Don't start firing if reloading or out of ammo
        if (IsReloading || (bulletCount <= 0 && totalAmmo <= 0))
        {
            if (bulletCount <= 0 && !IsReloading && totalAmmo > 0)
            {
                PlaySFX(reloadSound, false);
                StartReload();
            }
            return;
        }

        // Start continuous firing if not already firing
        if (!isFiring)
        {
            StartFiring();
        }
    }

    private void StartFiring()
    {
        isFiring = true;

        // Play flame effect
        if (flameEffect != null && !flameEffect.isPlaying)
        {
            flameEffect.Play();
        }

        // Play fire sound (looped)
        PlaySFX(fireSound, true);

        // Start damage coroutine
        damageCoroutine = StartCoroutine(DamageTickCoroutine());

        // Start fuel consumption coroutine
        fuelConsumptionCoroutine = StartCoroutine(ConsumeFuelCoroutine());
    }

    public void StopFiring()
    {
        if (!isFiring) return;

        isFiring = false;

        // Stop flame effect
        if (flameEffect != null && flameEffect.isPlaying)
        {
            flameEffect.Stop();
        }

        // Stop looped sound
        if (player != null && player.pController != null)
        {
            Transform parentTransform = player.pController.transform.parent;
            if (parentTransform != null)
            {
                AudioSource audioSource = parentTransform.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            }
        }

        // Clear target list
        currentTargets.Clear();

        // Stop coroutines
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        if (fuelConsumptionCoroutine != null)
            StopCoroutine(fuelConsumptionCoroutine);
    }

    private IEnumerator DamageTickCoroutine()
    {
        while (isFiring)
        {
            // Detect targets in flame cone
            DetectTargets();

            // Apply damage to all detected targets
            foreach (Damageable target in currentTargets)
            {
                if (target != null)
                {
                    float appliedDamage = damage * damageModifier;

                    target.TakeDamage(appliedDamage);

                    // Apply knockback if necessary
                    if (knockBackForce > 0 && target.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    {
                        Vector3 direction = (target.transform.position - player.transform.position).normalized;
                        rb.AddForce(direction * knockBackForce, ForceMode.Impulse);
                    }
                }
            }

            yield return new WaitForSeconds(damageTickRate);
        }
    }

    private IEnumerator ConsumeFuelCoroutine()
    {
        while (isFiring && bulletCount > 0)
        {
            // Calculate fuel to consume this frame
            float fuelToConsume = fuelConsumptionRate * Time.deltaTime;

            // Convert to int (minimum 1)
            int fuelConsumed = Mathf.Max(1, Mathf.FloorToInt(fuelToConsume));

            // Reduce bullet count
            bulletCount -= fuelConsumed;

            // Update UI
            if (player.pUI != null)
                player.pUI.AmmoDisplay(this);

            // Check if we need to stop or reload
            if (bulletCount <= 0 && !IsReloading)
            {
                PlaySFX(reloadSound, false);
                StartReload();
                StopFiring();
            }

            if (limitedAmmo && bulletCount <= 0 && totalAmmo <= 0)
            {
                StopFiring();
                player.DestroyCurrentWeapon();
            }
            yield return null;
        }
    }

    private void DetectTargets()
    {
        // Clear previous targets
        currentTargets.Clear();

        // Get the direction with spread
        Vector3 flameDirection = GetSpreadDirection(player.pCamera.transform.forward);

        // Perform an overlap sphere or capsule cast to find targets in flame area
        RaycastHit[] hits = Physics.SphereCastAll(
            firePoint.position,
            flameRadius,
            flameDirection,
            maxFlameDistance,
            hitLayers
        );

        foreach (RaycastHit hit in hits)
        {
            // Check if the hit object is damageable
            if (hit.collider.TryGetComponent<Damageable>(out Damageable damageable))
            {
                // Add to current targets if not already included
                if (!currentTargets.Contains(damageable))
                {
                    currentTargets.Add(damageable);
                }
            }
        }
    }

    // Override base reload to handle flamethrower specifics
    public override void StartReload()
    {
        // Stop firing when reloading
        StopFiring();

        // Call base reload
        base.StartReload();
    }

    private void OnDisable()
    {
        // Ensure we stop firing when weapon is disabled
        StopFiring();
    }

    // We need to add this additional method to enable or disable firing based on input
    public void UpdateFiringState(bool shouldFire)
    {
        if (shouldFire && !isFiring && bulletCount > 0 && !IsReloading)
        {
            Fire();
        }
        else if (!shouldFire && isFiring)
        {
            StopFiring();
        }
    }
}
