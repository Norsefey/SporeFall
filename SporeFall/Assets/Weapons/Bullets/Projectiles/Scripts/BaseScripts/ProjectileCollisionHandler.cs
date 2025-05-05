using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollisionHandler : MonoBehaviour
{
    private LayerMask hitLayers;
    private BaseProjectile projectile;
    private ProjectileMovement movement;

    [SerializeField] private float collisionCheckRadius = 0.5f;

    private void Awake()
    {
        movement = GetComponent<ProjectileMovement>();
    }

    public void Initialize(LayerMask layers, BaseProjectile owner)
    {
        hitLayers = layers;
        projectile = owner;
    }
    private void Update()
    {
        // For arc or kinematic projectiles, we need to manually check for collisions
        if (movement != null && (GetComponent<Rigidbody>()?.isKinematic ?? true))
        {
            CheckForCollisions();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision object's layer is in our hitLayers mask
        if (hitLayers == (hitLayers | (1 << collision.gameObject.layer)))
        {
            if (projectile != null)
                projectile.OnHit(collision.collider);
        }
    }
    private void CheckForCollisions()
    {
        if (movement == null) return;

        // Calculate movement direction
        Vector3 previousPosition = movement.GetPreviousPosition();
        Vector3 movementDirection = (transform.position - previousPosition).normalized;
        float movementDistance = Vector3.Distance(previousPosition, transform.position);

        if (movementDistance < 0.001f) return; // Skip if barely moving

        // Cast a sphere in the direction of movement
        RaycastHit hit;
        if (Physics.SphereCast(previousPosition, collisionCheckRadius, movementDirection,
                               out hit, movementDistance, hitLayers))
        {
            if (projectile != null)
                projectile.OnHit(hit.collider);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
}
