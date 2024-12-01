using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Explosion Attack", menuName = "Enemy/Attacks/Explosion Attack")]
public class ExplosiveAttack : Attack
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private bool destroySelfOnExplode = true;

    /*[Header("Additional Effects")]
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float upwardsModifier = 3f;*/

    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target)
    {
        // Start the attack cooldown
        StartCooldown();

        // Play animation if specified
        if (!string.IsNullOrEmpty(animationTrigger) && enemy.Animator != null)
        {
            enemy.Animator.SetTrigger(animationTrigger);
        }

        // Wait for attack delay
        yield return new WaitForSeconds(attackDelay);

        // Play effects
        PlaySFX(enemy.GetComponent<AudioSource>());
        SpawnVFX(enemy.transform.position, Quaternion.identity);

        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(enemy.transform.position, explosionRadius, damageableLayers);

        foreach (Collider hit in hitColliders)
        {
            // Calculate distance for damage falloff
            float distance = Vector3.Distance(enemy.transform.position, hit.transform.position);
            float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

            // Apply damage if object has IDamageable interface
            Damageable damageable = hit.GetComponent<Damageable>();
            if (damageable != null)
            {
                if(hit.CompareTag("structure"))
                damageable.TakeDamage(damage * damageMultiplier);
            }

          /*  // Add explosion force to rigidbodies
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, enemy.transform.position, explosionRadius, upwardsModifier);
            }*/
        }

        // Wait for recovery time
        yield return new WaitForSeconds(recoveryTime);

        if (destroySelfOnExplode)
        {
            enemy.Die();
        }
    }
}
