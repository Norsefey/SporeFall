using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Linq;

public class EnemyControls : MonoBehaviour
{
    public delegate void EnemyDeath();
    public event EnemyDeath OnEnemyDeath;
    // NavMeshAgent component
    private NavMeshAgent navMeshAgent;

    // Attack properties
    public float stoppingDistance = 2.0f;  // Distance at which the enemy stops moving and can attack
    public float attackRange = 2.5f;       // Distance at which the enemy can deal damage
    public float attackCoolDown = 2.0f;    // Time between attacks
    public float damageAmount = 5.0f;      // Damage dealt to the target
    private float lastAttackTime;

    // Range within which the enemy can target an object
    public float targetingRange = 15.0f;

    // Array to hold multiple valid target tags
    public string[] priorityTags; // e.g., "Player", "Ally"
    public LayerMask targetsLayerMask; // So we only detect what we need to
    public TrainHandler train;    // if nothing is in range will move to train

    private Transform currentTarget; // the current target it is moving to and will attack
    private float nextTargetUpdateTime = 0f; // check for potential targets in intervals
    public float updateTargetInterval = 0.5f;  // Time interval to update the target
    private Collider[] detectedColliders;      // Array to store detected colliders
    private int maxDetectedObjects = 10;        // Max number of objects the enemy can detect at once
    [Header("Drops")]
    [SerializeField] GameObject myceliaDropPrefab;
    [SerializeField] GameObject[] weaponDropPrefab;
    [SerializeField] private float dropChance = 20;
    private bool isTargetingTrain = false;

    // Movement sound variables
    [Header("Movement Sound")]
    public AudioClip movementSound; // Assign the movement sound clip in the inspector
    private AudioSource audioSource; // To play the sound
    private bool isMoving; // Track if the enemy is moving

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        detectedColliders = new Collider[maxDetectedObjects]; // Pre-allocate the array for detected objects
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component

