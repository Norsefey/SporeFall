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
                projectile.OnHit(collision);
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
            // Handle the collision
            Collision fakeCollision = CreateFakeCollision(hit);

            if (projectile != null)
                projectile.OnHit(fakeCollision);
        }
    }

    // Helper method to create a fake collision from a raycast hit
    private Collision CreateFakeCollision(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        // Create a new Collision
        Collision collision = new Collision();

        // Use reflection to set private fields (this is a workaround since Collision is not normally constructable)
        System.Reflection.FieldInfo contactPointsField = typeof(Collision).GetField("m_ContactPoints",
                                                                                   System.Reflection.BindingFlags.Instance |
                                                                                   System.Reflection.BindingFlags.NonPublic);

        if (contactPointsField != null)
        {
            // Create a contact point
            ContactPoint contact = new ContactPoint();

            // Set contact point fields via reflection
            typeof(ContactPoint).GetField("m_Point", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, hit.point);

            typeof(ContactPoint).GetField("m_Normal", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, hit.normal);

            typeof(ContactPoint).GetField("m_ThisCollider", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, GetComponent<Collider>());

            typeof(ContactPoint).GetField("m_OtherCollider", System.Reflection.BindingFlags.Instance |
                                         System.Reflection.BindingFlags.NonPublic)?.SetValue(contact, hit.collider);

            // Set the contact points
            contactPointsField.SetValue(collision, new[] { contact });
        }

        // Set the transform and gameObject fields
        typeof(Collision).GetField("m_Transform", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)?.SetValue(collision, hit.transform);

        typeof(Collision).GetField("m_Rigidbody", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)?.SetValue(collision, hit.rigidbody);

        typeof(Collision).GetField("m_GameObject", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)?.SetValue(collision, hitObject);

        return collision;
    }
}
