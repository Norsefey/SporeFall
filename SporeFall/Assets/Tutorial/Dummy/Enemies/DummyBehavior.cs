using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class DummyBehavior : BaseEnemy
{
    [Header("Wander Behavior")]
    [SerializeField] protected float wanderRadius = 15f;
    [SerializeField] protected float minWanderTime = 5f;
    [SerializeField] protected float maxWanderTime = 10f;
    [SerializeField] protected float idleChance = 0.3f; // Chance to idle when no target is present
    private Vector3 wanderTarget;
    private float wanderDestinationThreshold = 2f;

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
                if (randomValue <= currentSum)
                {
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
        float recentDamageSum = CalculateRecentDamage();

        // If we have a target, consider combat states
        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            // Chase Priority
            float chaseWeight = CalculateChaseWeight(distanceToTarget);
            weights.Add(new StateWeight(EnemyState.Chase, chaseWeight));

            // Attack Priority
            float attackWeight = CalculateAttackWeight(distanceToTarget);
            weights.Add(new StateWeight(EnemyState.Attack, attackWeight));

            float strafeWeight = CalculateStrafeWeight(recentDamageSum, distanceToTarget);
            weights.Add(new StateWeight(EnemyState.Strafe, strafeWeight));

            // Add small chance to wander even with target
            weights.Add(new StateWeight(EnemyState.Wander, 0.05f));
        }
        else
        {
            // No target - decide between idle and wandering
            weights.Add(new StateWeight(EnemyState.Idle, idleChance));
            weights.Add(new StateWeight(EnemyState.Wander, 1 - idleChance));
        }

        // Idle is lowest priority but always an option
        weights.Add(new StateWeight(EnemyState.Idle, 0.01f));

        return weights;
    }
    public override void SetState(EnemyState newState)
    {
        currentState = newState;

        // Set appropriate timer for the new state
        switch (currentState)
        {
            case EnemyState.Idle:
                stateTimer = Random.Range(2f, 4f);
                agent.isStopped = true;
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

            case EnemyState.Wander:
                stateTimer = Random.Range(minWanderTime, maxWanderTime);
                FindWanderDestination();
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
        // Periodically check for targets
        if (currentTarget == null || !currentTarget.gameObject.activeSelf)
        {
            DetectTargets();
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;

            case EnemyState.Wander:
                UpdateWanderState();
                break;

            case EnemyState.Chase:
                if (currentTarget != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                    UpdateChaseState(distanceToTarget);
                }
                else
                {
                    // No target available, switch to wandering or idle
                    if (Random.value < idleChance)
                        SetState(EnemyState.Idle);
                    else
                        SetState(EnemyState.Wander);
                }
                break;

            case EnemyState.Attack:
                if (currentTarget != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                    UpdateAttackState(distanceToTarget);
                }
                else
                {
                    // No target available, switch to wandering or idle
                    if (Random.value < idleChance)
                        SetState(EnemyState.Idle);
                    else
                        SetState(EnemyState.Wander);
                }
                break;

            case EnemyState.Strafe:
                UpdateStrafeState();
                break;
        }
    }
    protected override void UpdateIdleState()
    {
        agent.isStopped = true;

        // If there's a target, look at it
        if (currentTarget != null)
        {
            Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
        }
    }

    protected virtual void FindWanderDestination()
    {
        // Find a random point on the NavMesh within wanderRadius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            agent.isStopped = false;
            agent.SetDestination(wanderTarget);
        }
        else
        {
            // If we couldn't find a valid position, try again with a smaller radius
            randomDirection = Random.insideUnitSphere * (wanderRadius * 0.5f);
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius * 0.5f, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
                agent.isStopped = false;
                agent.SetDestination(wanderTarget);
            }
            else
            {
                // If still no valid position, just idle
                SetState(EnemyState.Idle);
            }
        }
    }

    protected virtual void UpdateWanderState()
    {
        if (agent.remainingDistance <= wanderDestinationThreshold)
        {
            // Reached destination, pause briefly
            agent.isStopped = true;

            // Choose a new wander destination after a short pause
            if (stateTimer <= 1.0f)
            {
                FindWanderDestination();
            }
        }

        // Check if we found a target during wandering
        if (currentTarget != null && Random.value > 0.7f) // 30% chance per frame to notice the target
        {
            SetState(EnemyState.Chase);
        }
    }
    protected override void UpdateChaseState(float distanceToTarget)
    {
        if (passedThreshold)
            DetectTargets();

        Vector3 targetPosition;

        if (!targetingStructure)
        {
            // Get closest point on the target's collider
            targetPosition = currentTarget.GetComponent<Collider>().ClosestPoint(transform.position);
            targetPosition.y = transform.position.y;

            // Update distance calculation using the closest point
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
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
        Vector3 targetPosition;

        if (!targetingStructure)
        {
            // Get closest point on the target's collider for consistent distance calculation
            targetPosition = currentTarget.GetComponent<Collider>().ClosestPoint(transform.position);
            targetPosition.y = transform.position.y;
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
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
            SetRandomState(); // Choose new state if we can't attack
        }
    }
    protected virtual void UpdateRetreatState()
    {
        if (currentTarget != null)
        {
            agent.stoppingDistance = stoppingDistance / 2;

            Vector3 retreatDirection = transform.position - currentTarget.position;
            Vector3 retreatPosition = transform.position + retreatDirection.normalized * retreatDistance;

            if (NavMesh.SamplePosition(retreatPosition, out NavMeshHit hit, retreatDistance, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            // No target to retreat from, switch to wander or idle
            if (Random.value < idleChance)
                SetState(EnemyState.Idle);
            else
                SetState(EnemyState.Wander);
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
        else
        {
            // No target to strafe around, switch to wander or idle
            if (Random.value < idleChance)
                SetState(EnemyState.Idle);
            else
                SetState(EnemyState.Wander);
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
        else
        {
            // No target to strafe around, switch to wander or idle
            if (Random.value < idleChance)
                SetState(EnemyState.Idle);
            else
                SetState(EnemyState.Wander);
        }
    }
    public override void DetectTargets()
    {
        int detectedCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectedColliders, targetsLayerMask);

        currentTarget = detectedColliders
        .Where(c => c != null && c.gameObject.activeSelf && priorityTags.Contains(c.tag))
        .Where(c => IsTargetAccessible(c.transform))
        .OrderBy(c => GetPriorityIndex(c.tag))
        .ThenBy(c => Vector3.Distance(transform.position, c.transform.GetComponent<Collider>().ClosestPoint(transform.position)))
        .Select(c => c.transform)
        .FirstOrDefault();

        // Only set targeting structure if we actually have a target
        if (currentTarget != null)
        {
            targetingStructure = false; // Reset this flag

            if (currentTarget.CompareTag("Train") ||
                currentTarget.CompareTag("Payload") || currentTarget.CompareTag("Structure"))
            {
                targetingStructure = true;
            }
        }

        // Check for train target if no priority target was found
        if (train != null && (currentTarget == null || currentTarget.CompareTag("Train")))
        {
            targetingStructure = true;
            if (train.Payload != null)
                currentTarget = train.Payload.transform;
            else if (trainWall != null)
                currentTarget = trainWall;
        }
    }
}
