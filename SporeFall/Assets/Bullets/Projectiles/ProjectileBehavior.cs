using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// Projectile data structure
[Serializable]
public struct ProjectileData
{
    public Vector3 Direction;
    public float Speed;
    public float Damage;
    public float Lifetime;
    public bool UseGravity;
    public float ArcHeight;
    public bool CanBounce;
    public int MaxBounces;
    public float BounceDamageMultiplier;
}
public enum ProjectileType
{
    Standard,
    Explosive,
    DOT, 
    Corrupted
}
public class ProjectileBehavior : MonoBehaviour
{
    private ProjectileData data;
    private ProjectilePool pool;
    private float elapsedTime;
    private Rigidbody rb;
    private int bounceCount;
    private float damage;

    [Header("General Settings")]
    [SerializeField] private ProjectileType type;
    [SerializeField] protected LayerMask hitLayers;
    [SerializeField] private GameObject vfxPrefab;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioPlayer;
    public AudioClip bulletSound; // Assign the bullet sound clip in the inspector
    [Range(0f, 1f)] public float bulletSoundVolume = 0.5f; // Set the default volume in the inspector

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionRadius;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("DOT Zone Settings")]
    [SerializeField] private bool createsDOTZone;
    [SerializeField] private GameObject dotZonePrefab;
    [SerializeField] private float dOTDuration;
    [SerializeField] private float dOTTickRate;
    [SerializeField] private float dOTDamagePerTick;
    [SerializeField] private float dOTRadius;

    [Header("Corruption Settings")]
    [SerializeField] private float corruptionAmount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if(audioPlayer != null)
            audioPlayer.volume = bulletSoundVolume;
    }

    public void Initialize(ProjectileData projectileData, ProjectilePool pool)
    {
        data = projectileData;
        damage = data.Damage;
        this.pool = pool;
        elapsedTime = 0f;

        if (rb != null)
        {
            rb.useGravity = data.UseGravity;

            if (data.UseGravity && data.ArcHeight > 0)
            {
                // Calculate velocity for arcing projectile
                Vector3 velocity = data.Direction * data.Speed;
                velocity.y += data.ArcHeight;
                rb.velocity = velocity;
            }
            else
            {
                rb.velocity = data.Direction * data.Speed;
            }
        }
        if(gameObject.activeSelf)
            StartCoroutine(LifetimeCounter());
    }
    private IEnumerator LifetimeCounter()
    {
        while (elapsedTime < data.Lifetime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ReturnToPool();
    }
    private void ReturnToPool()
    {
        Debug.Log($"Returning to pool {name}");
        StopAllCoroutines();
        if (pool != null)
        {
            pool.Return(this);
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision object's layer is in our hitLayers mask
        if (hitLayers == (hitLayers | (1 << collision.gameObject.layer)))
        {
            switch (type)
            {
                case ProjectileType.Standard:
                    HandleStandardAttack(collision);
                    break;
                case ProjectileType.Explosive:
                    HandleExplosiveAttack();
                    break;
                case ProjectileType.DOT:
                    HandleDOTAttack();
                    break;
                case ProjectileType.Corrupted:
                    HandleCorruptionAttack(collision);
                    break;
            }
        }
        Debug.Log("Projectile Hit" + collision.gameObject.name);

        HandleCollision(collision);

    }
    protected void ApplyDamage(Damageable target)
    {
            target.TakeDamage(damage);
    }
    protected void HandleCollision(Collision collision)
    {
        if (audioPlayer != null)
            audioPlayer.PlayOneShot(bulletSound);
        if (vfxPrefab != null)
        {// if you have a VFX assigned, play it on collision

            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(vfxPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {vfxPrefab.name}");
                //return;
            }
            else
            {
                VFXPoolingBehavior vfx = pool.Get(transform.position, transform.rotation);
                vfx.Initialize(pool);
            }
        }
        if (data.CanBounce && bounceCount < data.MaxBounces)
        {
            Bounce(collision.collider);
        }
        else
        {
            ReturnToPool();
        }
    }
    protected void Bounce(Collider surface)
    {
        if (rb != null)
        {
            Vector3 reflection = Vector3.Reflect(rb.velocity, surface.transform.up);
            rb.velocity = reflection;
            damage *= data.BounceDamageMultiplier;
            bounceCount++;
        }
    }
    private void HandleStandardAttack(Collision collision)
    {
        if (collision.collider.TryGetComponent<Damageable>(out var damageable))
        {
            ApplyDamage(damageable);
        }
    }
    private void HandleExplosiveAttack()
    {
        // Create explosion effect if prefab is assigned
        if (explosionEffectPrefab != null)
        {
            if (!PoolManager.Instance.vfxPool.TryGetValue(explosionEffectPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for enemy prefab: {explosionEffectPrefab.name}");
                return;
            }
            VFXPoolingBehavior explosiveVfx = pool.Get(transform.position, transform.rotation);
            explosiveVfx.Initialize(pool);
        }

        // Use OverlapSphere with the layer mask
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);
        foreach (var hit in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

            if (hit.TryGetComponent<Damageable>(out var damageable))
            {
                damageable.TakeDamage(damage * damageMultiplier);
            }
        }


        ReturnToPool();
    }
    private void HandleDOTAttack()
    {
        if (dotZonePrefab != null && createsDOTZone)
        {
            GameObject zoneObject = Instantiate(dotZonePrefab, transform.position, Quaternion.identity);

            if (zoneObject.TryGetComponent<DamageOverTimeZone>(out var dotZone))
            {
                // Pass the hitLayers instead of hitTag
                dotZone.Initialize(dOTDuration, dOTTickRate, dOTDamagePerTick, dOTRadius, hitLayers);
            }
        }

        ReturnToPool();
    }
    private void HandleCorruptionAttack(Collision collision)
    {
        if (collision.transform.TryGetComponent<Damageable>(out var damageable))
        {
            if (damageable.canHoldCorruption)
            {
                damageable.IncreaseCorruption(corruptionAmount);
            }
            damageable.TakeDamage(damage);
        }
    }
}
