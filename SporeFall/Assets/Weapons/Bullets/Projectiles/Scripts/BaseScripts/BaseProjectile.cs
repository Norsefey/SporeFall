using System.Collections;
using UnityEngine;

/// Base class for all projectiles with common functionality
public abstract class BaseProjectile : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] protected LayerMask hitLayers;
    // Components
    protected ProjectileMovement movement;
    protected ProjectileCollisionHandler collisionHandler;
    protected ProjectileEffects effects;

    // State
    protected ProjectileData data;
    protected ProjectilePool pool;
    protected float elapsedTime;
    protected float currentDamage;
    protected int bounceCount;

    protected virtual void Awake()
    {
        movement = GetComponent<ProjectileMovement>();
        collisionHandler = GetComponent<ProjectileCollisionHandler>();
        effects = GetComponent<ProjectileEffects>();
    }

    public virtual void Initialize(ProjectileData projectileData, ProjectilePool projectilePool)
    {
        data = projectileData;
        pool = projectilePool;
        currentDamage = data.Damage;
        elapsedTime = 0f;
        bounceCount = 0;

        // Initialize movement
        if (movement != null)
            movement.Initialize(data);

        // Initialize collision handler
        if (collisionHandler != null)
            collisionHandler.Initialize(hitLayers, this);

        if (gameObject.activeSelf)
            StartCoroutine(LifetimeCounter());
    }
    protected IEnumerator LifetimeCounter()
    {
        while (elapsedTime < data.Lifetime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ReturnToPool();
    }
    public virtual void ReturnToPool()
    {
        StopAllCoroutines();
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }

    public virtual void OnHit(Collision collision)
    {
        // Play sound and visual effects
        if (effects != null)
            effects.PlayImpactEffects(collision.contacts[0].point);

        // Handle damage application
        HandleImpact(collision);

        // Handle collision physics (bounce or destroy)
        if (data.CanBounce && bounceCount < data.MaxBounces)
        {
            Bounce(collision.collider);
        }
        else
        {
            ReturnToPool();
        }
    }
    protected virtual void Bounce(Collider surface)
    {
        if (movement != null)
            movement.Bounce(surface);

        currentDamage *= data.BounceDamageMultiplier;
        bounceCount++;
    }
    /// Apply damage based on projectile type
    protected abstract void HandleImpact(Collision collision);
    /// Apply damage to a damageable target
    protected void ApplyDamage(Damageable target, float damageAmount)
    {
        if (target != null)
            target.TakeDamage(damageAmount);
    }
}
