using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class EnemyControls : MonoBehaviour
{
    public delegate void EnemyDeath();
    public event EnemyDeath OnEnemyDeath;

    // Health properties
    public float hp = 10;
    private float maxHP;
    [SerializeField] private TMP_Text hpDisplay;

    // NavMeshAgent component
    private NavMeshAgent navMeshAgent;

    // Attack properties
    public float stoppingDistance = 2.0f;  // Distance at which the enemy stops moving and can attack
    public float attackRange = 2.5f;       // Distance at which the enemy can deal damage
    public float attackCooldown = 2.0f;    // Time between attacks
    public float damageAmount = 5.0f;      // Damage dealt to the target

    private float lastAttackTime;

    // Range within which the enemy can target an object
    public float targetingRange = 15.0f;

    // Array to hold multiple valid target tags
    public string[] targetTags; // e.g., "Player", "Ally"

    void Start()
    {
        maxHP = hp;
        UpdateHPDisplay();

        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.stoppingDistance = stoppingDistance;
        lastAttackTime = -attackCooldown; // Allow immediate attack when starting
    }

    void Update()
    {
        // Ensure the agent is on a valid NavMesh
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent is not on a valid NavMesh.");
            return;
        }

        // Targeting logic based on tags
        Transform priorityTarget = GetPriorityTargetByTags();

        if (priorityTarget != null)
        {
            // Set the destination to the closest target within range
            navMeshAgent.SetDestination(priorityTarget.position);

            // Check if we're within attack range to deal damage
            float distanceToTarget = Vector3.Distance(transform.position, priorityTarget.position);

            if (distanceToTarget > stoppingDistance)
            {
                navMeshAgent.isStopped = false; // Continue moving
            }
            else
            {
                navMeshAgent.isStopped = true; // Stop when close enough

                // If close enough, try to deal damage
                if (distanceToTarget <= attackRange)
                {
                    TryDealDamage(priorityTarget);
                }
            }
        }
        else
        {
            navMeshAgent.isStopped = true; // Stop if no target in range
        }
    }

    // Method to get the closest target with any of the specified tags within the targeting range
    Transform GetPriorityTargetByTags()
    {
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (string tag in targetTags)
        {
            GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject potentialTarget in potentialTargets)
            {
                float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.transform.position);

                // Check if the target is within the specified range
                if (distanceToTarget <= targetingRange && distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = potentialTarget.transform;
                }
            }
        }

        return closestTarget; // Returns the closest target within range, or null if none
    }

    // Try to deal damage to the target if within range and if cooldown allows
    void TryDealDamage(Transform target)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Check if the target has a Damageable component
            var targetDamageable = target.GetComponent<StructureHealth>();

            if (targetDamageable != null)
            {
                targetDamageable.TakeDamage(damageAmount); // Deal damage to the target
                Debug.Log("Dealt " + damageAmount + " damage to " + target.name);
            }

            lastAttackTime = Time.time; // Reset attack cooldown timer
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Received Damage: " + damage);
        hp -= damage;
        UpdateHPDisplay();

        if (hp <= 0)
        {
            Die();
        }
    }

    void UpdateHPDisplay()
    {
        if (hpDisplay != null)
        {
            hpDisplay.text = hp.ToString() + "/" + maxHP.ToString();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died.");
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }
}