        navMeshAgent.stoppingDistance = stoppingDistance;
        lastAttackTime = -attackCoolDown; // Allow immediate attack when starting
    }
    void Update()
    {
        // Update the target at intervals, but only if the current target is empty or out of range
        if (Time.time >= nextTargetUpdateTime && (currentTarget == null || !IsTargetInRange(currentTarget)))
        {
            DetectTargets();
            nextTargetUpdateTime = Time.time + updateTargetInterval;
        }else if(currentTarget != null)
        {
            // Check if we're within attack range to deal damage
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= stoppingDistance)
            {
                // If close enough, try to deal damage
                if (distanceToTarget <= attackRange)
                {
                    TryDealDamage(currentTarget);
                }
            }
        }
        
        // Move towards the current target
        MoveTowardsTarget();

 /*       // Targeting logic based on tags
        Transform priorityTarget = GetPriorityTarget();

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
        }*/
    }
    // Detect targets in the surrounding area and prioritize based on tags or closest proximity
    void DetectTargets()
    {
        int detectedCount = Physics.OverlapSphereNonAlloc(transform.position, targetingRange, detectedColliders, targetsLayerMask);

        // Sort by priority target based on tag, and then by closest distance
        currentTarget = detectedColliders
           .Where(c => c != null && priorityTags.Contains(c.tag))    // Filter by priority tags
           .OrderBy(c => GetPriorityIndex(c.tag))                    // Prioritize by tag order
           .ThenBy(c => Vector3.Distance(transform.position, c.transform.position)) // If same tag, choose closest
           .Select(c => c.transform)
           .FirstOrDefault();

        if (currentTarget == null && train != null)
        {
            int index = Random.Range(0, train.damagePoint.Length);
            currentTarget = train.damagePoint[index];
            isTargetingTrain = true;
        }
        else
        {
            isTargetingTrain = false;
        }

    }
    // Get the priority index of the tag, lower numbers mean higher priority
    int GetPriorityIndex(string tag)
    {
        for (int i = 0; i < priorityTags.Length; i++)
        {
            if (priorityTags[i] == tag)
            {
                return i; // Priority is determined by index in the array
            }
        }
        return priorityTags.Length; // If the tag isn't a priority, return a lower priority index
    }
    // Check if the current target is still within range
    bool IsTargetInRange(Transform target)
    {
        return target != null && Vector3.Distance(transform.position, target.position) <= targetingRange;
    }
    // Move towards the selected target using NavMeshAgent
    void MoveTowardsTarget()
    {
        // check if we have a target and agent is activated
        if (currentTarget != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(currentTarget.position); // Set the destination of the NavMeshAgent

            // Check if the enemy is moving
            if (navMeshAgent.velocity.magnitude > 0)
            {
                if (!isMoving)
                {
                    isMoving = true;
                    PlayMovementSound();
                }
            }
            else
            {
                isMoving = false; // Reset the moving state when not moving
            }
        }
    }
    void PlayMovementSound()
    {
        if (audioSource != null && movementSound != null)
        {
            audioSource.PlayOneShot(movementSound); // Play the movement sound
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }

    // Method to get the closest target with any of the specified tags within the targeting range
    /*   Transform GetPriorityTarget()
       {
           Transform closestTarget = null;
           float closestDistance = Mathf.Infinity;
           Collider[] potentialTargets = Physics.OverlapSphere(transform.position, targetingRange, targets);
           if (potentialTargets == null)
           {
               int index = Random.Range(0, train.damagePoint.Length);
               Debug.Log("Going to train");
               return train.damagePoint[index];
           }
           else
           {
               // nice unity function that takes in an array and sorts it based on parameters, then chooses the first item
               closestTarget = potentialTargets
                  .Where(c => c.CompareTag("Player") || c.CompareTag("Structure"))
                  .OrderBy(c => GetPriorityIndex(c.tag))                // Prioritize by tag order
                  .ThenBy(c => Vector3.Distance(transform.position, c.transform.position)) // If same tag, choose closest
                  .Select(c => c.transform)
                  .FirstOrDefault();

               Debug.Log(closestTarget);
               return closestTarget; // Returns the closest target within range, or null if none
           }



           *//*  foreach (Collider potentialTarget in potentialTargets)
             {
                 float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.transform.position);

                 // Check if the target is within the specified range
                 if (distanceToTarget <= targetingRange && distanceToTarget < closestDistance)
                 {
                     closestDistance = distanceToTarget;
                     closestTarget = potentialTarget.transform;
                 }
             }*//*

       }*/


    // Try to deal damage to the target if within range and if cooldown allows
    void TryDealDamage(Transform target)
    {
        if (Time.time >= lastAttackTime + attackCoolDown)
        {
            // since the script with Hp is in a child with no collider, we need to manually get the child
            // Moved turret controls to be the first child
            if (isTargetingTrain)
            {
                train.trainHP.TakeDamage(damageAmount);
            }
            else 
            {
                if (target.gameObject.TryGetComponent<Damageable>(out var hp))
                {
                    hp.TakeDamage(damageAmount);  // Apply 100 damage to the enemy
                }
            }

            lastAttackTime = Time.time; // Reset attack cooldown timer
        }
    }

    void Die()
    {
        OnEnemyDeath?.Invoke();
        
        var mycelia = Instantiate(myceliaDropPrefab, transform.position, Quaternion.identity).GetComponent<MyceliaPickup>();
        mycelia.Setup();
       
        if (train != null)
        {
            // so we can remove it if player doesn't pick it up, set as child of drops holder
            mycelia.transform.SetParent(train.dropsHolder, true);
        }

        if (weaponDropPrefab.Length != 0)
        {
            float randomChance = Random.Range(0, 100);
            if (randomChance <= dropChance)
            {
                int dropIndex = Random.Range(0, weaponDropPrefab.Length);
                var weapon = Instantiate(weaponDropPrefab[dropIndex], transform.position, Quaternion.identity);
                // so we can remove it if player doesn't pick it up, set as child of drops holder
                weapon.transform.SetParent(train.dropsHolder, true);
            }
        }
        Destroy(gameObject);
    }
}
