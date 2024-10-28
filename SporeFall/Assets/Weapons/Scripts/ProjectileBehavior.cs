using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
public class ProjectileBehavior : MonoBehaviour
{
    private ProjectileData data;
    private Rigidbody rb;
    private int bounceCount;
    private float currentDamage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(ProjectileData projectileData)
    {
        data = projectileData;
        currentDamage = data.Damage;

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

        Destroy(gameObject, data.Lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent<Damageable>(out var damageable))
        {
            damageable.TakeDamage(currentDamage);

            if (!data.CanBounce || bounceCount >= data.MaxBounces)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (data.CanBounce && bounceCount < data.MaxBounces)
        {
            Bounce(collision.collider);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Bounce(Collider surface)
    {
        if (rb != null)
        {
            Vector3 reflection = Vector3.Reflect(rb.velocity, surface.transform.up);
            rb.velocity = reflection;
            currentDamage *= data.BounceDamageMultiplier;
            bounceCount++;
        }
    }
}
