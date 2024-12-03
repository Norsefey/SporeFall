using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackArc = 90f; // Attack arc in degrees
    [SerializeField] private float comboWindow = 1.5f; // Time window to perform next combo
    [SerializeField] private float attackCooldown = 0.2f; // Cooldown between attacks

    [Header("Combo System")]
    [SerializeField] private List<ComboAttack> comboAttacks;
    private int currentComboIndex = 0;
    private float lastAttackTime;
    private bool canAttack = true;
    private Coroutine comboResetCoroutine;

    [System.Serializable]
    private class ComboAttack
    {
        public float damage;
        public float attackSpeed;
        public float knockbackForce;
        public AudioClip attackSound;
        public string animationTrigger;
    }

    private void Start()
    {
        // Initialize base weapon properties
        isHitScan = false;
        useSpread = false;
        bulletCount = 1;
        bulletCapacity = 1;
        limitedAmmo = false;
    }

    public override void Fire()
    {
        if (!canAttack) return;

        if (Time.time - lastAttackTime > comboWindow)
        {
            // Reset combo if too much time has passed
            currentComboIndex = 0;
        }

        if (currentComboIndex >= comboAttacks.Count)
        {
            // Reset combo if we've completed all attacks
            currentComboIndex = 0;
        }

        PerformAttack(comboAttacks[currentComboIndex]);

        // Update attack tracking
        lastAttackTime = Time.time;
        currentComboIndex++;
        canAttack = false;

        // Start cooldown
        StartCoroutine(AttackCooldown());

        // Reset combo after window expires
        if (comboResetCoroutine != null)
            StopCoroutine(comboResetCoroutine);
        comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());
    }

    private void PerformAttack(ComboAttack attack)
    {
        // Play attack sound
        PlaySFX(attack.attackSound, false);

        // Trigger animation if available
        if (!string.IsNullOrEmpty(attack.animationTrigger))
            player.pAnime.TriggerAnimation(attack.animationTrigger);

        // Calculate attack arc
        Vector3 forward = player.pCamera.myCamera.transform.forward;
        Vector3 attackOrigin = firePoint.position;

        // Perform spherecast to detect enemies
        Collider[] hits = Physics.OverlapSphere(attackOrigin, attackRange, hitLayers);

        foreach (Collider hit in hits)
        {
            // Check if target is within attack arc
            Vector3 directionToTarget = (hit.transform.position - attackOrigin).normalized;
            float angle = Vector3.Angle(forward, directionToTarget);

            if (angle <= attackArc / 2)
            {
                // Apply damage if target is damageable
                if (hit.TryGetComponent<Damageable>(out var damageable))
                {
                    damageable.TakeDamage(attack.damage);

                    // Apply knockback if target has rigidbody
                    if (hit.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 knockbackDirection = directionToTarget + Vector3.up * 0.2f;
                        rb.AddForce(knockbackDirection * attack.knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator ResetComboAfterDelay()
    {
        yield return new WaitForSeconds(comboWindow);
        currentComboIndex = 0;
    }

    // Override reload since melee weapons don't need to reload
    public override void StartReload()
    {
        // Do nothing - melee weapons don't reload
        return;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, attackRange);

            // Draw attack arc
            Vector3 forward = transform.forward;
            Vector3 leftBound = Quaternion.Euler(0, -attackArc / 2, 0) * forward;
            Vector3 rightBound = Quaternion.Euler(0, attackArc / 2, 0) * forward;

            Gizmos.DrawLine(firePoint.position, firePoint.position + leftBound * attackRange);
            Gizmos.DrawLine(firePoint.position, firePoint.position + rightBound * attackRange);
        }
    }
}
