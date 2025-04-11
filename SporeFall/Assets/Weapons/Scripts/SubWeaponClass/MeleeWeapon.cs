using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackArc = 90f; // Attack arc in degrees
    //[SerializeField] private float comboWindow = 1.5f; // Time window to perform next combo
    [SerializeField] private float attackCooldown = 0.2f; // Cooldown between attacks
    [SerializeField] private bool freeflow = true;
    [Header("Combo System")]
    [SerializeField] private List<ComboAttack> comboAttacks;
    private int currentComboIndex = 0;
    private float lastAttackTime;
    private bool canAttack = true;
    private bool isComboInProgress = false;
    private Coroutine autoComboCoroutine;

    [System.Serializable]
    private class ComboAttack
    {
        public float damage;
        public float attackSpeed;
        public float knockbackForce;
        public AudioClip attackSound;
        public string animationTrigger;
        public float animationDuration; // Duration of this attack's animation
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
        // Start combo sequence if player is holding the fire button
        if (player.isFiring && !isComboInProgress && canAttack)
        {
            StartComboSequence();
        }
        // Single attack if just clicked
        else if (!player.isFiring && canAttack && !isComboInProgress)
        {
            PerformSingleAttack();
        }
    }

    private void StartComboSequence()
    {
        isComboInProgress = true;
        currentComboIndex = 0;
        if (autoComboCoroutine != null)
            StopCoroutine(autoComboCoroutine);
        autoComboCoroutine = StartCoroutine(AutoCombo());
    }

    private void PerformSingleAttack()
    {
        currentComboIndex = 0; // Always use first attack
        PerformAttack(comboAttacks[currentComboIndex]);
        canAttack = false;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AutoCombo()
    {
        while (player.isFiring && currentComboIndex < comboAttacks.Count)
        {
            player.TogglePControl(false);
            if (canAttack)
            {
                ComboAttack currentAttack = comboAttacks[currentComboIndex];
                PerformAttack(currentAttack);
                canAttack = false;
                StartCoroutine(AttackCooldown());

                // Wait for animation to complete before next attack
                yield return new WaitForSeconds(currentAttack.animationDuration);
                currentComboIndex++;
            }
            yield return null;
        }

        // Reset combo when sequence ends or player releases button
        player.TogglePControl(true);
        isComboInProgress = false;
        currentComboIndex = 0;
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

    // This method can be called when the player releases the fire button early
    public void CancelCombo()
    {
        if (autoComboCoroutine != null)
        {
            StopCoroutine(autoComboCoroutine);
        }
        isComboInProgress = false;
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
