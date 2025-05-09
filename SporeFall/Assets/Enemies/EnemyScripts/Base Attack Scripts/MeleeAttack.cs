// Ignore Spelling: Melee
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Attack", menuName = "Enemy/Attacks/Melee Attack")]
public class MeleeAttack : Attack
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackArc = 90f;
    [SerializeField] private float attackRange = 5;
    [SerializeField] private LayerMask targetLayers;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color rangeColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color arcColor = new Color(1f, 0.5f, 0f, 0.5f);
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float debugDuration = 1.5f;

    private List<Vector3> hitPositions = new List<Vector3>();
    private Vector3 lastAttackOrigin;
    private Quaternion lastAttackRotation;
    private float lastAttackTime;

    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target)
    {
        // Begin attack sequence
        enemy.SetIsAttacking(true);
        if(enemy.Animator != null)
            enemy.Animator.SetTrigger(animationTrigger);
        Coroutine trackingCoroutine = enemy.StartCoroutine(TrackTarget(enemy, target));

        // Wait for wind-up
        yield return new WaitForSeconds(attackDelay);
        // Stop tracking once the delay is complete
        if (trackingCoroutine != null)
        {
            enemy.StopCoroutine(trackingCoroutine);
        }

        // Store attack origin and rotation for debug visualization
        lastAttackOrigin = enemy.firePoint.position;
        lastAttackRotation = enemy.transform.rotation;
        lastAttackTime = Time.time;
        hitPositions.Clear();

        // Perform the attack
        Vector3 attackOrigin = enemy.firePoint.position;
        Collider[] hits = Physics.OverlapSphere(attackOrigin, attackRange, targetLayers);

        bool hitTarget = false;

        foreach (Collider hit in hits)
        {
            // Check if target is within attack arc
            Vector3 directionToTarget = (hit.transform.position - attackOrigin).normalized;
            float angle = Vector3.Angle(enemy.transform.forward, directionToTarget);

            if (angle <= attackArc * 0.5f)
            {
                if (hit.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                    SpawnVFX(hit.transform.position, enemy.transform.rotation);
                    hitPositions.Add(hit.transform.position);
                    hitTarget = true;

                    // Draw debug line to hit target
                    if (showDebugGizmos)
                    {
                        Debug.DrawLine(lastAttackOrigin, hit.transform.position, hitColor, debugDuration);
                    }
                }
            }
        }

        PlaySFX(enemy.AudioSource);
        // Log attack result
        if (showDebugGizmos)
        {
            Debug.Log($"Melee Attack! Hit target: {hitTarget}, Hits: {hitPositions.Count}");
        }

        StartCooldown();
        // Recovery period
        yield return new WaitForSeconds(recoveryTime);
        enemy.SetIsAttacking(false);
    }

    // track the target during attack charge-up
    private IEnumerator TrackTarget(BaseEnemy enemy, Transform target)
    {
        float trackingSpeed = enemy.trackingSpeed;

        while (target != null)
        {
            // Smoothly rotate to face the target
            Vector3 directionToTarget = (target.position - enemy.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * trackingSpeed);

            yield return null;
        }
    }

    public void DrawGizmosForEnemy(BaseEnemy enemy)
    {
        if (!showDebugGizmos || enemy == null)
            return;

        Vector3 origin = enemy.firePoint != null ? enemy.firePoint.position : enemy.transform.position;

        // Draw attack range sphere
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(origin, attackRange);

        // Draw attack arc
        DrawAttackArc(origin, enemy.transform.forward, attackArc, attackRange, arcColor);

        // Draw hit positions from last attack if still within debug duration
        if (Time.time - lastAttackTime <= debugDuration)
        {
            foreach (Vector3 hitPos in hitPositions)
            {
                Gizmos.color = hitColor;
                Gizmos.DrawSphere(hitPos, 0.2f);
            }
        }
    }
    // Helper method to draw attack arc
    private void DrawAttackArc(Vector3 origin, Vector3 forward, float arcAngle, float radius, Color color)
    {
        Vector3 rightDir = Quaternion.Euler(0, arcAngle * 0.5f, 0) * forward;
        Vector3 leftDir = Quaternion.Euler(0, -arcAngle * 0.5f, 0) * forward;

        Gizmos.color = color;

        // Draw arc rays
        Gizmos.DrawRay(origin, rightDir * radius);
        Gizmos.DrawRay(origin, leftDir * radius);

        // Draw arc segments
        int segments = 20;
        Vector3 prevPos = origin + forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -arcAngle * 0.5f + (arcAngle / segments) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 pos = origin + direction * radius;

            Gizmos.DrawLine(prevPos, pos);
            prevPos = pos;
        }
    }

    // Add a way to visualize attacks in play mode
    public void VisualizeAttack(Vector3 origin, Quaternion rotation)
    {
        if (!showDebugGizmos)
            return;

        Vector3 forward = rotation * Vector3.forward;

        // Draw temporary arc visualization
        int segments = 20;
        Vector3 prevPos = origin + forward * attackRange;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -attackArc * 0.5f + (attackArc / segments) * i;
            Vector3 direction = (rotation * Quaternion.Euler(0, angle, 0)) * Vector3.forward;
            Vector3 pos = origin + direction * attackRange;

            Debug.DrawLine(prevPos, pos, arcColor, debugDuration);
            prevPos = pos;
        }

        // Draw attack range sphere using debug lines
        DrawDebugSphere(origin, attackRange, rangeColor, debugDuration);
    }

    // Helper method to draw debug sphere
    private void DrawDebugSphere(Vector3 center, float radius, Color color, float duration)
    {
        int segments = 16;
        float angle = 360f / segments;

        // Draw three circles for X, Y, Z axes
        for (int i = 0; i < segments; i++)
        {
            float currAngle = angle * i * Mathf.Deg2Rad;
            float nextAngle = angle * (i + 1) * Mathf.Deg2Rad;

            // X-Z plane
            Vector3 currPos = new Vector3(center.x + Mathf.Cos(currAngle) * radius, center.y, center.z + Mathf.Sin(currAngle) * radius);
            Vector3 nextPos = new Vector3(center.x + Mathf.Cos(nextAngle) * radius, center.y, center.z + Mathf.Sin(nextAngle) * radius);
            Debug.DrawLine(currPos, nextPos, color, duration);

            // X-Y plane
            currPos = new Vector3(center.x + Mathf.Cos(currAngle) * radius, center.y + Mathf.Sin(currAngle) * radius, center.z);
            nextPos = new Vector3(center.x + Mathf.Cos(nextAngle) * radius, center.y + Mathf.Sin(nextAngle) * radius, center.z);
            Debug.DrawLine(currPos, nextPos, color, duration);

            // Y-Z plane
            currPos = new Vector3(center.x, center.y + Mathf.Cos(currAngle) * radius, center.z + Mathf.Sin(currAngle) * radius);
            nextPos = new Vector3(center.x, center.y + Mathf.Cos(nextAngle) * radius, center.z + Mathf.Sin(nextAngle) * radius);
            Debug.DrawLine(currPos, nextPos, color, duration);
        }
    }
    // Override the SpawnVFX method to also visualize the attack
    protected override void SpawnVFX(Vector3 position, Quaternion rotation)
    {
        base.SpawnVFX(position, rotation);

        // Add visualization of the attack
        if (showDebugGizmos)
        {
            VisualizeAttack(lastAttackOrigin, lastAttackRotation);
        }
    }
}
