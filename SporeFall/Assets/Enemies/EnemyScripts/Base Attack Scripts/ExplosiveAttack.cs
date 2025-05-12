using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Explosion Attack", menuName = "Enemy/Attacks/Explosion Attack")]
public class ExplosiveAttack : Attack
{
    [Header("Explosion Settings")]
    [Tooltip("How far the explosion can do damage")]
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private LayerMask damageableLayers;
    [Tooltip("Further away from center of explosion less damage")]
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private bool destroySelfOnExplode = true;
    public override IEnumerator ExecuteAttack(BaseEnemy enemy, Transform target, float damageModifier)
    {
        damage *= damageModifier;
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
            Debug.Log(hit.name + "");
            // Calculate distance for damage falloff
            float distance = Vector3.Distance(enemy.transform.position, hit.transform.position);
            float damageMultiplier = damageFalloff.Evaluate(distance / explosionRadius);

            // Apply damage if object has IDamageable interface
            Damageable damageable = hit.GetComponent<Damageable>();
            if (damageable != null)
            {
                Debug.Log("Eplosive Damage Amount: " + (damage * damageMultiplier));
                damageable.TakeDamage((damage * damageMultiplier));
            }
        }

        // Wait for recovery time
        yield return new WaitForSeconds(recoveryTime);

        if (destroySelfOnExplode)
        {
            enemy.Die();
        }
    }

}
