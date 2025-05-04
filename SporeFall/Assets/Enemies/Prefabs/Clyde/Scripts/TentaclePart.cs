using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TentacleType
{
    Melee,
    Ranged
}
public class TentaclePart : MonoBehaviour
{
    [Header("Tentacle Settings")]
    [SerializeField] private TentacleType tentacleType;
    [SerializeField] private Damageable health;
    [SerializeField] private Animator tentacleAnimator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Attack[] tentacleAttacks;

    [Header("Targeting")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float meleeAttackSpeed = 1.5f;
    [SerializeField] private float rangedAttackSpeed = 2f;

    private OctoBoss mainBody;
    private Transform currentTarget;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDestroyed = false;

    // Animation parameters
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int DestroyedTrigger = Animator.StringToHash("Destroyed");

    public void Initialize(OctoBoss mainBody)
    {
        this.mainBody = mainBody;

        // Subscribe to health events
        if (health == null)
            health = GetComponent<Damageable>();

        if (health != null)
        {
            // If using EnemyHP, we need to set it up to work with the tentacle
            EnemyHP enemyHP = health as EnemyHP;
            if (enemyHP != null)
            {
                enemyHP.flinchable = true;
            }
        }
    }

    public bool IsActive()
    {
        return gameObject.activeSelf && !isDestroyed;
    }

    private void Update()
    {
        if (!IsActive() || isAttacking || mainBody == null)
            return;

        // Get target from main body
        currentTarget = mainBody.currentTarget;
        if (currentTarget == null)
            return;

        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(attackPoint.position, currentTarget.position);

        // Check if in range based on tentacle type
        bool inRange = false;

        if (tentacleType == TentacleType.Melee)
        {
            inRange = distanceToTarget <= attackRange;
        }
        else // Ranged
        {
            // Ranged attacks have a minimum distance too
            inRange = distanceToTarget <= attackRange && distanceToTarget > attackRange * 0.3f;
        }

        // If in range and cooldown is over, attack
        if (inRange && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
        }

        // Always rotate to face target
        RotateTowardsTarget();
    }

    private void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0; // Keep rotation on y-axis only

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float speed = tentacleType == TentacleType.Melee ? meleeAttackSpeed : rangedAttackSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }

    private void Attack()
    {
        Attack bestAttack = ChooseBestAttack();
        if (bestAttack == null)
            return;

        StartCoroutine(ExecuteAttackSequence(bestAttack));
    }

    private Attack ChooseBestAttack()
    {
        if (tentacleAttacks.Length == 0)
            return null;

        // Filter attacks that are off cooldown
        var readyAttacks = new System.Collections.Generic.List<Attack>();
        float distanceToTarget = Vector3.Distance(attackPoint.position, currentTarget.position);

        foreach (var attack in tentacleAttacks)
        {
            if (attack.CanUse(distanceToTarget))
            {
                readyAttacks.Add(attack);
            }
        }

        if (readyAttacks.Count == 0)
            return null;

        // Choose random attack from ready attacks
        return readyAttacks[Random.Range(0, readyAttacks.Count)];
    }

    private IEnumerator ExecuteAttackSequence(Attack attackToUse)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Play animation
        if (tentacleAnimator != null)
        {
            tentacleAnimator.SetTrigger(AttackTrigger);
        }

        // Execute the actual attack
        yield return attackToUse.ExecuteAttack(mainBody, currentTarget);

        isAttacking = false;
    }

    public void DestroyTentacle()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        // Play destruction animation
        if (tentacleAnimator != null)
        {
            tentacleAnimator.SetTrigger(DestroyedTrigger);
        }

        // Notify main body
        mainBody.OnTentacleDestroyed(this);

        // Disable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Could play VFX, sound, etc. here

        // Wait for animation to finish then disable
        StartCoroutine(DisableAfterAnimation());
    }

    private IEnumerator DisableAfterAnimation()
    {
        // Wait for animation to finish
        yield return new WaitForSeconds(2.0f);

        // Disable the tentacle
        gameObject.SetActive(false);
    }
}
