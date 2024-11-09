using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// Projectile data structure
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
    DOT
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
    [SerializeField] private string hitTag;
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
        if(string.IsNullOrEmpty(hitTag))
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
            }
            HandleCollision(collision);
        }
        else
        {
            switch (type)
            {
                case ProjectileType.Standard:
                    if (collision.collider.CompareTag(hitTag))
                        HandleStandardAttack(collision);
                    break;
                case ProjectileType.Explosive:
                    HandleExplosiveAttack();
                    break;
                case ProjectileType.DOT:
                    HandleDOTAttack();
                    break;
            }
            HandleCollision(collision);
        }
       
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
                return;
            }
            VFXPoolingBehavior vfx = pool.Get(transform.position, transform.rotation);
            vfx.Initialize(pool);
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
            VFXPoolingBehavior vfx = pool.Get(transform.position, transform.rotation);
            vfx.Initialize(pool);
        }

        // Damage all targets in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            // Calculate distance for damage falloff
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

            // Apply damage if object has IDamageable interface
            Damageable damageable = hit.GetComponent<Damageable>();
            if (damageable != null)
            {
                if(string.IsNullOrEmpty(hitTag))// If theres no tag damage all in range
                    damageable.TakeDamage(damage * damageMultiplier);
                else if (damageable.CompareTag(hitTag))// if there is a tag only damage if tag matches
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
                dotZone.Initialize(dOTDuration, dOTTickRate, dOTDamagePerTick, dOTRadius, hitTag);
            }
        }

        ReturnToPool();
    }

}
