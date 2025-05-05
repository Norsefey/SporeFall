using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    private Rigidbody rb;
    private ProjectileData data;

    // Arc trajectory variables
    private Vector3 initialPosition;
    private float arcDistance;
    private float arcTraveledDistance = 0;
    private Vector3 previousPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(ProjectileData projectileData)
    {
        data = projectileData;
        initialPosition = transform.position;
        previousPosition = transform.position;

        if (data.UseArcTrajectory)
        {
            SetupArcTrajectory();
        }
        else
        {
            SetupDirectTrajectory();
        }
    }

    private void SetupArcTrajectory()
    {
        arcDistance = Vector3.Distance(initialPosition, data.TargetPosition);
        arcTraveledDistance = 0f;

        // Disable rigidbody physics for arc trajectories
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Ensure we have a valid direction
        if (data.Direction.magnitude <= 0)
        {
            data.Direction = (data.TargetPosition - initialPosition).normalized;
        }
    }

    private void SetupDirectTrajectory()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = data.UseGravity;
            rb.velocity = data.Direction * data.Speed;
        }
    }

    private void Update()
    {
        previousPosition = transform.position;

        if (data.UseArcTrajectory)
        {
            UpdateArcMovement();
        }
        else if (!data.UseGravity && rb.isKinematic)
        {
            // Direct movement for non-physics projectiles
            transform.position += data.Direction * data.Speed * Time.deltaTime;
        }
    }

    private void UpdateArcMovement()
    {
        // Add constant distance each frame based on speed
        float distanceThisFrame = data.Speed * Time.deltaTime;
        arcTraveledDistance += distanceThisFrame;

        // Calculate arc progress (0 to 1) based on traveled distance
        float arcProgress = Mathf.Clamp01(arcTraveledDistance / arcDistance);

        // Calculate position along the arc
        Vector3 linearPosition = Vector3.Lerp(initialPosition, data.TargetPosition, arcProgress);

        // Add arc height (parabolic motion)
        float heightOffset = data.ArcHeight * Mathf.Sin(arcProgress * Mathf.PI);
        transform.position = linearPosition + Vector3.up * heightOffset;

        // Rotate to face the direction of movement
        if (arcProgress < 1)
        {
            // Calculate the next position for orientation
            float nextProgress = Mathf.Clamp01((arcTraveledDistance + 0.1f) / arcDistance);
            Vector3 nextPosition = Vector3.Lerp(initialPosition, data.TargetPosition, nextProgress);
            nextPosition += Vector3.up * (data.ArcHeight * Mathf.Sin(nextProgress * Mathf.PI));

            // Only update rotation if there's a meaningful difference in position
            if (Vector3.Distance(transform.position, nextPosition) > 0.001f)
            {
                transform.LookAt(nextPosition);
            }
        }

        // Check if arc is completed
        if (arcProgress >= 1)
        {
            // Notify listeners that we've reached the target position
            SendMessage("OnArcComplete", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void Bounce(Collider surface)
    {
        if (data.UseArcTrajectory)
        {
            // Handle bouncing for arc trajectories
            Vector3 currentDirection = (data.TargetPosition - initialPosition).normalized;
            Vector3 reflection = Vector3.Reflect(currentDirection, surface.transform.up);

            // Update target position based on reflection
            float remainingDistance = arcDistance * (1 - (arcTraveledDistance / arcDistance));
            data.TargetPosition = transform.position + (reflection * remainingDistance);

            // Reset arc parameters for the bounce
            initialPosition = transform.position;
            arcDistance = Vector3.Distance(initialPosition, data.TargetPosition);
            arcTraveledDistance = 0;
        }
        else if (rb != null && !rb.isKinematic)
        {
            // Handle physics-based bouncing
            Vector3 reflection = Vector3.Reflect(rb.velocity, surface.transform.up);
            rb.velocity = reflection;
        }
    }

    public Vector3 GetPreviousPosition()
    {
        return previousPosition;
    }

}
