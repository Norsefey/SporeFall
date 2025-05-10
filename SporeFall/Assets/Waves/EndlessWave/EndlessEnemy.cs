using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EndlessEnemy : BaseEnemy
{
    protected override void Awake()
    {
        // Get component references once
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        detectedColliders = new Collider[maxDetectedObjects];
    }
    public override void Initialize()
    {
        ResetState();
        StartCoroutine(PeriodicTargetDetection(4));
        // do not want movement while rising from from ground
        if (!isRising)
        {
            // Start behavior
            DetectTargets();
            SetRandomState();
        }
    }
    protected override void ResetState()
    {
        // Reset all state when object is reused from pool
        foreach (var att in attacks)
        {
            //Debug.LogWarning("Reseting Attack: " + att);
            att.ResetCooldown();
        }

        // Reset variables
        currentState = EnemyState.Idle;
        isAttacking = false;
        passedThreshold = false;
        targetingStructure = false;
        strafeDirectionRight = true;
        stateTimer = 0f;

        // Reset queues and collections
        recentDamage.Clear();

        // Reset health
        if (health != null)
        {
            health.ResetHealth();
        }

        // Reset NavMeshAgent
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.velocity = Vector3.zero;
            agent.stoppingDistance = stoppingDistance;
        }
    }
    protected override void Update()
    {
        UpdateStateTimer();

        if (!isAttacking)
        {
            UpdateCurrentState();
        }
    }
    protected override void UpdateStateTimer()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            SetRandomState();
        }
    }
    protected override void SetRandomState()
    {
        List<StateWeight> stateWeights = CalculateStateWeights();

        // Normalize weights
        float totalWeight = stateWeights.Sum(sw => sw.weight);
        if (totalWeight > 0)
        {
            float randomValue = Random.value * totalWeight;
            float currentSum = 0;

            foreach (var stateWeight in stateWeights)
            {
                currentSum += stateWeight.weight;
                //Debug.Log($"{stateWeight.state} State - Weight: {stateWeight.weight}");
                if (randomValue <= currentSum)
                {
                    //Debug.Log($"Entering {stateWeight.state} State - Weight: {stateWeight.weight}");
                    SetState(stateWeight.state);
                    return;
                }
            }
        }

        // Fallback to idle if something goes wrong
        SetState(EnemyState.Idle);
    }
    protected override List<StateWeight> CalculateStateWeights()
    {
        List<StateWeight> weights = new();
        float distanceToTarget = currentTarget ? Vector3.Distance(transform.position, currentTarget.position) : float.MaxValue;
        float recentDamageSum = CalculateRecentDamage();
        // Chase Priority
        float chaseWeight = CalculateChaseWeight(distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Chase, chaseWeight));
        // Attack Priority
        float attackWeight = CalculateAttackWeight(distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Attack, attackWeight));
        float strafeWeight = CalculateStrafeWeight(recentDamageSum, distanceToTarget);
        weights.Add(new StateWeight(EnemyState.Strafe, strafeWeight));

        // Idle is lowest priority
        weights.Add(new StateWeight(EnemyState.Idle, 0.01f));

        return weights;
    }
    protected override float CalculateChaseWeight(float distanceToTarget)
    {
        if (distanceToTarget > chasePriorityDistance)
            return 3f;
        else if (distanceToTarget > stoppingDistance)
            return 1.5f;
        return 0.1f;
    }
    protected override float CalculateAttackWeight(float distanceToTarget)
    {
        Attack bestAttack = ChooseBestAttack(distanceToTarget);
        if (bestAttack != null)
            return 2.5f;
        return 0.5f;
    }
    protected override float CalculateStrafeWeight(float recentDamage, float distanceToTarget)
    {
        float weight = 0.01f; // Base weight for strafing

        if (recentDamage > damagePriorityThreshold * 0.5f)
            weight += 1.5f;
        else if (recentDamage <= 0)
            return 0;

        return weight;
    }
    public override void SetState(EnemyState newState)
    {
        currentState = newState;

        // Set appropriate timer for the new state
        switch (currentState)
        {
            case EnemyState.Idle:
                stateTimer = Random.Range(2f, 4f);
                break;
            case EnemyState.Strafe:
                stateTimer = Random.Range(2f, 4f);
                CalculateStrafePosition();
                break;
            case EnemyState.Attack:
                stateTimer = Random.Range(5f, 8f);
                break;
            case EnemyState.Chase:
                DetectTargets();
                stateTimer = Random.Range(1f, 4f);
                break;
            default:
                stateTimer = Random.Range(2f, 4f);
                break;
        }

        if (animator != null)
            animator.SetInteger("State", (int)currentState);
    }
    protected override void UpdateCurrentState()
    {
        // for when current target is destroyed find a new target
        if (currentTarget == null)
        {
            DetectTargets();
        }
        else if (currentTarget.gameObject.activeSelf == false)
        {
            DetectTargets();
        }
        // alot of behavior relies on distance to current target
        float distanceToTarget = currentTarget != null ?
            Vector3.Distance(transform.position, currentTarget.position) : float.MaxValue;

        switch (currentState)
        {
            case EnemyState.Idle:
                // Debug.Log("Idling");
                UpdateIdleState();
                break;
            case EnemyState.Chase:
                // Debug.Log("Chasing");
                UpdateChaseState(distanceToTarget);
                break;
            case EnemyState.Attack:
                //Debug.Log("Attacking");
                UpdateAttackState(distanceToTarget);
                break;
            case EnemyState.Strafe:
                // Debug.Log("Strafing");
                UpdateStrafeState();
                break;
        }
    }
    protected override void UpdateIdleState()
    {
        agent.isStopped = true;
        // Possibly look at target or play idle animation
        if (currentTarget != null)
        {
            Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
        }
    }
    protected override void UpdateChaseState(float distanceToTarget)
    {
        if (passedThreshold)
            DetectTargets();

        if (currentTarget == null) return;

        Vector3 targetPosition;

        if (!targetingStructure)
        {
            // Get closest point on the target's collider
            Collider targetCollider = currentTarget.GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetPosition = targetCollider.ClosestPoint(transform.position);
                targetPosition.y = transform.position.y;

                // Update distance calculation using the closest point
                distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            }
            else
            {
                targetPosition = currentTarget.position;
            }
        }
        else
        {
            // Structures
            targetPosition = currentTarget.position;
        }

        // Check if we're outside the stopping distance
        if (distanceToTarget > stoppingDistance)
        {
            // Set up agent for movement
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
        else
        {
            // We're within attack range, transition to attack state
            agent.isStopped = true;
            SetState(EnemyState.Attack);
        }
    }
    protected override void UpdateAttackState(float distanceToTarget)
    {
        if (currentTarget == null) return;

        Vector3 targetPosition;

        if (!targetingStructure)
        {
            // Get closest point on the target's collider for consistent distance calculation
            Collider targetCollider = currentTarget.GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetPosition = targetCollider.ClosestPoint(transform.position);
                targetPosition.y = transform.position.y;
                distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            }
            else
            {
                targetPosition = currentTarget.position;
            }
        }
        else
        {
            targetPosition = currentTarget.position;
        }

        Attack bestAttack = ChooseBestAttack(distanceToTarget);
        if (bestAttack != null && currentTarget.gameObject.activeSelf)
        {
            // Calculate direction to the actual target position for rotation
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Keep the enemy level

            // Smoothly rotate towards the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5 * Time.deltaTime);

            StartCoroutine(bestAttack.ExecuteAttack(this, currentTarget));
            return;
        }
        else
        {
            //Debug.Log("Cannot Attack");
            SetRandomState(); // Choose new state if we can't attack
        }
    }
    protected override void CalculateStrafePosition()
    {
        if (currentTarget != null)
        {
            strafeDirectionRight = !strafeDirectionRight; // Alternate strafe direction
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 perpendicularDirection = strafeDirectionRight ?
                Vector3.Cross(directionToTarget, Vector3.up) :
                Vector3.Cross(Vector3.up, directionToTarget);

            Vector3 potentialStrafePosition = transform.position + perpendicularDirection * strafeRadius;

            if (NavMesh.SamplePosition(potentialStrafePosition, out NavMeshHit hit, strafeRadius, NavMesh.AllAreas))
            {
                strafeTarget = hit.position;
            }
        }
    }
    protected override void UpdateStrafeState()
    {
        if (currentTarget != null)
        {
            agent.stoppingDistance = stoppingDistance / 2;
            // Move to strafe position
            agent.isStopped = false;
            agent.SetDestination(strafeTarget);

            // Look at target while strafing
            Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

            // If we're close to the strafe target, calculate a new one
            if (Vector3.Distance(transform.position, strafeTarget) < 1f)
            {
                CalculateStrafePosition();
            }
        }
    }

    protected override Attack ChooseBestAttack(float distanceToTarget)
    {
        Attack bestAttack = null;
        float bestPriority = float.MinValue;

        foreach (Attack attack in attacks)
        {
            if (attack.CanUse(distanceToTarget))
            {
                float priority = attack.priority;
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestAttack = attack;
                }
            }
        }
        return bestAttack;
    }

    // Modified to work without TrainHandler dependency
    public override void DetectTargets()
    {
        int detectedCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectedColliders, targetsLayerMask);

        currentTarget = detectedColliders
        .Where(c => c != null && c.gameObject.activeSelf && priorityTags.Contains(c.tag))    // Filter by priority tags
        .Where(c => IsTargetAccessible(c.transform))              // Filter by NavMesh accessibility
        .OrderBy(c => GetPriorityIndex(c.tag))                   // Prioritize by tag order
        .ThenBy(c => Vector3.Distance(transform.position, c.transform.GetComponent<Collider>().ClosestPoint(transform.position))) // If same tag, choose closest
        .Select(c => c.transform)
        .FirstOrDefault();

        if (currentTarget != null)
        {
            targetingStructure = currentTarget.CompareTag("Structure");
        }
        else
        {
            targetingStructure = false;
        }
    }

    // Added method to allow EndlessWaveManager to set target explicitly
    public override void SetTarget(Transform target)
    {
        if (target != null)
        {
            currentTarget = target;
            targetingStructure = target.CompareTag("Structure");
        }
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();

        // Clean up any references
        currentTarget = null;

        // Reset any ongoing effects or states
        SetIsAttacking(false);
    }
    protected override void SpawnDeathVFX(Vector3 position, Quaternion rotation)
    {
        if (deathVFXPrefab != null && PoolManager.Instance != null)
        {
            // Get VFX from pool
            if (!PoolManager.Instance.vfxPool.TryGetValue(deathVFXPrefab, out VFXPool pool))
            {
                Debug.LogError($"No pool found for VFX prefab: {deathVFXPrefab.name}");
                return;
            }
            VFXPoolingBehavior vfx = pool.Get(position, rotation);
            vfx.Initialize(pool);
        }
    }
    public override void SetHealthMultiplier(float multiplier)
    {
        // Access the enemy's health component
        if (health != null)
        {
            // Store original max health if not already stored
            if (!health.HasStoredOriginalHealth())
            {
                health.StoreOriginalMaxHealth();
            }

            // Apply multiplier to max health
            health.SetMaxHealthWithMultiplier(multiplier);
        }
    }
}
